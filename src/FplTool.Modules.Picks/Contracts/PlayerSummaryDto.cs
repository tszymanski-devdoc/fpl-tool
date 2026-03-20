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
    int NowCost,
    string PhotoCode,
    string? OpponentTeamShortName,
    bool? IsHome
);

public sealed record AllPlayersDto(
    int GameweekId,
    string GameweekName,
    DateTime Deadline,
    IReadOnlyList<PlayerSummaryDto> Players,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyDictionary<int, int> CaptainPickCounts
);
