using DSP_Speed_and_Consumption_Tweaks.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Config;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;
using static UnityEngine.UI.Image;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{
    // TODO Review this file and update to your own requirements, or remove it altogether if not required


    /// <summary>
    /// Sample Harmony Patch class. Suggestion is to use one file per patched class
    /// though you can include multiple patch classes in one file.
    /// Below is included as an example, and should be replaced by classes and methods
    /// for your mod.
    /// </summary>
    public class ExtraStationComponent
    {
        public bool showFirst = false;
        public int position = 0;

        // drone
        private double _drone_Taxi_Speed = 1;
        private double _drone_Ejection_Energy = 800_000;
        private double _drone_Energy_Per_Meter = 20_000;

        public double DroneTaxiSpeed { get { return Volatile.Read(ref _drone_Taxi_Speed); } set { Volatile.Write(ref _drone_Taxi_Speed, value); updated(); } }
        public double DroneTakeOffEnergyMultiplier { get { return Volatile.Read(ref _drone_Ejection_Energy); } set { Volatile.Write(ref _drone_Ejection_Energy, 800_000 * value); updated(); }  }
        public double DroneEnergyPerMeterMultiplier { get { return Volatile.Read(ref _drone_Energy_Per_Meter); } set { Volatile.Write(ref _drone_Energy_Per_Meter, 20_000 * value); updated(); } }

        // vessel
        private double Ship_Approch_Speed = 400;
        private double Ship_Taxi_Speed = 1;

        private double Ship_Energy_Cost_Per_Meter = 30;
        private double Ship_Energy_Cost_Take_Off = 6_000_000;
        private double Ship_Energy_Cost_For_Max_Speed = 200_000;
        private double Ship_Energy_Cost_Per_Warp = 100_000_000;

        public double ShipApprochSpeed { get { return Volatile.Read(ref Ship_Approch_Speed); } set { Volatile.Write(ref Ship_Approch_Speed, Math.Pow(value,0.4)); updated(); } }
        public double ShipTaxiSpeed { get { return Volatile.Read(ref Ship_Taxi_Speed); } set { Volatile.Write(ref Ship_Taxi_Speed, value); updated(); } }

        public double ShipEnergyCostPerMeter { get { return Volatile.Read(ref Ship_Energy_Cost_Per_Meter); } set { Volatile.Write(ref Ship_Energy_Cost_Per_Meter, 30 * value); updated(); } }
        public double ShipEnergyCostTakeOff { get { return Volatile.Read(ref Ship_Energy_Cost_Take_Off); } set { Volatile.Write(ref Ship_Energy_Cost_Take_Off, 6_000_000 * value); updated(); } }
        public double ShipEnergyCostForMaxSpeed { get { return Volatile.Read(ref Ship_Energy_Cost_For_Max_Speed); } set { Volatile.Write(ref Ship_Energy_Cost_For_Max_Speed, 200_000 * value); updated(); } }
        public double ShipEnergyCostPerWarp { get { return Volatile.Read(ref Ship_Energy_Cost_Per_Warp); } set { Volatile.Write(ref Ship_Energy_Cost_Per_Warp, 100_000_000 * value); updated(); } }
        
        volatile bool uptodate = false;
        public bool is_uptodate() { return uptodate; }
        public void changed() { uptodate = false; }
        public void updated() { uptodate = true; }
    }
    public static class ExtraStationComponentData
    {
        private static readonly ConditionalWeakTable<StationComponent, ExtraStationComponent> table
            = new ConditionalWeakTable<StationComponent, ExtraStationComponent>();

        public static ExtraStationComponent Get(StationComponent instance) => table.GetOrCreateValue(instance);
    }
    public static class StationComponentPatches
    {
        public static bool StationComponentPost = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(StationComponent), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void Postfix_StationComponent_Ctor(StationComponent __instance)
        {
            if (StationComponentPost)
            {
                Log.LogInfo("+-------------------------------------------------+");
                Log.LogInfo("|             StationComponentPatches             |");
                Log.LogInfo("| In Postfix_StationComponent_Ctor method Postfix |");
                Log.LogInfo("+-------------------------------------------------+");
                StationComponentPost = false;
            }

            var extra = ExtraStationComponentData.Get(__instance);
            extra.DroneTaxiSpeed = Logistic_DRONE_CONFIG.DroneTaxiSpeed.Value;
            extra.DroneTakeOffEnergyMultiplier = Logistic_DRONE_CONFIG.DroneEnergyTakeOffMultiplier.Value;
            extra.DroneEnergyPerMeterMultiplier = Logistic_DRONE_CONFIG.DroneEnergyTransitMultiplier.Value;
            extra.ShipTaxiSpeed = Logistic_SHIP_CONFIG.taxiSpeed.Value;
            extra.ShipApprochSpeed = Logistic_SHIP_CONFIG.approchSpeed.Value;
            extra.ShipEnergyCostForMaxSpeed = Logistic_SHIP_CONFIG.ShipEnergyMaxSpeedMultiplier.Value;
            extra.ShipEnergyCostTakeOff = Logistic_SHIP_CONFIG.ShipEnergyTakeOffMultiplier.Value;
            extra.ShipEnergyCostPerMeter = Logistic_SHIP_CONFIG.ShipEneregyTransitCostMultiplier.Value;
            extra.ShipEnergyCostPerWarp = Logistic_SHIP_CONFIG.ShipEnergyWarpMultiplier.Value;
        }

        /// <summary>
        /// Patches the StationComponent InternalTickLocal method with Transpiler code.
        /// </summary>
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.InternalTickLocal))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalTickLocal_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        )
        {
            if (DEBUG)
            {
                Log.LogInfo("+---------------------------------------------------------+");
                Log.LogInfo("| In StationComponent InternalTickLocal method Transpiler |");
                Log.LogInfo("+---------------------------------------------------------+");
            }

            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            StringCollection debugInstructions = new StringCollection();
            StringCollection expectedInstructions = new StringCollection();

            LocalBuilder taxispeed = il.DeclareLocal(typeof(double));
            LocalBuilder takeoffEnergyMulitplier = il.DeclareLocal(typeof(double));
            LocalBuilder energyPerMeterMultiplier = il.DeclareLocal(typeof(double));

            // replace takeoff cost and flight cost with custom values
            var matcher = new CodeMatcher(codeInstructions, il);
            matcher.Start();
            
            // création de la référence à la variable additionnelle
            // maintenant pour accéder à un champ de l'instance liée de la classe additionnelle
            // je peux juste faire un OpCodes.Ldloc, extraStation suivi d'un Ldfld
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStationComponentData), nameof(ExtraStationComponentData.Get))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.DroneTaxiSpeed))),
                new CodeInstruction(OpCodes.Stloc, taxispeed.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.DroneTakeOffEnergyMultiplier))),
                new CodeInstruction(OpCodes.Stloc, takeoffEnergyMulitplier.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.DroneEnergyPerMeterMultiplier))),
                new CodeInstruction(OpCodes.Stloc, energyPerMeterMultiplier.LocalIndex)
            );

            int iter = 0;

            while (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_S),
                new CodeMatch(OpCodes.Ldc_R4, 8f),
                new CodeMatch(OpCodes.Div),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_0)
            ).IsValid)
            {
                matcher.Advance(-4);
                matcher.SetAndAdvance(OpCodes.Ldloc, taxispeed.LocalIndex);
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Conv_R4)
                );
                matcher.RemoveInstructions(3);
                iter++;
            }
            matcher.Start();
            while (matcher.MatchForward(true, 
                new CodeMatch(OpCodes.Ldc_I4, 800_000)
            ).IsValid)
            {
                matcher.SetAndAdvance(OpCodes.Ldloc, takeoffEnergyMulitplier.LocalIndex);
                matcher.Insert(new CodeInstruction(OpCodes.Conv_I4));
            }

            matcher.Start();
            while (matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R8, 800_000.0)).IsValid)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Advance(-4);
                matcher.Set(OpCodes.Ldloc, energyPerMeterMultiplier.LocalIndex);
                matcher.Advance(4);
                matcher.Set(OpCodes.Ldloc, takeoffEnergyMulitplier.LocalIndex);
                var modified = Helpers.returnInstructions(ref matcher, 10);
                if (DEBUG)
                {
                    for (int i = 0; i < modified.Count && i < origin.Count; i++)
                    {
                        var splited = modified[i].Split(',');
                        Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
                    }
                }
            }

            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// Patches the StationComponent InternalTickLocal method with Transpiler code.
        /// </summary>
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.CalcLocalSingleTripTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcLocalSingleTripTime_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        )
        {
            if (DEBUG)
            {
                Log.LogInfo("+---------------------------------------------------------------+");
                Log.LogInfo("| In StationComponent CalcLocalSingleTripTime method Transpiler |");
                Log.LogInfo("+---------------------------------------------------------------+");
            }

            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            // replace takeoff cost and flight cost with custom values
            var matcher = new CodeMatcher(codeInstructions, il);
            matcher.Start();

            while(matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Ldc_R4, 8f),
                new CodeMatch(OpCodes.Div),
                new CodeMatch(OpCodes.Conv_R8),
                new CodeMatch(OpCodes.Call)
            ).IsValid)
            {
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStationComponentData), nameof(ExtraStationComponentData.Get))),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.DroneTaxiSpeed)))
                );
                matcher.RemoveInstructions(5);
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.CalcTripEnergyCost))]
        [HarmonyPrefix]
        public static bool CalcTripEnergyCost_prefix(ref StationComponent __instance, ref long __result, double trip, float maxSpeed, bool canWarp)
        {
            var extra = ExtraStationComponentData.Get(__instance);

            double num2 = Helpers.ClampDouble(Helpers.MinDouble(trip * 0.3 + 100, maxSpeed), 0, 3000) * extra.ShipEnergyCostForMaxSpeed;
            num2 += extra.ShipEnergyCostPerWarp * Helpers.BoolToInt(canWarp && trip > __instance.warpEnableDist) ;
            
            __result =(long)(extra.ShipEnergyCostTakeOff + trip * extra.ShipEnergyCostPerMeter + num2);

            return false;
        }

        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.CalcRemoteSingleTripTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcRemoteSingleTripTime_transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+----------------------------------------------------------------+");
                Log.LogInfo("| In StationComponent CalcRemoteSingleTripTime method Transpiler |");
                Log.LogInfo("+----------------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var taxi_speed = il.DeclareLocal(typeof(float));
            var approach_speed = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStationComponentData), nameof(ExtraStationComponentData.Get))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipTaxiSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, taxi_speed.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipApprochSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, approach_speed.LocalIndex)
                );

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_R4, 0.4f),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_S)
                );
            if (matcher.IsValid)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Set(OpCodes.Ldloc, taxi_speed.LocalIndex);
                matcher.Advance(4);
                matcher.Set(OpCodes.Ldloc, approach_speed.LocalIndex);

                var modified = Helpers.returnInstructions(ref matcher, 10);
                if (DEBUG)
                {
                    for (int i = 0; i < modified.Count && i < origin.Count; i++)
                    {
                        var splited = modified[i].Split(',');
                        Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
                    }
                }
            }

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.CalcArrivalRemainingTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcArrivalRemainingTime_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+----------------------------------------------------------------+");
                Log.LogInfo("| In StationComponent CalcArrivalRemainingTime method Transpiler |");
                Log.LogInfo("+----------------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var taxi_speed = il.DeclareLocal(typeof(float));
            var approach_speed = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStationComponentData), nameof(ExtraStationComponentData.Get))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipTaxiSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, taxi_speed.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipApprochSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, approach_speed.LocalIndex)
                );

            while (matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldc_R4, 600f),
                new CodeMatch(OpCodes.Div),
                new CodeMatch(OpCodes.Ldc_R4, 0.4f)
                ).IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.SetAndAdvance(OpCodes.Ldloc, taxi_speed.LocalIndex);
                matcher.RemoveInstructions(2);

                matcher.Advance(-1);

                var modified = Helpers.returnInstructions(ref matcher, 10);
                if (DEBUG)
                {
                    for (int i = 0; i < modified.Count && i < origin.Count; i++)
                    {
                        var splited = modified[i].Split(',');
                        Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
                    }
                }
                matcher.Advance(1);
            }

            matcher.Start();

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_R4, 0.4f),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_S)
                );

            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Set(OpCodes.Ldloc, taxi_speed.LocalIndex);
                matcher.Advance(4);
                matcher.Set(OpCodes.Ldloc, approach_speed.LocalIndex);
                matcher.Advance(3);
                matcher.Set(OpCodes.Ldloc, approach_speed.LocalIndex);
                matcher.Advance(2);
                matcher.Set(OpCodes.Ldloc, approach_speed.LocalIndex);
                matcher.Advance(-9);

                var modified = Helpers.returnInstructions(ref matcher, 10);
                if (DEBUG)
                {
                    for (int i = 0; i < modified.Count && i < origin.Count; i++)
                    {
                        var splited = modified[i].Split(',');
                        Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
                    }
                }
            }

            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// Patches the StationComponent internalTickRemote method with Transpiler code.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.InternalTickRemote))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+----------------------------------------------------------+");
                Log.LogInfo("| In StationComponent InternalTickRemote method Transpiler |");
                Log.LogInfo("+----------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var taxi_speed = il.DeclareLocal(typeof(float));
            var approach_speed = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStationComponentData), nameof(ExtraStationComponentData.Get))),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipTaxiSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, taxi_speed.LocalIndex),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraStationComponent), nameof(ExtraStationComponent.ShipApprochSpeed))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, approach_speed.LocalIndex)
                );

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_R4, 0.4f),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_S)
                );
            if (matcher.IsValid)
            {
                Log.LogInfo("found");
                matcher.Set(OpCodes.Ldloc, taxi_speed.LocalIndex);
                matcher.Advance(4);
                matcher.Set(OpCodes.Ldloc, approach_speed.LocalIndex);
            }


            if (DEBUG)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher))
                    Log.LogInfo($"{strInstruction}");
            }

            return matcher.InstructionEnumeration();
        }
    }
}