namespace FplTool.Modules.Picks.Domain;

public sealed class CaptainPick
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int GameweekId { get; private set; }
    public int FplPlayerId { get; private set; }
    public string PlayerWebName { get; private set; } = string.Empty;
    public int? PointsScored { get; private set; }
    public DateTime PickedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CaptainPick() { }

    public static CaptainPick Create(Guid userId, int gameweekId, int fplPlayerId, string playerWebName)
    {
        return new CaptainPick
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameweekId = gameweekId,
            FplPlayerId = fplPlayerId,
            PlayerWebName = playerWebName,
            PickedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdatePlayer(int fplPlayerId, string playerWebName)
    {
        FplPlayerId = fplPlayerId;
        PlayerWebName = playerWebName;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetPointsScored(int points)
    {
        PointsScored = points;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
