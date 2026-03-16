using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class PickDto
{
    [JsonPropertyName("element")]
    public int Element { get; init; }

    [JsonPropertyName("position")]
    public int Position { get; init; }

    [JsonPropertyName("multiplier")]
    public int Multiplier { get; init; }

    [JsonPropertyName("is_captain")]
    public bool IsCaptain { get; init; }

    [JsonPropertyName("is_vice_captain")]
    public bool IsViceCaptain { get; init; }
}
