using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace STFU;

[HarmonyPatch]
internal static class CGWS_Wrapper
{
    public const string PLUGIN_GUID = "com.Untilted.ConfigurableGlobalWaveSettings";
    public static bool IsLoaded { get; private set; } = false;
    private static readonly Type? SPtype;
    public const string SPmethod = "LogNextType";

    static CGWS_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
        {
            try
            {
                var cgwsAsm = info?.Instance?.GetType()?.Assembly ?? throw new Exception("CGWS Assembly is missing!");
                Type[] types = AccessTools.GetTypesFromAssembly(cgwsAsm);
                SPtype = types.First(t => t.Name == "SettingsPatches");
                IsLoaded = SPtype != null;
            }
            catch (Exception e)
            {
                STFULogger.LogError($"Exception thrown while reading data from ConfigurableGlobalWaveSettings:\n{e}");
                IsLoaded = false;
            }
        }

        STFULogger.LogDebug($"ConfigurableGlobalWaveSettings is loaded: {IsLoaded}");
    }

    [HarmonyPrepare]
    private static bool PreparePatch()
    {
        return IsLoaded;
    }

    [HarmonyTargetMethod]
    static MethodBase GetTargetMethod()
    {
        STFULogger.LogDebug($"Found CGWS patch method: {SPmethod}");
        return AccessTools.Method(SPtype, SPmethod);
    }

    [HarmonyPrefix]
    private static bool Pre_LogNextType()
    {
        return !Configuration.SuppressAllCGWSLogs;
    }
}
