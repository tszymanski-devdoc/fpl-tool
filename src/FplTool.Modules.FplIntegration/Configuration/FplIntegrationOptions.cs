namespace FplTool.Modules.FplIntegration.Configuration;

public sealed class FplIntegrationOptions
{
    public const string SectionName = "Fpl";

    public string BaseUrl { get; set; } = "https://fantasy.premierleague.com/api/";
    public int BootstrapCacheMinutes { get; set; } = 60;
    public int LiveCacheMinutes { get; set; } = 5;
    public int PicksCacheMinutes { get; set; } = 10;

    // Optional FPL account credentials — required when calling from cloud environments
    // that get 403 blocked. Set via Secret Manager: Fpl__FplEmail, Fpl__FplPassword.
    public string FplEmail { get; set; } = string.Empty;
    public string FplPassword { get; set; } = string.Empty;
}
