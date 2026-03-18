using FplTool.Modules.Auth.Contracts;
using FplTool.Modules.FplIntegration.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Auth.Features.GetFplProfile;

internal sealed class GetFplProfileHandler : IRequestHandler<GetFplProfileQuery, Result<FplProfileDto>>
{
    private readonly IFplApiService _fplApiService;

    public GetFplProfileHandler(IFplApiService fplApiService)
    {
        _fplApiService = fplApiService;
    }

    public async Task<Result<FplProfileDto>> Handle(GetFplProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _fplApiService.GetManagerEntryAsync(request.ManagerId, cancellationToken);
            var playerName = $"{entry.PlayerFirstName} {entry.PlayerLastName}".Trim();
            return Result.Success(new FplProfileDto(entry.Name, playerName));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Failure<FplProfileDto>(Error.NotFound("No FPL manager found with that ID."));
        }
    }
}
