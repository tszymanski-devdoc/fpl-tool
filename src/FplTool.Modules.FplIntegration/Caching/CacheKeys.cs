namespace FplTool.Modules.FplIntegration.Caching;

internal static class CacheKeys
{
    public const string BootstrapStatic = "fpl:bootstrap-static";
    public static string LiveEvent(int gameweekId) => $"fpl:live:{gameweekId}";
    public static string ManagerPicks(int fplManagerId, int gameweekId) => $"fpl:picks:{fplManagerId}:{gameweekId}";
}
