namespace FplTool.Modules.Picks.Contracts;

public sealed record CaptainPickDto(
    Guid Id,
    int GameweekId,
    int FplPlayerId,
    string PlayerWebName,
    int? PointsScored,
    DateTime PickedAtUtc
);

public sealed record SquadDto(
    int GameweekId,
    string GameweekName,
    DateTime Deadline,
    IReadOnlyList<SquadPlayerDto> Players
);

public sealed record SquadPlayerDto(
    int FplPlayerId,
    string WebName,
    int TeamId,
    int ElementType
);
