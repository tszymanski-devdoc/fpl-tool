using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Domain;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.SyncPoints;

internal sealed class SyncGameweekPointsHandler : IRequestHandler<SyncGameweekPointsCommand, Result>
{
    private readonly PicksDbContext _dbContext;
    private readonly IFplApiService _fplApiService;

    public SyncGameweekPointsHandler(PicksDbContext dbContext, IFplApiService fplApiService)
    {
        _dbContext = dbContext;
        _fplApiService = fplApiService;
    }

    public async Task<Result> Handle(SyncGameweekPointsCommand request, CancellationToken cancellationToken)
    {
        var liveEvent = await _fplApiService.GetLiveEventAsync(request.GameweekId, cancellationToken);
        var pointsMap = liveEvent.Elements.ToDictionary(e => e.Id, e => e.Stats.TotalPoints);

        var picks = await _dbContext.CaptainPicks
            .Where(p => p.GameweekId == request.GameweekId)
            .ToListAsync(cancellationToken);

        foreach (var pick in picks)
        {
            if (pointsMap.TryGetValue(pick.FplPlayerId, out var points))
                pick.SetPointsScored(points);
        }

        var sync = await _dbContext.GameweekPointsSyncs
            .FirstOrDefaultAsync(s => s.GameweekId == request.GameweekId, cancellationToken);

        if (sync is null)
        {
            sync = GameweekPointsSync.Create(request.GameweekId);
            _dbContext.GameweekPointsSyncs.Add(sync);
        }
        sync.MarkComplete();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
