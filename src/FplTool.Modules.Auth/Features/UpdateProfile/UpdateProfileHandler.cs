using FplTool.Modules.Auth.Contracts;
using FplTool.Modules.Auth.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Auth.Features.UpdateProfile;

internal sealed class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    private readonly AuthDbContext _dbContext;

    public UpdateProfileHandler(AuthDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserDto>(Error.NotFound("User not found."));

        user.UpdateProfile(request.DisplayName, request.FplManagerId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserDto(user.Id, user.Email, user.DisplayName, user.FplManagerId, user.IsAdmin));
    }
}
