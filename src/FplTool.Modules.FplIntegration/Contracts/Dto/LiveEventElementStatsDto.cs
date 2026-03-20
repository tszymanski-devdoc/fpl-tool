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

    [JsonPropertyName("clean_sheets")]
    public int CleanSheets { get; init; }

    [JsonPropertyName("bonus")]
    public int Bonus { get; init; }

    [JsonPropertyName("yellow_cards")]
    public int YellowCards { get; init; }

    [JsonPropertyName("red_cards")]
    public int RedCards { get; init; }

    [JsonPropertyName("saves")]
    public int Saves { get; init; }

    [JsonPropertyName("penalties_saved")]
    public int PenaltiesSaved { get; init; }

    [JsonPropertyName("penalties_missed")]
    public int PenaltiesMissed { get; init; }

    [JsonPropertyName("own_goals")]
    public int OwnGoals { get; init; }
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
