using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class LiveEventElementStatsDto
{
    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; init; }

    [JsonPropertyName("goals_scored")]
    public int GoalsScored { get; init; }

    [JsonPropertyName("assists")]
    public int Assists { get; init; }
}

public sealed class LiveEventElementDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("stats")]
    public LiveEventElementStatsDto Stats { get; init; } = new();
}

public sealed class LiveEventDto
{
    [JsonPropertyName("elements")]
    public List<LiveEventElementDto> Elements { get; init; } = [];
}
