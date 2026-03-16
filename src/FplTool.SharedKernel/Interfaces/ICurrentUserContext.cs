namespace FplTool.SharedKernel.Interfaces;

public interface ICurrentUserContext
{
    Guid UserId { get; }
    string Email { get; }
}
