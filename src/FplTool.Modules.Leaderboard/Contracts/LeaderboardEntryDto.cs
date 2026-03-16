namespace FplTool.Modules.Leaderboard.Contracts;

public sealed record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string TeamName,
    int TotalPoints,
    int PicksCount
);

public sealed record GameweekBreakdownDto(
    int GameweekId,
    Guid UserId,
    string TeamName,
    int FplPlayerId,
    string PlayerWebName,
    int? PointsScored
);
