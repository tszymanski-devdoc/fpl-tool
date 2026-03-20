namespace FplTool.Modules.Picks.Services;

public interface IPointsSyncService
{
    /// <summary>
    /// Finds all finished gameweeks that have not yet been synced and syncs them.
    /// Returns the IDs of gameweeks that were synced in this call.
    /// </summary>
    Task<IReadOnlyList<int>> SyncPendingGameweeksAsync(CancellationToken cancellationToken);
}
