namespace FplTool.Modules.Picks.Contracts;

public sealed record PlayerSummaryDto(
    int FplPlayerId,
    string WebName,
    string FirstName,
    string LastName,
    int TeamId,
    string TeamName,
    string TeamShortName,
    int Position,
    string PositionName,
    int TotalPoints,
    int NowCost
);

public sealed record AllPlayersDto(
    int GameweekId,
    string GameweekName,
    DateTime Deadline,
    IReadOnlyList<PlayerSummaryDto> Players
);
