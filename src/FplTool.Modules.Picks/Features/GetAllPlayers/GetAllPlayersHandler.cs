using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.GetAllPlayers;

internal sealed class GetAllPlayersHandler : IRequestHandler<GetAllPlayersQuery, Result<AllPlayersDto>>
{
    private static readonly Dictionary<int, string> PositionNames = new()
    {
        { 1, "GK" }, { 2, "DEF" }, { 3, "MID" }, { 4, "FWD" }
    };

    private readonly IFplApiService _fplApiService;
    private readonly PicksDbContext _db;

    public GetAllPlayersHandler(IFplApiService fplApiService, PicksDbContext db)
    {
        _fplApiService = fplApiService;
        _db = db;
    }

    public async Task<Result<AllPlayersDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var upcomingGw = bootstrap.Events.FirstOrDefault(e => e.IsNext)
                         ?? bootstrap.Events.FirstOrDefault(e => e.IsCurrent);

        if (upcomingGw is null)
            return Result.Failure<AllPlayersDto>(Error.Business("NO_ACTIVE_GAMEWEEK", "No active or upcoming gameweek found."));

        // Fetch fixtures and captain counts in parallel
        var fixturesTask = _fplApiService.GetFixturesAsync(upcomingGw.Id, cancellationToken);
        var captainCountsTask = _db.CaptainPicks
            .Where(p => p.GameweekId == upcomingGw.Id)
            .GroupBy(p => p.FplPlayerId)
            .Select(g => new { PlayerId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(fixturesTask, captainCountsTask);

        var fixtures = await fixturesTask;
        var captainPickCounts = (await captainCountsTask).ToDictionary(x => x.PlayerId, x => x.Count);

        var teamMap = bootstrap.Teams.ToDictionary(t => t.Id);

        // Build lookup: teamId -> (opponentShortName, isHome)
        var fixtureByTeam = new Dictionary<int, (string OpponentShortName, bool IsHome)>();
        foreach (var f in fixtures)
        {
            if (teamMap.TryGetValue(f.TeamH, out var homeTeam) && teamMap.TryGetValue(f.TeamA, out var awayTeam))
            {
                fixtureByTeam[f.TeamH] = (awayTeam.ShortName, true);
                fixtureByTeam[f.TeamA] = (homeTeam.ShortName, false);
            }
        }

        IEnumerable<PlayerSummaryDto> players = bootstrap.Elements
            .Where(p => teamMap.ContainsKey(p.TeamId))
            .Select(p =>
            {
                var team = teamMap[p.TeamId];
                var positionName = PositionNames.GetValueOrDefault(p.ElementType, "?");
                fixtureByTeam.TryGetValue(p.TeamId, out var fixture);
                return new PlayerSummaryDto(
                    p.Id,
                    p.WebName,
                    p.FirstName,
                    p.SecondName,
                    p.TeamId,
                    team.Name,
                    team.ShortName,
                    p.ElementType,
                    positionName,
                    p.TotalPoints,
                    p.NowCost,
                    p.Photo.Replace(".jpg", ""),
                    fixture == default ? null : fixture.OpponentShortName,
                    fixture == default ? null : fixture.IsHome
                );
            });

        if (request.Position.HasValue)
            players = players.Where(p => p.Position == request.Position.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            players = players.Where(p =>
                p.WebName.ToLowerInvariant().Contains(q) ||
                p.FirstName.ToLowerInvariant().Contains(q) ||
                p.LastName.ToLowerInvariant().Contains(q));
        }

        players = request.SortBy switch
        {
            "name" => players.OrderBy(p => p.WebName),
            "position" => players.OrderBy(p => p.Position).ThenByDescending(p => p.TotalPoints),
            _ => players.OrderByDescending(p => p.TotalPoints)
        };

        var allMatched = players.ToList();
        var totalCount = allMatched.Count;
        var pageSize = Math.Max(1, request.PageSize);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var page = Math.Clamp(request.Page, 1, Math.Max(1, totalPages));
        var paged = allMatched.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result.Success(new AllPlayersDto(
            upcomingGw.Id,
            upcomingGw.Name,
            upcomingGw.DeadlineTime,
            paged,
            totalCount,
            page,
            pageSize,
            totalPages,
            captainPickCounts
        ));
    }
}
