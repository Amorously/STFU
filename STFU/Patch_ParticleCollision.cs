using HarmonyLib;

namespace STFU;

[HarmonyPatch(typeof(CellSoundEmitter), nameof(CellSoundEmitter.OnParticleCollision))]
internal static class Patch_ParticleCollision
{
    [HarmonyPrefix]
    private static bool Cancel_Null_ps_Param(CellSoundEmitter __instance)
    {
        if (__instance.m_particleSystem == null)
        {
            return false;
        }
        return true;
    }
}
