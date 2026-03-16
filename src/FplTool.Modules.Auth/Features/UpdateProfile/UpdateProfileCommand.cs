using FplTool.Modules.Auth.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Auth.Features.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    int? FplManagerId
) : IRequest<Result<UserDto>>;
