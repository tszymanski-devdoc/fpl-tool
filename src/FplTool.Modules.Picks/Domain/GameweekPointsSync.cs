namespace FplTool.Modules.Picks.Domain;

public sealed class GameweekPointsSync
{
    public int GameweekId { get; private set; }
    public DateTime? LastSyncedUtc { get; private set; }
    public bool IsComplete { get; private set; }

    private GameweekPointsSync() { }

    public static GameweekPointsSync Create(int gameweekId)
    {
        return new GameweekPointsSync
        {
            GameweekId = gameweekId,
            LastSyncedUtc = null,
            IsComplete = false
        };
    }

    public void MarkComplete()
    {
        IsComplete = true;
        LastSyncedUtc = DateTime.UtcNow;
    }
}
