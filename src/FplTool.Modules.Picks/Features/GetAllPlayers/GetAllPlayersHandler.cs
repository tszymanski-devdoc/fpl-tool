using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetAllPlayers;

internal sealed class GetAllPlayersHandler : IRequestHandler<GetAllPlayersQuery, Result<AllPlayersDto>>
{
    private static readonly Dictionary<int, string> PositionNames = new()
    {
        { 1, "GK" }, { 2, "DEF" }, { 3, "MID" }, { 4, "FWD" }
    };

    private readonly IFplApiService _fplApiService;

    public GetAllPlayersHandler(IFplApiService fplApiService)
    {
        _fplApiService = fplApiService;
    }

    public async Task<Result<AllPlayersDto>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var upcomingGw = bootstrap.Events.FirstOrDefault(e => e.IsNext)
                         ?? bootstrap.Events.FirstOrDefault(e => e.IsCurrent);

        if (upcomingGw is null)
            return Result.Failure<AllPlayersDto>(Error.Business("NO_ACTIVE_GAMEWEEK", "No active or upcoming gameweek found."));

        var teamMap = bootstrap.Teams.ToDictionary(t => t.Id);

        IEnumerable<PlayerSummaryDto> players = bootstrap.Elements
            .Where(p => teamMap.ContainsKey(p.TeamId))
            .Select(p =>
            {
                var team = teamMap[p.TeamId];
                var positionName = PositionNames.GetValueOrDefault(p.ElementType, "?");
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
                    p.Photo.Replace(".jpg", "")
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
            totalPages
        ));
    }
}
