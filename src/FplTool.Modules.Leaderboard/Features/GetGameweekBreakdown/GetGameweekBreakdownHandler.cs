using FplTool.Modules.Auth.Infrastructure;
using FplTool.Modules.Leaderboard.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Leaderboard.Features.GetGameweekBreakdown;

internal sealed class GetGameweekBreakdownHandler : IRequestHandler<GetGameweekBreakdownQuery, Result<List<GameweekBreakdownDto>>>
{
    private readonly PicksDbContext _picksDb;
    private readonly AuthDbContext _authDb;

    public GetGameweekBreakdownHandler(PicksDbContext picksDb, AuthDbContext authDb)
    {
        _picksDb = picksDb;
        _authDb = authDb;
    }

    public async Task<Result<List<GameweekBreakdownDto>>> Handle(GetGameweekBreakdownQuery request, CancellationToken cancellationToken)
    {
        var picks = await _picksDb.CaptainPicks
            .AsNoTracking()
            .Where(p => p.GameweekId == request.GameweekId)
            .OrderByDescending(p => p.PointsScored)
            .ToListAsync(cancellationToken);

        var userIds = picks.Select(p => p.UserId).Distinct().ToList();

        var users = await _authDb.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        var breakdown = picks
            .Select(p => new GameweekBreakdownDto(
                GameweekId: p.GameweekId,
                UserId: p.UserId,
                TeamName: users.TryGetValue(p.UserId, out var name) ? name : "Unknown",
                FplPlayerId: p.FplPlayerId,
                PlayerWebName: p.PlayerWebName,
                PointsScored: p.PointsScored
            ))
            .ToList();

        return Result.Success(breakdown);
    }
}
