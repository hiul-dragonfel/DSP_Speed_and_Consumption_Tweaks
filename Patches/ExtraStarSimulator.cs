using BepInEx.Configuration;
using DSP_Speed_and_Consumption_Tweaks.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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

            while (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StarSimulator), nameof(StarSimulator.starData))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StarData), nameof(StarData.radius))),
                new CodeMatch(OpCodes.Div)
            ).IsValid)
            {
                
                Log.LogInfo($"------ Desceleration rate multiplier -------");
                Log.LogInfo($"-----------------  before  ----------------");
                foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                    Log.LogInfo($"{expectedInstruction}");
               
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(StarData_CONFIG), nameof(StarData_CONFIG.flare_multiplier))),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ConfigEntry<float>), nameof(ConfigEntry<float>.Value))),
                    new CodeInstruction(OpCodes.Div)
                );

                Log.LogInfo($"------ Desceleration rate multiplier -------");
                Log.LogInfo($"-----------------  before  ----------------");
                foreach (string expectedInstruction in Helpers.returnInstructions(ref matcher, 5))
                    Log.LogInfo($"{expectedInstruction}");
            
            }

            return matcher.InstructionEnumeration();
        }
    }
}
