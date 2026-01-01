using BepInEx.Configuration;
using BepInEx;
using GTFO.API.Utilities;

namespace STFU;

internal static class Configuration
{
    private static readonly ConfigFile Config;

    public static bool SuppressAllMCP3Logs => Config_SuppressAllMCP3Logs.Value;
    private static readonly ConfigEntry<bool> Config_SuppressAllMCP3Logs;

    public static bool SuppressAllCGWSLogs => Config_SuppressCGWSLogs.Value;
    private static readonly ConfigEntry<bool> Config_SuppressCGWSLogs;

    public static bool LogGeoVolumeResults => Config_LogGeoVolumeResults.Value;
    private static readonly ConfigEntry<bool> Config_LogGeoVolumeResults;

    public static bool LogBoxColliderResults => Config_LogBoxColliderResults.Value;
    private static readonly ConfigEntry<bool> Config_LogBoxColliderResults;

    public static bool ShowAffectedBoxColliderNames => Config_ShowAffectedBoxColliderNames.Value;
    private static readonly ConfigEntry<bool> Config_ShowAffectedBoxColliderNames;

    static Configuration()
    {
        Config = new(Path.Combine(Paths.ConfigPath, "STFU.cfg"), true);
        string section = "General Settings";

        Config_SuppressAllMCP3Logs = Config.Bind(section, "Suppress All MyCoolPlugin3 Logs", false, "If enabled, don't allow any logs from MCP3. Otherwise, only print unique logs.");
        Config_SuppressCGWSLogs = Config.Bind(section, "Suppress ConfigurableGlobalWaveSettings Logs", true, "If enabled, don't allow any logs of EnemyType Heats and selected EnemyTypes from CGWS.");
        Config_LogGeoVolumeResults = Config.Bind(section, "Show Post-LG GeomorphVolume Results", true, "Show the total amount of \"borken cells\" error logs.");
        Config_LogBoxColliderResults = Config.Bind(section, "Show Post-LG BoxCollider Results", true, "Show the total amount of BoxColliders warning logs with a negative size or scale.");
        Config_ShowAffectedBoxColliderNames = Config.Bind(section, "Show Affected BoxCollider Names", false, "List the specific BoxColliders that have a negative size or scale.");
    }

    internal static void Init()
    {
        LiveEditListener listener = LiveEdit.CreateListener(Paths.ConfigPath, "STFU.cfg", false);
        listener.FileChanged += OnFileChanged;
    }

    private static void OnFileChanged(LiveEditEventArgs e)
    {
        STFULogger.LogWarning($"Config file changed: {e.FullPath}");
        Config.Reload();
    }
}
