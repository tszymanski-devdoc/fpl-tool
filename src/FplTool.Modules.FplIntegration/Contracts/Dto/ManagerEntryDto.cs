using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class ManagerEntryDto
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("player_first_name")]
    public string PlayerFirstName { get; init; } = string.Empty;

    [JsonPropertyName("player_last_name")]
    public string PlayerLastName { get; init; } = string.Empty;
}
