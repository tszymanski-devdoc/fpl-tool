using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class GameweekDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("deadline_time")]
    public DateTime DeadlineTime { get; init; }

    [JsonPropertyName("is_current")]
    public bool IsCurrent { get; init; }

    [JsonPropertyName("is_next")]
    public bool IsNext { get; init; }

    [JsonPropertyName("finished")]
    public bool Finished { get; init; }

    [JsonPropertyName("average_entry_score")]
    public int AverageEntryScore { get; init; }
}
