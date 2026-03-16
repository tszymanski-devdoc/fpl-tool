using System.Text.Json.Serialization;

namespace FplTool.Modules.FplIntegration.Contracts.Dto;

public sealed class ManagerPicksResponseDto
{
    [JsonPropertyName("picks")]
    public List<PickDto> Picks { get; init; } = [];
}
