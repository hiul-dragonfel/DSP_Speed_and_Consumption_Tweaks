﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;
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
    [HarmonyPatch(typeof(DFRelayComponent))]
    internal class MyDFRelayComponent
    {
        /// <summary>
        /// Patches the Player Awake method with prefix code.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(nameof(DFRelayComponent.CarrierSailLogic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CarrierSailLogic_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var matcher = new CodeMatcher(codeInstructions, il);
            StringCollection expectedInstructions;

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+--------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In DFRelayComponent CarrierSailLogic method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+--------------------------------------------------------+");
                
                if (generateCsvFile)
                {
                    try
                    {
                        using (FileStream FS = new FileStream(pluginPath + "/expectedInstructionsNew.csv", FileMode.Append, FileAccess.Write))
                        {
                            //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError($"this is the content we want to write to file {pluginPath + "/expectedInstructions.csv"}\n{strexpectedInstructions}");

                            StringCollection strexpectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 1000000);
                            byte[] strexpectedInstructionsBytes;
                            foreach (string expectedInstruction in strexpectedInstructions)
                            {
                                strexpectedInstructionsBytes = Encoding.UTF8.GetBytes(
                                    expectedInstruction
                                );
                                strexpectedInstructionsBytes = strexpectedInstructionsBytes.Concat(Encoding.UTF8.GetBytes("\n")).ToArray();

                                //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError($"The length of the Bytes array is {strexpectedInstructionsBytes.Length} and it's count is {strexpectedInstructionsBytes.Count()}");
                                FS.Write(strexpectedInstructionsBytes, 0, strexpectedInstructionsBytes.Length);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError();
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"Error writing to file {pluginPath + "/expectedInstructions.csv"}");

                        //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError();
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError(e.ToString());

                    }

                }
                
            }

            float maxCarrierCruiseSpeed = (float)(
            Config.Dark_Fog_CONFIG.maxCarrierSpeedUnit.Value == "M" ?
                Config.Dark_Fog_CONFIG.maxCarrierSpeed.Value :
                Config.Dark_Fog_CONFIG.maxCarrierSpeedUnit.Value == "AU" ?
                Config.Dark_Fog_CONFIG.maxCarrierSpeed.Value * Config.AU :
                Config.Dark_Fog_CONFIG.maxCarrierSpeed.Value * Config.LY
            ) * 1.5f;

            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"maxCarrierCruiseSpeed is {maxCarrierCruiseSpeed}");
            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"maxCarrierCruiseSpeedUnits is {Config.Dark_Fog_CONFIG.maxCarrierSpeedUnit.Value}");

            for (int i = 0; i < 2; i++)
            {
                matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_R4, (object)3000f),
                    new CodeMatch(OpCodes.Bge_Un)
                );

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------------  carrier target landing site threshold {i} --------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 753);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                if (matcher.IsValid)
                {
                    matcher.Advance(-1);
                    matcher.Set(OpCodes.Ldc_R8, (double)((maxCarrierCruiseSpeed * 5.0/3.0) < 3000.0 ? 3000.0 : maxCarrierCruiseSpeed * 5.0/3.0));

                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  after  ----------------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 773);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }
                }
            }
            
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R8, (object)40000.0)
            );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("------------  carrier update frequency optimization distance  --------------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 753);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
            }

            if (matcher.IsValid)
            {
                matcher.Set(OpCodes.Ldc_R8, (double)((maxCarrierCruiseSpeed * 2.0) < 40000.0 ? 40000.0 : maxCarrierCruiseSpeed * 2.0));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  after  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 773);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, (object)900f)
            );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("------------  max Carrier Cruise acceleration  and deceleration --------------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 753);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
            }

            if (matcher.IsValid)
            {
                matcher.Advance(-2);
                matcher.Set(OpCodes.Ldc_R4, (float)(maxCarrierCruiseSpeed / 10.0));

                matcher.Advance(2);
                matcher.Set(OpCodes.Ldc_R4, (float)(maxCarrierCruiseSpeed / 2.0));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  after  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 773);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }
            }

            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_R4, (object)1800f),
                    new CodeMatch(OpCodes.Ble_Un),
                    new CodeMatch(OpCodes.Ldc_R4, (object)1800f)
                );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("------------  max Carrier Cruise overshoot  --------------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 773);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
            }

            if (matcher.IsValid)
            {
                matcher.Advance(-2);
                matcher.Set(OpCodes.Ldc_R4, maxCarrierCruiseSpeed);

                matcher.Advance(2);
                matcher.Set(OpCodes.Ldc_R4, maxCarrierCruiseSpeed);

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 773);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }
            }

            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_R4, (object)0.3f),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldc_R4, (object)1800.0f),
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Call)
                );

            if(matcher.IsValid)
            {
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("------------  max Carrier Cruise Speed  --------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 777);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                matcher.Advance(-2);
                matcher.Set(OpCodes.Ldc_R4, maxCarrierCruiseSpeed);

                if(DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("-----------------------  after  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 777);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }
            }
            
            if (matcher.IsInvalid)
            {
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 200000);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($" {expectedInstruction}");
                return codeInstructions;
            }
            return matcher.InstructionEnumeration();
        }

        /// <summary>
        /// Patches the Player Awake method with prefix code.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(nameof(DFRelayComponent.RelaySailLogic))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RelaySailLogic_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var matcher = new CodeMatcher(codeInstructions, il);
            StringCollection expectedInstructions;

            float maxRelayCruiseSpeed = (float)(
                    Config.Dark_Fog_CONFIG.maxRelaySpeedUnit.Value == "M" ?
                        Config.Dark_Fog_CONFIG.maxRelaySpeed.Value :
                        Config.Dark_Fog_CONFIG.maxRelaySpeedUnit.Value == "AU" ?
                        Config.Dark_Fog_CONFIG.maxRelaySpeed.Value * Config.AU :
                        Config.Dark_Fog_CONFIG.maxRelaySpeed.Value * Config.LY
                    ) * 2.0f;

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In DFRelayComponent RelaySailLogic method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+------------------------------------------------------+");
                if (generateCsvFile)
                {
                    try
                    {
                        using (FileStream FS = new FileStream(pluginPath + "/expectedInstructionsNew.csv", FileMode.Append, FileAccess.Write))
                        {
                            //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError($"this is the content we want to write to file {pluginPath + "/expectedInstructions.csv"}\n{strexpectedInstructions}");

                            StringCollection strexpectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 1000000);
                            byte[] strexpectedInstructionsBytes;
                            foreach (string expectedInstruction in strexpectedInstructions)
                            {
                                strexpectedInstructionsBytes = Encoding.UTF8.GetBytes(
                                    expectedInstruction
                                );
                                strexpectedInstructionsBytes = strexpectedInstructionsBytes.Concat(Encoding.UTF8.GetBytes("\n")).ToArray();

                                //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError($"The length of the Bytes array is {strexpectedInstructionsBytes.Length} and it's count is {strexpectedInstructionsBytes.Count()}");
                                FS.Write(strexpectedInstructionsBytes, 0, strexpectedInstructionsBytes.Length);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError();
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"Error writing to file {pluginPath + "/expectedInstructions.csv"}");

                        //DSP_Speed_and_Consumption_Tweaks_Plugin.LogError();
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError(e.ToString());

                    }

                }
            }

            matcher.MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && (Convert.ToDouble(i.operand) == 500))
                );
            if (matcher.IsValid)
            {
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("--------------  max Relay Acceleration Speed  --------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 0);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                matcher.Set(opcode: OpCodes.Ldc_R4, operand: 500.0f * (float)(maxRelayCruiseSpeed / 1000.0));

            }

            matcher.MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && (Convert.ToDouble(i.operand) == 1000))
                );
            if (matcher.IsValid)
            {
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("--------------  max Relay Cruise Speed  --------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  before  ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 587);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(maxRelayCruiseSpeed));

                matcher.MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && (Convert.ToSingle(i.operand) == 1000.0f))
                    );

                matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(maxRelayCruiseSpeed));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {

                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("----------------------  after   ----------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 587);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("------------------------------------------------------");
                }
            }

            if (matcher.IsInvalid)
            {
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 2000000);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($" {expectedInstruction}");
                return codeInstructions;
            }

            return matcher.InstructionEnumeration();
        }
    }
}