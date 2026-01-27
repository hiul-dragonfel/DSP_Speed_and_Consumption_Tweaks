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
    internal class ExtraDysonSphere
    {
        [HarmonyPatch(typeof(GameLogic), "_dyson_sphere_rocket_parallel")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> _dyson_sphere_rocket_parallel(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+-----------------------------------------------------------------+");
                Log.LogInfo("| In StationComponent _dyson_sphere_rocket_parallel method Transpiler |");
                Log.LogInfo("+-----------------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

            var rocketSpeedMultiplier = il.DeclareLocal(typeof(float));

            var matcher = new CodeMatcher(codeInstructions, il);

            var rt = new runtime_logger(il, matcher);

            matcher.Start();
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DysonSphere_CONFIG), nameof(DysonSphere_CONFIG.RocketSpeedMutliplier))),
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ConfigEntry<double>), nameof(ConfigEntry<double>.Value))),
                new CodeInstruction(OpCodes.Conv_R4),
                new CodeInstruction(OpCodes.Stloc, rocketSpeedMultiplier.LocalIndex)
                );

            if (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 7.5f)
                ).IsValid)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, rocketSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

                matcher.Advance(-1);
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
            }

            if (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 18f)
                ).IsValid)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, rocketSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );

                matcher.Advance(-1);
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
            }

            if (matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, 2800f)
                ).IsValid)
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc, rocketSpeedMultiplier.LocalIndex),
                    new CodeInstruction(OpCodes.Mul)
                    );
                
                matcher.Advance(-1);
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
            }

            return matcher.Instructions();
        }

        [HarmonyPatch(typeof(GameLogic), "_ejector_parallel")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> _ejector_parallel_transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+--------------------------------------------------------------------+");
                Log.LogInfo("| In GameLogic _ejector_parallel_transpiler method Transpiler |");
                Log.LogInfo("+--------------------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var value = il.DeclareLocal(typeof(double));
            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            if (matcher.MatchForward(
                true,
                new CodeMatch(OpCodes.Ldc_R8, 5000.0)).IsValid
                )
            {
                var pos = matcher.Pos;
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.StaticFieldRefAccess<ConfigEntry<double>>(typeof(DysonSphere_CONFIG), nameof(DysonSphere_CONFIG.SolarSailBulletSpeedMutliplier))),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ConfigEntry<double>), nameof(ConfigEntry<double>.Value))),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Stloc, value.LocalIndex)
                    );
                matcher.Start().Advance(pos);
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
            }

            return matcher.Instructions();
        }

        [HarmonyPatch(typeof(GameLogic), "_dyson_sphere_bullet_parallel")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> _dyson_sphere_bullet_parallel_transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            if (DEBUG)
            {
                Log.LogInfo("+--------------------------------------------------------------------------------+");
                Log.LogInfo("| In GameLogic _dyson_sphere_bullet_parallel_transpiler method Transpiler |");
                Log.LogInfo("+--------------------------------------------------------------------------------+");
            }

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            //var value = il.DeclareLocal(typeof(float));
            var matcher = new CodeMatcher(codeInstructions, il);

            matcher.Start();
            Log.LogInfo($"------------- DUMP ----------------");
            foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                Log.LogInfo($"{strInstruction}");
            if (matcher.MatchForward(
                true,
                new CodeMatch(OpCodes.Ldc_R4, 1f / 60f)).IsValid
                )
            {
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
                matcher.Advance(1);
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DysonSphere_CONFIG), nameof(DysonSphere_CONFIG.SolarSailBulletSpeedMutliplier))),
                    new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ConfigEntry<double>), nameof(ConfigEntry<double>.Value))),
                    new CodeInstruction(OpCodes.Conv_R4),
                    new CodeInstruction(OpCodes.Mul)
                    );

                matcher.Advance(-1);
                Log.LogInfo($"------------- DUMP ----------------");
                foreach (string strInstruction in Helpers.returnInstructions(ref matcher, 10))
                    Log.LogInfo($"{strInstruction}");
            }

            return matcher.Instructions();
        }
    }
}
