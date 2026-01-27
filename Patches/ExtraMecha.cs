using DSP_Speed_and_Consumption_Tweaks.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Config;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{

    // TODO Review this file and update to your own requirements, or remove it altogether if not required

    /// <summary>
    /// Sample Harmony Patch class. Suggestion is to use one file per patched class
    /// though you can include multiple patch classes in one file.
    /// Below is included as an example, and should be replaced by classes and methods
    /// for your mod.
    /// </summary>
    public class ExtraMecha
    {
        // Mecha
        private volatile bool uptodate = false;

        private volatile float _cruise_Acceleration_Rate_Multiplier = 1;
        private volatile float _flight_Speed_Multiplier = 1;
        private volatile float _walk_speed_upgrade_Multiplier = 1;
        public float Flight_Speed_Multiplier
        {
            get { return _flight_Speed_Multiplier; }
            set
            {
                if (value < .1) _flight_Speed_Multiplier = 0.1f;
                else _flight_Speed_Multiplier = value;
                changed();
            }
        }
        public double Cruise_Acceleration_Rate_Multiplier
        {
            get { return _cruise_Acceleration_Rate_Multiplier; }
            set { _cruise_Acceleration_Rate_Multiplier = value > 0 ? (float)value: 0.1f ; changed(); }
        }

        public float Walk_speed_upgrade_Multiplier
        {
            get { return _walk_speed_upgrade_Multiplier; }
            set { _walk_speed_upgrade_Multiplier = value > 0 ? (float)value : 0.1f; changed(); }
        }
        public bool is_uptodate() { return uptodate; }
        private void changed() { uptodate = false; }
        public void updated() { uptodate = true; }
    }

    public static class ExtraMechaData
    {
        private static readonly ConditionalWeakTable<Mecha, ExtraMecha> table
            = new ConditionalWeakTable<Mecha, ExtraMecha>();

        public static ExtraMecha Get(Mecha m) => table.GetOrCreateValue(m);

    }

    public static class MechaPatches
    {
        public static bool freeModePost = true; 
        [HarmonyPatch( typeof(Configs), nameof(Configs.freeMode), MethodType.Getter)]
        [HarmonyPostfix]
        public static void postfix_freeMode(ModeConfig __result, Configs __instance)
        {
            if (freeModePost)
            {
                Log.LogInfo("+------------------------------------+");
                Log.LogInfo("|            MechaPatches            |");
                Log.LogInfo("| In Configs.freeMode method Postfix |");
                Log.LogInfo("+------------------------------------+");
                freeModePost = false;
            }
            
            __result.mechaWalkSpeed = (float)(5.0 * Mecha_LAND_CONFIG.WalkSpeedMultiplier.Value);
            __result.mechaJumpEnergy = 25_000 * Mecha_LAND_CONFIG.JumpEnergyMultiplier.Value;
            __result.mechaThrustPowerPerAcc = (float)(24_000 * Mecha_CRUISE_CONFIG.CruiseAccelerationEnergyCostMultiplier.Value);
            __result.mechaWarpStartPowerPerSpeed = 1_600 * Mecha_WARP_CONFIG.warpStartPowerConsumptionMultiplier.Value;
            __result.mechaWarpKeepingPowerPerSpeed = 100 * Mecha_WARP_CONFIG.warpKeepingConsumptionMultiplier.Value;

            __result.mechaWalkPower = 15_000 * Mecha_LAND_CONFIG.WalkEnergyCostMultiplier.Value;
            __result.mechaJumpSpeed = (float)(32.0 * Mecha_LAND_CONFIG.JumpSpeedMultiplier.Value);
            __result.mechaSailSpeedMax = (float)(1_000 * Mecha_CRUISE_CONFIG.CruiseMaxSpeedMultiplier.Value);
            __result.mechaWarpSpeedMax = (float)(1_000_000 * Mecha_WARP_CONFIG.maxWarpSpeedMultiplier.Value);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.Import))]
        [HarmonyPostfix]
        public static void postfix_Gamedata_import(GameData __instance)
        {
            if (freeModePost)
            {
                Log.LogInfo("+------------------------------------+");
                Log.LogInfo("|      postfix_Gamedata_import       |");
                Log.LogInfo("|     In GameData method Postfix     |");
                Log.LogInfo("+------------------------------------+");
                freeModePost = false;
            }

            __instance.mainPlayer.mecha.walkPower = 15_000 * Mecha_LAND_CONFIG.WalkEnergyCostMultiplier.Value;
            __instance.mainPlayer.mecha.jumpEnergy = 25_000 * Mecha_LAND_CONFIG.JumpEnergyMultiplier.Value;
            __instance.mainPlayer.mecha.thrustPowerPerAcc = 24_000 * Mecha_CRUISE_CONFIG.CruiseAccelerationEnergyCostMultiplier.Value;
            __instance.mainPlayer.mecha.warpStartPowerPerSpeed = 1_600 * Mecha_WARP_CONFIG.warpStartPowerConsumptionMultiplier.Value;
            __instance.mainPlayer.mecha.warpKeepingPowerPerSpeed = 100 * Mecha_WARP_CONFIG.warpKeepingConsumptionMultiplier.Value;
                  
            
            __instance.mainPlayer.mecha.jumpSpeed = (float)(32.0 * Mecha_LAND_CONFIG.JumpSpeedMultiplier.Value);
            double filler = 0;
            Postfix_UnlockTechFunction(3, ref filler , 0, __instance.history);
            Postfix_UnlockTechFunction(11, ref filler, 0, __instance.history);
            Postfix_UnlockTechFunction(27, ref filler, 0, __instance.history);
        }

        public static bool UnlockTeckFunctionPost = true;
        [HarmonyPatch(typeof(GameHistoryData), nameof(GameHistoryData.UnlockTechFunction))]
        [HarmonyPostfix]
        public static void Postfix_UnlockTechFunction(int func, ref double value, int level, GameHistoryData __instance)
        {
            if (UnlockTeckFunctionPost && DEBUG)
            {
                Log.LogInfo("+----------------------------------------------+");
                Log.LogInfo("|                 MechaPatches                 |");
                Log.LogInfo("| In PostFix_UnlockTechFunction method Postfix |");
                Log.LogInfo("+----------------------------------------------+");
                UnlockTeckFunctionPost = false;
            }
            if(DEBUG)
            {
                Log.LogInfo($"func  : {func}");
                Log.LogInfo($"value : {value}");
                Log.LogInfo($"level : {level}");
            }
            int id = 0;
            switch (func)
            {
                case 3:
                    GameMain.mainPlayer.mecha.walkSpeed = Configs.freeMode.mechaWalkSpeed;
                    for ( id = 2201; id <= 2208; id++)
                    {
                        if (DEBUG)
                        {
                            Log.LogInfo($"item Id = {id}");
                            Log.LogInfo($"GameMain.history.techStates.curLevel      = {__instance.techStates[id].curLevel}");
                            Log.LogInfo($"GameMain.history.techStates.maxLevel      = {__instance.techStates[id].maxLevel}");
                            Log.LogInfo($"GameMain.history.techStates.unlocked      = {__instance.techStates[id].unlocked}");
                            Log.LogInfo($"GameMain.history.techStates.unlockTick    = {__instance.techStates[id].unlockTick}");
                            Log.LogInfo($"GameMain.history.techStates.hashNeeded    = {__instance.techStates[id].hashNeeded}");
                            Log.LogInfo($"GameMain.history.techStates.hashUploaded  = {__instance.techStates[id].hashUploaded}");
                            Log.LogInfo($"GameMain.history.techStates.uPointPerHash = {__instance.techStates[id].uPointPerHash}");
                            Log.LogInfo($"LDB.techs.Select({id}).UnlockValues[0]    = {LDB.techs.Select(id).UnlockValues[0]}");
                        }
                        if (GameMain.history.techStates[id].unlocked) GameMain.mainPlayer.mecha.walkSpeed += (float)(LDB.techs.Select(id).UnlockValues[0] * Mecha_LAND_CONFIG.WalkSpeedUpgrageMultiplier.Value);
                    }
                    break;
                case 4:
                    for (id = 2901; id <= 2903; id++)
                    {
                        if (DEBUG)
                        {
                            Log.LogInfo($"item Id = {id}");
                            Log.LogInfo($"GameMain.history.techStates.curLevel      = {__instance.techStates[id].curLevel}");
                            Log.LogInfo($"GameMain.history.techStates.maxLevel      = {__instance.techStates[id].maxLevel}");
                            Log.LogInfo($"GameMain.history.techStates.unlocked      = {__instance.techStates[id].unlocked}");
                            Log.LogInfo($"GameMain.history.techStates.unlockTick    = {__instance.techStates[id].unlockTick}");
                            Log.LogInfo($"GameMain.history.techStates.hashNeeded    = {__instance.techStates[id].hashNeeded}");
                            Log.LogInfo($"GameMain.history.techStates.hashUploaded  = {__instance.techStates[id].hashUploaded}");
                            Log.LogInfo($"GameMain.history.techStates.uPointPerHash = {__instance.techStates[id].uPointPerHash}");
                            Log.LogInfo($"LDB.techs.Select({id}).UnlockValues[0]    = {LDB.techs.Select(id).UnlockValues[0]}");
                        }
                        //if (GameMain.history.techStates[id].unlocked) GameMain.mainPlayer.mecha.walkSpeed += (float)(LDB.techs.Select(id).UnlockValues[0] * Mecha_LAND_CONFIG.WalkSpeedUpgrageMultiplier.Value);
                    }
                    break;
                case 11:
                    id = 2903;
                    if (DEBUG)
                    {
                        Log.LogInfo($"item Id = {id}");
                        Log.LogInfo($"GameMain.history.techStates.curLevel      = {__instance.techStates[id].curLevel}");
                        Log.LogInfo($"GameMain.history.techStates.maxLevel      = {__instance.techStates[id].maxLevel}");
                        Log.LogInfo($"GameMain.history.techStates.unlocked      = {__instance.techStates[id].unlocked}");
                        Log.LogInfo($"GameMain.history.techStates.unlockTick    = {__instance.techStates[id].unlockTick}");
                        Log.LogInfo($"GameMain.history.techStates.hashNeeded    = {__instance.techStates[id].hashNeeded}");
                        Log.LogInfo($"GameMain.history.techStates.hashUploaded  = {__instance.techStates[id].hashUploaded}");
                        Log.LogInfo($"GameMain.history.techStates.uPointPerHash = {__instance.techStates[id].uPointPerHash}");
                        Log.LogInfo($"LDB.techs.Select({id}).UnlockValues[0]    = {LDB.techs.Select(id).UnlockValues[0]}");
                    }
                    GameMain.mainPlayer.mecha.maxSailSpeed = Configs.freeMode.mechaSailSpeedMax;
                    if (__instance.techStates[id].unlocked) GameMain.mainPlayer.mecha.maxSailSpeed += Configs.freeMode.mechaSailSpeedMax;
                    break;
                case 27:
                    GameMain.mainPlayer.mecha.maxWarpSpeed = Configs.freeMode.mechaWarpSpeedMax;

                    for (id = 2905; id <= 2906; id++)
                    {
                        if (DEBUG)
                        {
                            Log.LogInfo($"item Id = {id}");
                            Log.LogInfo($"GameMain.history.techStates.curLevel      = {__instance.techStates[id].curLevel}");
                            Log.LogInfo($"GameMain.history.techStates.maxLevel      = {__instance.techStates[id].maxLevel}");
                            Log.LogInfo($"GameMain.history.techStates.unlocked      = {__instance.techStates[id].unlocked}");
                            Log.LogInfo($"GameMain.history.techStates.unlockTick    = {__instance.techStates[id].unlockTick}");
                            Log.LogInfo($"GameMain.history.techStates.hashNeeded    = {__instance.techStates[id].hashNeeded}");
                            Log.LogInfo($"GameMain.history.techStates.hashUploaded  = {__instance.techStates[id].hashUploaded}");
                            Log.LogInfo($"GameMain.history.techStates.uPointPerHash = {__instance.techStates[id].uPointPerHash}");
                            Log.LogInfo($"LDB.techs.Select({id}).UnlockValues[0]    = {LDB.techs.Select(id).UnlockValues[0]}");
                        }
                        float mechaWarpSpeed = (float)Mecha_WARP_CONFIG.maxWarpSpeedMultiplier.Value * 3 * 40_000;
                        if (__instance.techStates[id].unlocked && id == 2905) GameMain.mainPlayer.mecha.maxWarpSpeed += mechaWarpSpeed;
                        if (id == 2906) GameMain.mainPlayer.mecha.maxWarpSpeed += mechaWarpSpeed * (__instance.techStates[id].curLevel - 6);
                        if (__instance.techStates[id].unlocked && id == 2906) GameMain.mainPlayer.mecha.maxWarpSpeed += mechaWarpSpeed;
                    }                    
                    break;
            }
        }

        [HarmonyPatch(typeof(Mecha), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void PostfixCtor(Mecha __instance)
        {
            if (DEBUG)
            {
                Log.LogInfo("+---------------------------------------+");
                Log.LogInfo("|             MechaPatches              |");
                Log.LogInfo("|  In Mecha_PostfixCtor method Postfix  |");
                Log.LogInfo("+---------------------------------------+");
                Log.LogInfo($"| Acceleration multiplier = {Mecha_CRUISE_CONFIG.CruiseAccelerationRateMultiplier.Value,11} |");
                Log.LogInfo($"| Flight Speed Multiplier = {Mecha_LAND_CONFIG.FlightSpeedMultiplier.Value,11} |");
                Log.LogInfo($"| __instance              = {RuntimeHelpers.GetHashCode(__instance),11} |");
                Log.LogInfo("+---------------------------------------+");
            }
            var extra = ExtraMechaData.Get(__instance);
            extra.Cruise_Acceleration_Rate_Multiplier = Mecha_CRUISE_CONFIG.CruiseAccelerationRateMultiplier.Value;
            extra.Flight_Speed_Multiplier = (float)Mecha_LAND_CONFIG.FlightSpeedMultiplier.Value;
            extra.Walk_speed_upgrade_Multiplier = (float)Mecha_LAND_CONFIG.WalkSpeedUpgrageMultiplier.Value;
        }

        public static bool PlayerMove_Sail_Constructor = true;
        [HarmonyPatch(typeof(PlayerMove_Sail), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void PostfixCtor(PlayerMove_Sail __instance)
        {
            if (PlayerMove_Sail_Constructor)
            {
                Log.LogInfo("+-----------------------------------------------+");
                Log.LogInfo("|                 MechaPatches                  |");
                Log.LogInfo("| In PlayerMove_Sail_PostfixCtor method Postfix |");
                Log.LogInfo("+-----------------------------------------------+");
                PlayerMove_Sail_Constructor = false;
            }
            __instance.max_acc = double.MaxValue;
        }

        public static bool SetForNewGamePost = true;
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.SetForNewGame))]
        [HarmonyPostfix]
        public static void postfix_SetForNewGame(Mecha __instance)
        {
            if (SetForNewGamePost)
            {
                Log.LogInfo("+-----------------------------------------+");
                Log.LogInfo("|               MechaPatches              |");
                Log.LogInfo("| In postfix_SetForNewGame method Postfix |");
                Log.LogInfo("+-----------------------------------------+");
                SetForNewGamePost = false;
            }
        }
        static bool b_PlayerMove_Fly_GameTick_Transpiler = true;
        [HarmonyPatch(typeof(PlayerMove_Fly), nameof(PlayerMove_Fly.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PlayerMove_Fly_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);
            if (b_PlayerMove_Fly_GameTick_Transpiler)
            {
                Log.LogInfo("+------------------------------------------+");
                Log.LogInfo("|               MechaPatches               |");
                Log.LogInfo("| In GameTick_Transpiler method Transpiler |");
                Log.LogInfo("+------------------------------------------+");
                b_PlayerMove_Fly_GameTick_Transpiler = false;
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerAction), nameof(PlayerAction.player))),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.mecha))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Mecha), nameof(Mecha.walkSpeed))),
                new CodeMatch(OpCodes.Ldc_R4, 2.5f),
                new CodeMatch(OpCodes.Mul)
            );
            if (matcher.IsValid)
            {
                matcher.Advance(-1);
                matcher.SetAndAdvance(OpCodes.Ldc_R4, 1f);
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldc_R4, 1.5f),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerMove_Sail), nameof(PlayerMove_Sail.mecha))),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraMechaData), nameof(ExtraMechaData.Get))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ExtraMecha), nameof(ExtraMecha.Flight_Speed_Multiplier))),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Add)
                );
            }
            
            if (DEBUG)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher))
                    Log.LogInfo($"{strInstruction}");
            }

            return matcher.Instructions();
        }

        public static bool b_UITechNode_UpdateInfoComplete = true;
        [HarmonyPatch(typeof(UITechNode), nameof(UITechNode.UpdateInfoComplete))]
        [HarmonyPostfix]
        public static void postfix_UITechNode_UpdateInfoComplete(UITechNode __instance)
        {
            if (b_UITechNode_UpdateInfoComplete && DEBUG)
            {
                Log.LogInfo("+---------------------------------------------------------+");
                Log.LogInfo("|                       MechaPatches                      |");
                Log.LogInfo("| In postfix_UITechNode_UpdateInfoComplete method Postfix |");
                Log.LogInfo("+---------------------------------------------------------+");
            }
            if (__instance.destroyed) return;
            if (2201 <= __instance.techProto.ID && __instance.techProto.ID <= 2208){
                var result = Regex.Replace(
                    __instance.unlockText.text, 
                    @"([0-9]+) m/s",
                    match =>
                    {
                        float i = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        i = i * ExtraMechaData.Get(GameMain.mainPlayer.mecha).Walk_speed_upgrade_Multiplier;
                        return i.ToString() + " m/s";
                    }
                );
                __instance.unlockText.text = result;
                __instance.descHeight = (int)__instance.techDescText.preferredHeight;
            }
            if (__instance.techProto.ID == 2901)
            {
                var flightSpeedMultiplier = ExtraMechaData.Get(GameMain.mainPlayer.mecha).Flight_Speed_Multiplier;

                __instance.techDescText.text = Regex.Replace(__instance.techDescText.text, @"150%", ((150 * flightSpeedMultiplier)).ToString("0") + "%");
                __instance.descHeight = (int)__instance.techDescText.preferredHeight;
            }
            else if (__instance.techProto.ID == 2903)
            {
                var mechasailspeed = Configs.freeMode.mechaSailSpeedMax;
                if (mechasailspeed < 20_000) __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "1000 m/s", (mechasailspeed).ToString("0") + " m/s");
                else if (20_000 <= mechasailspeed && mechasailspeed < 600_000) __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "1000 m/s", (mechasailspeed /40_000).ToString("0.00") + " AU/s");
                else __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "1000 m/s", (mechasailspeed / 2_400_000).ToString("0.00") + " LY/s");

                __instance.descHeight = (int)__instance.techDescText.preferredHeight;
            }
            else if (__instance.techProto.ID == 2905 || __instance.techProto.ID == 2906)
            {
                
                float mechaWarpSpeed = (float) Mecha_WARP_CONFIG.maxWarpSpeedMultiplier.Value * 3 * 40_000;
                if (mechaWarpSpeed < 20_000) __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "3 AU/s", (mechaWarpSpeed).ToString("0") + " m/s");
                else if (20_000 <= mechaWarpSpeed && mechaWarpSpeed < 600_000) __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "3 AU/s", (mechaWarpSpeed / 40_000).ToString("0.00") + " AU/s");
                else __instance.unlockText.text = Regex.Replace(__instance.unlockText.text, "3 AU/s", (mechaWarpSpeed / 2_400_000).ToString("0.00") + " LY/s");

                __instance.descHeight = (int)__instance.techDescText.preferredHeight;
                b_UITechNode_UpdateInfoComplete = false;
            }
        }

        public static bool b_RefreshDataValueText = true;
        [HarmonyPatch(typeof(UITechTree), nameof(UITechTree.RefreshDataValueText))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UITechTree_RefreshDataValueText_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);
            if (b_RefreshDataValueText)
            {
                Log.LogInfo("+-----------------------------------------------------------------+");
                Log.LogInfo("|                           MechaPatches                          |");
                Log.LogInfo("|    In UITechTree_RefreshDataText_Transpiler method Transpiler   |");
                Log.LogInfo("+-----------------------------------------------------------------+");
                b_PlayerMove_Fly_GameTick_Transpiler = false;
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_R4, 2.5f),
                new CodeMatch(OpCodes.Mul)
            );

            if (matcher.IsValid)
            {
                matcher.Advance(-1);
                matcher.SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 1f));
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldc_R4, 1.5f),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraMechaData), nameof(ExtraMechaData.Get))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ExtraMecha), nameof(ExtraMecha.Flight_Speed_Multiplier))),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Add)
                );
            }

            if (DEBUG)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher))
                    Log.LogInfo($"{strInstruction}");
            }

            return matcher.Instructions();
        }

        public static volatile bool GameTickTrans = true;
        /// <summary>
        /// Patches the PlayerMove_Sail GameTick method with postfix code.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(PlayerMove_Sail), nameof(PlayerMove_Sail.GameTick))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var matcher = new CodeMatcher(codeInstructions, il);

            if (GameTickTrans)
            {
                Log.LogInfo("+------------------------------------------+");
                Log.LogInfo("|               MechaPatches               |");
                Log.LogInfo("| In GameTick_Transpiler method Transpiler |");
                Log.LogInfo("+------------------------------------------+");
                GameTickTrans = false;
            }

            if (DEBUG)
            {
                Log.LogInfo("+----------------------------------------------------------+");
                Log.LogInfo("| In PlayerMove_Sail.GameTick_Transpiler method Transpiler |");
                Log.LogInfo("+----------------------------------------------------------+");
            }

            matcher.Start();
            var acc_mult = il.DeclareLocal(typeof(double));
            var acc_power_mult = il.DeclareLocal(typeof(double));
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerMove_Sail), nameof(PlayerMove_Sail.mecha))),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraMechaData), nameof(ExtraMechaData.Get))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ExtraMecha), nameof(ExtraMecha.Cruise_Acceleration_Rate_Multiplier))),
                new CodeInstruction(OpCodes.Stloc_S, acc_mult.LocalIndex)
            );
            if (DEBUG)
            {
                foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 50))
                    Log.LogInfo($"{expectedInstruction}");
            }
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R8, 0.02),
                new CodeMatch(OpCodes.Mul)
            );

            if (matcher.IsValid)
            {
                if (DEBUG)
                {
                    Log.LogInfo($"------ Acceleration rate multiplier -------");
                    Log.LogInfo($"-----------------  before  ----------------");
                }

                Log.LogInfo($"Found! at {matcher.Pos - 1}");
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, acc_mult.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                );

                if (DEBUG)
                {
                    Log.LogInfo($"-------------  after  ------------");
                    foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                        Log.LogInfo($"{expectedInstruction}");
                }
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R8, 0.02),
                new CodeMatch(OpCodes.Mul)
            );

            if (matcher.IsValid)
            {
                if (DEBUG)
                {
                    Log.LogInfo($"------ Acceleration rate multiplier -------");
                    Log.LogInfo($"-----------------  before  ----------------");
                    foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                        Log.LogInfo($"{expectedInstruction}");
                }

                Log.LogInfo($"Found! at {matcher.Pos - 1}");
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, acc_mult.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                );

                if (DEBUG)
                {
                    Log.LogInfo($"-------------  after  ------------");
                    foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                        Log.LogInfo($"{expectedInstruction}");
                }
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerMove_Sail), nameof(PlayerMove_Sail.visual_uvel))),
                new CodeMatch(OpCodes.Ldc_R8, 0.008)
            );
            if (matcher.IsValid)
            {
                if (DEBUG)
                {
                    Log.LogInfo($"------ Desceleration rate multiplier -------");
                    Log.LogInfo($"-----------------  before  ----------------");
                    foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                        Log.LogInfo($"{expectedInstruction}");
                }

                Log.LogInfo($"Found! at {matcher.Pos - 1}");
                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, acc_mult.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                );

                if (DEBUG)
                {
                    Log.LogInfo($"-------------  after  ------------");
                    foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                        Log.LogInfo($"{expectedInstruction}");
                }
            }
            if (DEBUG)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher))
                    Log.LogInfo($"{strInstruction}");
            }
            return matcher.Instructions();
        }
    }
}