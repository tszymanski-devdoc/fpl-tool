using FplTool.Modules.Auth.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Auth.Features.VerifyGoogleToken;

public sealed record VerifyGoogleTokenCommand(string IdToken) : IRequest<Result<AuthResponseDto>>;

public sealed record AuthResponseDto(string AccessToken, UserDto User);
