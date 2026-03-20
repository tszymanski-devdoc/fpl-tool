namespace FplTool.Modules.Picks.Contracts;

public sealed record PlayerStatsDto(
    int Minutes,
    int GoalsScored,
    int Assists,
    int CleanSheets,
    int Bonus,
    int YellowCards,
    int RedCards,
    int Saves,
    int PenaltiesSaved,
    int PenaltiesMissed,
    int OwnGoals
);

public sealed record PreviousPickStatsDto(
    int FplPlayerId,
    string PlayerWebName,
    string? PhotoCode,
    string TeamShortName,
    string PositionName,
    int? PointsScored,
    PlayerStatsDto Stats
);

public sealed record PreviousPickResultDto(
    int GameweekId,
    string GameweekName,
    PreviousPickStatsDto? Pick  // null = no pick was made that GW
);
