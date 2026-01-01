using BepInEx.Logging;
using GTFO.API;
using System.Collections.Concurrent;

namespace STFU;

internal static class STFULogger
{
    public static ConcurrentDictionary<string, int> UniqueLogMap { get; internal set; } = new();
    private static readonly ManualLogSource MLS = Logger.CreateLogSource("STFU");

    static STFULogger()
    {
        LevelAPI.OnBuildStart += Clear;
        LevelAPI.OnLevelCleanup += Clear;
    }

    private static void Clear()
    {
        UniqueLogMap.Clear();
    }

    public static void LogInfo(string str) => MLS.Log(LogLevel.Message, str);

    public static void LogWarning(string str) => MLS.Log(LogLevel.Warning, str);

    public static void LogError(string str) => MLS.Log(LogLevel.Error, str);

    public static void LogDebug(string str) => MLS.Log(LogLevel.Debug, str);

    public static void LogAs(string logLevel, string str, bool suppressLog = false)
    {
        UniqueLogMap.AddOrUpdate(str, key =>
        {
            if (!string.IsNullOrEmpty(str) && !suppressLog)
            {
                switch (logLevel)
                {
                    case "Info":
                        LogInfo(str);
                        break;
                    case "Debug":
                        LogDebug(str);
                        break;
                    case "Error":
                        LogError(str);
                        break;
                    case "Warning":
                        LogWarning(str);
                        break;
                    default:
                        LogError("<null> " + str);
                        break;
                }
            }
            return 1;
        }, (key, cnt) => cnt += 1);
    }
}
