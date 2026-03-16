using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.GetCurrentPick;

internal sealed class GetCurrentPickHandler : IRequestHandler<GetCurrentPickQuery, Result<CaptainPickDto?>>
{
    private readonly PicksDbContext _dbContext;

    public GetCurrentPickHandler(PicksDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<CaptainPickDto?>> Handle(GetCurrentPickQuery request, CancellationToken cancellationToken)
    {
        var pick = await _dbContext.CaptainPicks
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId && p.GameweekId == request.GameweekId)
            .Select(p => new CaptainPickDto(p.Id, p.GameweekId, p.FplPlayerId, p.PlayerWebName, p.PointsScored, p.PickedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(pick);
    }
}
