using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Features.SyncPoints;
using FplTool.Modules.Picks.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FplTool.Modules.Picks.Services;

internal sealed class PointsSyncService : IPointsSyncService
{
    private readonly IFplApiService _fplApiService;
    private readonly IMediator _mediator;
    private readonly PicksDbContext _picksDb;
    private readonly ILogger<PointsSyncService> _logger;

    public PointsSyncService(
        IFplApiService fplApiService,
        IMediator mediator,
        PicksDbContext picksDb,
        ILogger<PointsSyncService> logger)
    {
        _fplApiService = fplApiService;
        _mediator = mediator;
        _picksDb = picksDb;
        _logger = logger;
    }

    public async Task<IReadOnlyList<int>> SyncPendingGameweeksAsync(CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var finishedGwIds = bootstrap.Events
            .Where(e => e.Finished)
            .Select(e => e.Id)
            .ToHashSet();

        var currentGwIds = bootstrap.Events
            .Where(e => e.IsCurrent)
            .Select(e => e.Id)
            .ToHashSet();

        var alreadySyncedGwIds = (await _picksDb.GameweekPointsSyncs
            .Where(s => s.IsComplete)
            .Select(s => s.GameweekId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        // Always include the current (in-progress) GW so live points are updated each tick
        var gwsToSync = finishedGwIds.Except(alreadySyncedGwIds)
            .Union(currentGwIds)
            .ToList();

        foreach (var gwId in gwsToSync)
        {
            _logger.LogInformation("Syncing points for gameweek {GameweekId}", gwId);
            await _mediator.Send(new SyncGameweekPointsCommand(gwId, IsFinished: finishedGwIds.Contains(gwId)), cancellationToken);
            _logger.LogInformation("Points synced for gameweek {GameweekId}", gwId);
        }

        return gwsToSync;
    }
}
