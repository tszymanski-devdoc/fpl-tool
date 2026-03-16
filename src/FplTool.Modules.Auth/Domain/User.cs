namespace FplTool.Modules.Auth.Domain;

public sealed class User
{
    public Guid Id { get; private set; }
    public string GoogleSubjectId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public int? FplManagerId { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime UpdatedUtc { get; private set; }

    private User() { }

    public static User Create(string googleSubjectId, string email, string displayName)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            GoogleSubjectId = googleSubjectId,
            Email = email,
            DisplayName = displayName,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string? displayName, int? fplManagerId)
    {
        if (displayName is not null)
            DisplayName = displayName;
        if (fplManagerId.HasValue)
            FplManagerId = fplManagerId.Value;
        UpdatedUtc = DateTime.UtcNow;
    }
}
