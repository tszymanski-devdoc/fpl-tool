using FplTool.Modules.Auth.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Auth.Features.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IRequest<Result<UserDto>>;
