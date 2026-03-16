using FplTool.Modules.FplIntegration.Contracts.Dto;

namespace FplTool.Modules.FplIntegration.Contracts;

public interface IFplApiService
{
    Task<BootstrapStaticDto> GetBootstrapStaticAsync(CancellationToken ct = default);
    Task<ManagerPicksResponseDto> GetManagerPicksAsync(int fplManagerId, int gameweekId, CancellationToken ct = default);
    Task<LiveEventDto> GetLiveEventAsync(int gameweekId, CancellationToken ct = default);
}
