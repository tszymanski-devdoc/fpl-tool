using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class BootstrapStaticDto
{
    [JsonPropertyName("events")]
    public List<GameweekDto> Events { get; init; } = [];

    [JsonPropertyName("elements")]
    public List<PlayerDto> Elements { get; init; } = [];

    [JsonPropertyName("teams")]
    public List<TeamDto> Teams { get; init; } = [];
}
