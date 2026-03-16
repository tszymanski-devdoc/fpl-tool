using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class PlayerDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("web_name")]
    public string WebName { get; init; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("second_name")]
    public string SecondName { get; init; } = string.Empty;

    [JsonPropertyName("team")]
    public int TeamId { get; init; }

    [JsonPropertyName("element_type")]
    public int ElementType { get; init; }

    [JsonPropertyName("now_cost")]
    public int NowCost { get; init; }

    [JsonPropertyName("total_points")]
    public int TotalPoints { get; init; }
}
