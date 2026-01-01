using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace STFU;

[BepInPlugin("Amor.STFU", "STFU", "1.2.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MCP3_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(CGWS_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class EntryPoint : BasePlugin
{
    public override void Load()
    {
        Configuration.Init();
        new Harmony("Amor.STFU").PatchAll();
        BoxColliderPseudofix.Setup();
        STFULogger.LogInfo("STFU is done loading!");
    }
}