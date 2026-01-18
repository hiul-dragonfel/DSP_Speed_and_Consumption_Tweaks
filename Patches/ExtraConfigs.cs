using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{
    public static class ConfigsPatches
    {
        private static readonly double _walk_Speed = 5;
        private static readonly double _walk_Power = 15_000;
        private static readonly double _max_Sail_Speed = 1_000;
        private static readonly double _max_Warp_Speed = 1_000_000;
        private static readonly double _warp_Keeping_Power_Per_Speed = 100;
        private static readonly double _warp_Start_Power_Per_Speed = 1_600;

        private static readonly double _drone_Speed = 8;

        private static readonly double _ship_Cruise_Speed = 1_000;
        private static readonly double _ship_Warp_Speed = 1_000_000;

        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.Import))]
        [HarmonyPostfix]
        public static void Postfix_GameHistoryData_Import(GameHistoryData __instance)
        {
            __instance.logisticDroneSpeed = (float)(_drone_Speed * DSP_Config.Logistic_DRONE_CONFIG.DroneTravelSpeedMutliplier.Value);
            __instance.logisticShipSailSpeed = (float)(_ship_Cruise_Speed * DSP_Config.Logistic_SHIP_CONFIG.ShipCruiseSpeedMultiplier.Value);
            __instance.logisticShipWarpSpeed = (float)(_ship_Warp_Speed * DSP_Config.Logistic_SHIP_CONFIG.ShipWarpSpeedMultiplier.Value);
        }

        [HarmonyPatch(typeof(Configs))]
        [HarmonyPatch("get_freeMode")]
        [HarmonyPostfix]
        public static void postfix_freeMode(ref ModeConfig __result)
        {
            // mecha game start modifs
            __result.mechaWalkSpeed                 = (float) (_walk_Speed * DSP_Config.Mecha_LAND_CONFIG.WalkSpeedMultiplier.Value);
            __result.mechaWalkPower                 = _walk_Power * DSP_Config.Mecha_LAND_CONFIG.WalkEnergyCostMultiplier.Value;
            __result.mechaSailSpeedMax              = (float) (_max_Sail_Speed * DSP_Config.Mecha_CRUISE_CONFIG.CruiseMaxSpeedMultiplier.Value);
            __result.mechaWarpSpeedMax              = (float) (_max_Warp_Speed * DSP_Config.Mecha_WARP_CONFIG.maxWarpSpeedMultiplier.Value);
            __result.mechaWarpKeepingPowerPerSpeed  = _warp_Keeping_Power_Per_Speed * DSP_Config.Mecha_WARP_CONFIG.warpKeepingConsumptionMultiplier.Value;
            __result.mechaWarpStartPowerPerSpeed    = _warp_Start_Power_Per_Speed * DSP_Config.Mecha_WARP_CONFIG.warpStartPowerConsumptionMultiplier.Value;

            // logistic drones game start modifs
            __result.logisticDroneSpeed             = (float) (_drone_Speed * DSP_Config.Logistic_DRONE_CONFIG.DroneTravelSpeedMutliplier.Value);
            
            // logistic vessels game start modifs
            __result.logisticShipSailSpeed          = (float) ( _ship_Cruise_Speed * DSP_Config.Logistic_SHIP_CONFIG.ShipCruiseSpeedMultiplier.Value);
            __result.logisticShipWarpSpeed          = (float) ( _ship_Warp_Speed * DSP_Config.Logistic_SHIP_CONFIG.ShipWarpSpeedMultiplier.Value);
        }
    }
}
