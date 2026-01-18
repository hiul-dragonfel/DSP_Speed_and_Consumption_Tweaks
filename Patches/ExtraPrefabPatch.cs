using HarmonyLib;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Config;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{
    internal class ExtraPrefabPatch
    {
        [HarmonyPatch(typeof(PrefabDesc), "ReadPrefab")]
        [HarmonyPostfix]
        public static void ReadPrefab_Postfix(ref PrefabDesc __instance)
        {
            __instance.unitMaxMovementSpeed *= (float)Dark_Fog_CONFIG.maxEnemySpeedMultiplier.Value;
            __instance.unitMaxMovementAcceleration *= (float)Dark_Fog_CONFIG.maxEnemySpeedMultiplier.Value;
            __instance.fleetMaxMovementSpeed *= (float)Dark_Fog_CONFIG.maxEnemySpeedMultiplier.Value;
            __instance.fleetMaxMovementAcceleration *= (float)Dark_Fog_CONFIG.maxEnemySpeedMultiplier.Value;
            __instance.unitMarchMovementSpeed *= (float)Dark_Fog_CONFIG.maxEnemyAttackSpeedMultiplier.Value;
        }
    }
}
