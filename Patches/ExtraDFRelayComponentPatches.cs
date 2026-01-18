using DSP_Speed_and_Consumption_Tweaks.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using UnityEngine;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Config;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{
    // TODO Review this file and update to your own requirements, or remove it altogether if not required

    public class ExtraDFRelayComponent
    {
        public bool showFirst = false;
        public int position = 0;

        private double _carrier_Speed_Multiplier = 1;
        private double _relay_Speed_Multiplier = 1;
        public double Carrier_Speed_Multiplier { get { return _carrier_Speed_Multiplier; } set { _carrier_Speed_Multiplier = value; } }
        public double Relay_Speed_Multiplier   { get { return _relay_Speed_Multiplier; } set { _relay_Speed_Multiplier = value; } }

    }

    public static class ExtraDFRelayComponentData
    {
        private static readonly ConditionalWeakTable<DFRelayComponent, ExtraDFRelayComponent> table
            = new ConditionalWeakTable<DFRelayComponent, ExtraDFRelayComponent>();

        public static ExtraDFRelayComponent Get(DFRelayComponent instance) => table.GetOrCreateValue(instance);
    }

    /// <summary>
    /// Sample Harmony Patch class. Suggestion is to use one file per patched class
    /// though you can include multiple patch classes in one file.
    /// Below is included as an example, and should be replaced by classes and methods
    /// for your mod.
    /// </summary>
    static public class ExtraDFRelayComponentPatches
    {
        [HarmonyPatch(typeof(DFRelayComponent), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void DFRelayComponent_ctor_Postfix(DFRelayComponent __instance)
        {
            var extra = ExtraDFRelayComponentData.Get(__instance);
            extra.Carrier_Speed_Multiplier = Dark_Fog_CONFIG.maxCarrierSpeedMultiplier.Value;
            extra.Relay_Speed_Multiplier = Dark_Fog_CONFIG.maxRelaySpeedMultiplier.Value;
        }

        [HarmonyPatch(typeof(DFRelayComponent), nameof(DFRelayComponent.RelaySailLogic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RelaySailLogic_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+------------------------------------------------------+");
                Log.LogInfo("| In DFRelayComponent RelaySailLogic method Transpiler |");
                Log.LogInfo("+------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var RelaySpeedMultiplier = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraDFRelayComponentData), nameof(ExtraDFRelayComponentData.Get))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraDFRelayComponent), nameof(ExtraDFRelayComponent.Relay_Speed_Multiplier))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, RelaySpeedMultiplier.LocalIndex)
                );

            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 500f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );
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

            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 400f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );
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

            matcher.Start();
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1000f));
            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

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

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1000f));
            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

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

            //matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1000f));
            //if (matcher.IsValid)
            //{
            //    var origin = Helpers.returnInstructions(ref matcher, 20);

            //    matcher.Advance(-3);
            //    matcher.InsertAndAdvance(
            //        new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
            //        new CodeInstruction(OpCodes.Mul)
            //        );

            //    matcher.Advance(4);
            //    matcher.InsertAndAdvance(
            //        new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
            //        new CodeInstruction(OpCodes.Mul)
            //        );

            //    matcher.Advance(4);

            //    matcher.InsertAndAdvance(
            //        new CodeInstruction(OpCodes.Ldloc, RelaySpeedMultiplier.LocalIndex),
            //        new CodeInstruction(OpCodes.Div)
            //        );

            //    var modified = Helpers.returnInstructions(ref matcher, 20);
            //    if (DEBUG)
            //    {
            //        for (int i = 0; i < modified.Count && i < origin.Count; i++)
            //        {
            //            var splited = modified[i].Split(',');
            //            Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
            //        }
            //    }
            //    matcher.Advance(1);
            //}

            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// Patches the Player Awake method with prefix code.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(DFRelayComponent), nameof(DFRelayComponent.CarrierSailLogic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CarrierSailLogic_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+--------------------------------------------------------+");
                Log.LogInfo("| In DFRelayComponent CarrierSailLogic method Transpiler |");
                Log.LogInfo("+--------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var CarrierSpeedMultiplier = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraDFRelayComponentData), nameof(ExtraDFRelayComponentData.Get))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ExtraDFRelayComponent), nameof(ExtraDFRelayComponent.Carrier_Speed_Multiplier))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, CarrierSpeedMultiplier.LocalIndex)
                );

            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 180f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, CarrierSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );
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

            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 900f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, CarrierSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );
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

            matcher.Start();
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1800f));
            if(matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, CarrierSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

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

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1800f));
            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, CarrierSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

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

            //matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1800f));
            //if (matcher.IsValid)
            //{

            //    matcher.Advance(6);
            //    var origin = Helpers.returnInstructions(ref matcher, 10);

            //    matcher.Insert(
            //        new CodeInstruction(OpCodes.Ldloc, 80),
            //        new CodeInstruction(OpCodes.Ldloc, CarrierSpeedMultiplier.LocalIndex),
            //        new CodeInstruction(OpCodes.Mul),
            //        new CodeInstruction(OpCodes.Stloc, 80)
            //        );

            //    var modified = Helpers.returnInstructions(ref matcher, 10);
            //    if (DEBUG)
            //    {
            //        for (int i = 0; i < modified.Count && i < origin.Count; i++)
            //        {
            //            var splited = modified[i].Split(',');
            //            Log.LogInfo($"{origin[i]},{string.Join(",", splited, 1, splited.Length - 1)}");
            //        }
            //    }
            //    matcher.Advance(1);
            //}

            return matcher.InstructionEnumeration();
        }
    }
}