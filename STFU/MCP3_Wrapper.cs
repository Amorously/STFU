using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace STFU;

[HarmonyPatch]
internal static class MCP3_Wrapper
{
    public const string PLUGIN_GUID = "com.MyName.MyGTFOPlugin";
    public static bool IsLoaded { get; private set; } = false;
    private static readonly Type? Ltype;
    private static readonly string[] Lmethods = { "Info" , "Debug", "Error" };  

    static MCP3_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
        {
            try
            {
                var mcp3Asm = info?.Instance?.GetType()?.Assembly ?? throw new Exception("MCP3 Assembly is missing!");
                Type[] types = AccessTools.GetTypesFromAssembly(mcp3Asm);
                Ltype = types.First(t => t.Name == "L");
                IsLoaded = Ltype != null;
            }
            catch (Exception e)
            {
                STFULogger.LogError($"Exception thrown while reading data from MyCoolPLugin3:\n{e}");
                IsLoaded = false;
            }
        }

        STFULogger.LogDebug($"MyCoolPlugin3 is loaded: {IsLoaded}");
    }
    
    [HarmonyPrepare]
    private static bool PrepPatches()
    {
        return IsLoaded;
    }

    [HarmonyTargetMethods]
    static IEnumerable<MethodBase> GetTargetMethods()
    {
        foreach (string name in Lmethods)
        {
            yield return AccessTools.Method(Ltype, name);
            STFULogger.LogAs(name, $"Found MCP3 patch method: {name}");
        }
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpile_Lspam(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
    {
        yield return new(OpCodes.Ldstr, __originalMethod.Name); // string method name
        yield return new(OpCodes.Ldarg_0); // object data
        yield return new(OpCodes.Call, AccessTools.Method(typeof(MCP3_Wrapper), nameof(Intercept), new[] { typeof(string), typeof(object) })); // call our intercept method
        yield return new(OpCodes.Ret); // return        
    }

    public static void Intercept(string method, object data)
    {
        STFULogger.LogAs(method, data?.ToString() ?? string.Empty, Configuration.SuppressAllMCP3Logs);
    }
}