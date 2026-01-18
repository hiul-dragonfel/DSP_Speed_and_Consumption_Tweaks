using BepInEx.Configuration;
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
using UnityEngine;
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
    internal class ExtraDFTinderComponentPatch
    {
        [HarmonyPatch(typeof(DFTinderComponent), nameof(DFTinderComponent.TinderSailLogic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TinderSailLogic_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+-------------------------------------------------------+");
                Log.LogInfo("| In DFTinderComponent TinderSailLogic method Transpiler |");
                Log.LogInfo("+-------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var SeedSpeedMultiplier = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Dark_Fog_CONFIG), nameof(Dark_Fog_CONFIG.maxSeedSpeedMultiplier))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ConfigEntry<double>), nameof(ConfigEntry<double>.Value))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, SeedSpeedMultiplier.LocalIndex)
                );

            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 240f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, SeedSpeedMultiplier.LocalIndex),
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
                new CodeMatch(OpCodes.Ldc_R4, 360f)
                );
            if (matcher.IsValid)
            {
                matcher.Advance(1);
                Log.LogInfo($"------------- DUMP ----------------");
                var origin = Helpers.returnInstructions(ref matcher, 10);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, SeedSpeedMultiplier.LocalIndex),
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
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1200f));
            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, SeedSpeedMultiplier.LocalIndex),
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

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldc_R4, (object)1200f));
            if (matcher.IsValid)
            {
                var origin = Helpers.returnInstructions(ref matcher, 10);

                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, SeedSpeedMultiplier.LocalIndex),
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

            return matcher.InstructionEnumeration();
        }
    }
}