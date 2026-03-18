using FplTool.Modules.Auth.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Auth.Features.GetFplProfile;

public sealed record GetFplProfileQuery(int ManagerId) : IRequest<Result<FplProfileDto>>;
