using BepInEx.Configuration;
using DSP_Speed_and_Consumption_Tweaks.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Config;
using static DSP_Speed_and_Consumption_Tweaks.DSP_Speed_and_Consumption_Tweaks_Plugin;

namespace DSP_Speed_and_Consumption_Tweaks.Patches
{
    internal class ExtraStarSimulator
    {
        /// <summary>
        /// Patches the StarSimulator UpdateUniversalPosition method with Transpiler code.
        /// This allows to make star flare effect adjustable
        /// </summary>
        [HarmonyPatch(typeof(StarSimulator), nameof(StarSimulator.UpdateUniversalPosition))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateUniversalPosition_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        )
        {
            if (DEBUG)
            {
                Log.LogInfo("+------------------------------------------------------------+");
                Log.LogInfo("| In StarSimulator UpdateUniversalPosition method Transpiler |");
                Log.LogInfo("+------------------------------------------------------------+");
            }

            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            // replace takeoff cost and flight cost with custom values
            var matcher = new CodeMatcher(codeInstructions, il);
            matcher.Start();

            if (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R8, 40000.0),
                new CodeMatch(OpCodes.Div)
            ).IsValid)
            {
                Log.LogInfo($"------ flare multiplier -------");
                Log.LogInfo($"-----------------  before  ----------------");
                foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                    Log.LogInfo($"{expectedInstruction}");

                matcher.Advance(-1).RemoveInstruction();
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StarSimulator), nameof(StarSimulator.starData))),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraStarSimulator), nameof(flareMultiplier)))
                );

                Log.LogInfo($"------ flare multiplier -------");
                Log.LogInfo($"-----------------  after  ----------------");
                foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                    Log.LogInfo($"{expectedInstruction}");
            }

            return matcher.InstructionEnumeration();
        }

        public static double flareMultiplier(StarData currentStart)
        {
            // function used for blending : Ys / (1 + ((x - T) / Xs)^-3)
            // T = 3_097_600_000_000 distance squared to the star for starting to blend when leaving
            // Xs = 90_000_000_000   magic number to stretch the curve in the X-axis
            // Ys = 1.12             magic number to stretch the curve in the Y-Axis
            
            // value is equal to leaveSystem distance squared => ((45 AU) - 1 AU)^2
            const float multiplier_transistion_threshold = 3_097_600_000_000;
            float multiplier_transition = (float)(currentStart.uPosition - GameMain.mainPlayer.uPosition).sqrMagnitude - multiplier_transistion_threshold;
            multiplier_transition = multiplier_transition > 0 ? multiplier_transition : 1.0f;
            multiplier_transition /= 90_000_000_000;
            multiplier_transition =  1 / (multiplier_transition * multiplier_transition * multiplier_transition);
            multiplier_transition = 1.12f / (1 + multiplier_transition);
            multiplier_transition = multiplier_transition > 1 ? 1.0f : multiplier_transition;
            double flareMultiplier = 40000.0 * Mathf.Lerp(StarData_CONFIG.local_star_flare_multiplier.Value, StarData_CONFIG.distant_flare_multiplier.Value, multiplier_transition);
            return flareMultiplier;
        }
    }
}
