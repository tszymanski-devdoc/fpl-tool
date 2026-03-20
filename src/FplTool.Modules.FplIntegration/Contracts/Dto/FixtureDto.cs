using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class FixtureDto
{
    [JsonPropertyName("event")]
    public int Event { get; init; }

    [JsonPropertyName("team_h")]
    public int TeamH { get; init; }

    [JsonPropertyName("team_a")]
    public int TeamA { get; init; }

    [JsonPropertyName("finished")]
    public bool Finished { get; init; }
}
