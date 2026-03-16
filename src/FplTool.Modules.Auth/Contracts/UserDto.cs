namespace FplTool.Modules.Auth.Contracts;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    int? FplManagerId
);
