using FplTool.Modules.Auth.Contracts;
using FplTool.Modules.Auth.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Auth.Features.GetProfile;

internal sealed class GetProfileHandler : IRequestHandler<GetProfileQuery, Result<UserDto>>
{
    private readonly AuthDbContext _dbContext;

    public GetProfileHandler(AuthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<UserDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(Error.NotFound("User not found."));

        return Result.Success(new UserDto(user.Id, user.Email, user.DisplayName, user.FplManagerId, user.IsAdmin));
    }
}
