using FplTool.Modules.Auth.Infrastructure;
using FplTool.Modules.Leaderboard.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Leaderboard.Features.GetLeaderboard;

internal sealed class GetLeaderboardHandler : IRequestHandler<GetLeaderboardQuery, Result<List<LeaderboardEntryDto>>>
{
    private readonly PicksDbContext _picksDb;
    private readonly AuthDbContext _authDb;

    public GetLeaderboardHandler(PicksDbContext picksDb, AuthDbContext authDb)
    {
        _picksDb = picksDb;
        _authDb = authDb;
    }

    public async Task<Result<List<LeaderboardEntryDto>>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var picksQuery = _picksDb.CaptainPicks.AsNoTracking();

        if (request.GameweekId.HasValue)
            picksQuery = picksQuery.Where(p => p.GameweekId == request.GameweekId.Value);

        var grouped = await picksQuery
            .Where(p => p.PointsScored.HasValue)
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalPoints = g.Sum(p => p.PointsScored!.Value),
                PicksCount = g.Count()
            })
            .OrderByDescending(g => g.TotalPoints)
            .ToListAsync(cancellationToken);

        var userIds = grouped.Select(g => g.UserId).ToList();

        var users = await _authDb.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        var entries = grouped
            .Select((g, index) => new LeaderboardEntryDto(
                Rank: index + 1,
                UserId: g.UserId,
                TeamName: users.TryGetValue(g.UserId, out var name) ? name : "Unknown",
                TotalPoints: g.TotalPoints,
                PicksCount: g.PicksCount
            ))
            .ToList();

        return Result.Success(entries);
    }
}
