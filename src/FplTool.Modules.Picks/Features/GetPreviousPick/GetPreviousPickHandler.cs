using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.GetPreviousPick;

internal sealed class GetPreviousPickHandler : IRequestHandler<GetPreviousPickQuery, Result<PreviousPickResultDto?>>
{
    private static readonly Dictionary<int, string> PositionNames = new()
    {
        { 1, "GK" }, { 2, "DEF" }, { 3, "MID" }, { 4, "FWD" }
    };

    private readonly PicksDbContext _dbContext;
    private readonly IFplApiService _fplApiService;

    public GetPreviousPickHandler(PicksDbContext dbContext, IFplApiService fplApiService)
    {
        _dbContext = dbContext;
        _fplApiService = fplApiService;
    }

    public async Task<Result<PreviousPickResultDto?>> Handle(GetPreviousPickQuery request, CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var currentGw = bootstrap.Events.FirstOrDefault(e => e.IsCurrent)
                        ?? bootstrap.Events.FirstOrDefault(e => e.IsNext);

        if (currentGw is null)
            return Result.Success<PreviousPickResultDto?>(null);

        var previousGwId = currentGw.Id - 1;
        if (previousGwId < 1)
            return Result.Success<PreviousPickResultDto?>(null);

        var previousGwName = bootstrap.Events.FirstOrDefault(e => e.Id == previousGwId)?.Name
                             ?? $"Gameweek {previousGwId}";

        var pick = await _dbContext.CaptainPicks
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId && p.GameweekId == previousGwId)
            .FirstOrDefaultAsync(cancellationToken);

        if (pick is null)
            return Result.Success<PreviousPickResultDto?>(new PreviousPickResultDto(previousGwId, previousGwName, null));

        var liveEvent = await _fplApiService.GetLiveEventAsync(previousGwId, cancellationToken);

        var playerMap = bootstrap.Elements.ToDictionary(p => p.Id);
        var teamMap = bootstrap.Teams.ToDictionary(t => t.Id);

        playerMap.TryGetValue(pick.FplPlayerId, out var player);
        var team = player is not null && teamMap.TryGetValue(player.TeamId, out var t) ? t : null;

        var photoCode = player?.Photo.Replace(".jpg", "");
        var teamShortName = team?.ShortName ?? "?";
        var positionName = player is not null
            ? PositionNames.GetValueOrDefault(player.ElementType, "?")
            : "?";

        var liveElement = liveEvent.Elements.FirstOrDefault(e => e.Id == pick.FplPlayerId);
        var stats = liveElement?.Stats;

        var playerStats = new PlayerStatsDto(
            Minutes: stats?.Minutes ?? 0,
            GoalsScored: stats?.GoalsScored ?? 0,
            Assists: stats?.Assists ?? 0,
            CleanSheets: stats?.CleanSheets ?? 0,
            Bonus: stats?.Bonus ?? 0,
            YellowCards: stats?.YellowCards ?? 0,
            RedCards: stats?.RedCards ?? 0,
            Saves: stats?.Saves ?? 0,
            PenaltiesSaved: stats?.PenaltiesSaved ?? 0,
            PenaltiesMissed: stats?.PenaltiesMissed ?? 0,
            OwnGoals: stats?.OwnGoals ?? 0
        );

        var pickPoints = pick.PointsScored ?? stats?.TotalPoints;

        var pickStats = new PreviousPickStatsDto(
            FplPlayerId: pick.FplPlayerId,
            PlayerWebName: pick.PlayerWebName,
            PhotoCode: string.IsNullOrEmpty(photoCode) ? null : photoCode,
            TeamShortName: teamShortName,
            PositionName: positionName,
            PointsScored: pickPoints,
            Stats: playerStats
        );

        return Result.Success<PreviousPickResultDto?>(new PreviousPickResultDto(previousGwId, previousGwName, pickStats));
    }
}
