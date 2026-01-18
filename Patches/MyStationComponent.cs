using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
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
    [HarmonyPatch(typeof(StationComponent))]
    internal class MyStationComponent
    {
        static public bool showFirst = false;
        static public int position = 0;

        /// <summary>
        /// Patches the StationComponent InternalTickLocal method with Transpiler code.
        /// </summary>
        [HarmonyPatch(nameof(StationComponent.InternalTickLocal))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalTickLocal_Transpiler(
            IEnumerable<CodeInstruction> instructions,
            ILGenerator il
        )
        {
            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            StringCollection debugInstructions = new StringCollection();
            StringCollection expectedInstructions = new StringCollection();

            // replace takeoff cost and flight cost with custom values
            var matcher = new CodeMatcher(codeInstructions, il);


            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent InternalTickLocal method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------+");

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
            double maxDroneTaxiSpeed = Config.Logistic_DRONE_CONFIG.DroneMaxTaxiSpeed.Value / 100.0;
            //double maxDroneSpeed = Config.Logistic_DRONE_CONFIG.DroneMaxSpeed.Value;
            double DroneEnergyTakeOff = Config.Logistic_DRONE_CONFIG.DroneEnergyTakeOff.Value;
            double DroneEnergyPerMeter = Config.Logistic_DRONE_CONFIG.DroneEnergyPerMeter.Value;

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ble),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_I4)
            );

            if (matcher.IsValid)
            {
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"----- drone energy take off 1------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  before  ------------");
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 114))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                }
                matcher.Set(OpCodes.Ldc_I4, (int)DroneEnergyTakeOff);
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  after  -------------");
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 114))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                }
            }
            else
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"No match found in InternalTickLocal_Transpiler code incompatible with mod Version {DSP_Speed_and_Consumption_Tweaks_Plugin.VersionString}");
                foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 114))
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"{expectedInstruction}");
                return codeInstructions;
            }
            int patches = 0;
            while (matcher.Pos < instructions.Count() && patches < 2)
            {
                matcher.MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Stloc_S && (i.operand.ToString() == "System.Double (39)" || i.operand.ToString() == "System.Double (58)")),
                    new CodeMatch(i => i.opcode == OpCodes.Ldloc_S && (i.operand.ToString() == "System.Double (37)" || i.operand.ToString() == "System.Double (56)")),
                    new CodeMatch(i => i.opcode == OpCodes.Ldloc_S && (i.operand.ToString() == "System.Double (39)" || i.operand.ToString() == "System.Double (58)")),
                    new CodeMatch(i => i.opcode == OpCodes.Mul),
                    new CodeMatch(i => i.opcode == OpCodes.Stloc_S && (i.operand.ToString() == "System.Double (40)" || i.operand.ToString() == "System.Double (59)")),
                    new CodeMatch(i => i.opcode == OpCodes.Ldloc_S && (i.operand.ToString() == "System.Double (40)" || i.operand.ToString() == "System.Double (59)"))
                );
                if (matcher.IsValid)
                {
                    patches++;
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"----- drone energy per meter ------");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------ drone takeoff energy {patches + 1} ---");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  before  ------------");
                        foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: (patches == 1) ? 385 : 906))
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                    }
                    matcher.Advance(1);
                    matcher.Set(OpCodes.Ldc_R8, DroneEnergyPerMeter);
                    matcher.Advance(4);
                    matcher.Set(OpCodes.Ldc_R8, DroneEnergyTakeOff);

                    matcher.Advance(-5);
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  after  -------------");
                        foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: (patches == 1) ? 385 : 906))
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                    }
                }
                else
                {
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: (patches == 1) ? 385 : 906))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.LogError($"{expectedInstruction}");

                    return codeInstructions;
                }
            }

            // set max taxi speed with custom values
            matcher.Start();
            matcher.MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldarg_S),
                new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && (Convert.ToDouble(i.operand) == 8)),
                new CodeMatch(i => i.opcode == OpCodes.Div),
                new CodeMatch(OpCodes.Call)
                );
            matcher.Advance(-3);
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------ max drone taxi speed -------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  before  ------------");
                foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 39))
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");

            }
            if (matcher.IsValid)
            {
                matcher.Set(
                    OpCodes.Ldc_R4, (float)(maxDroneTaxiSpeed * 8.0)
                );
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  after  -------------");
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 39))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                }
            }
            else
            {

                foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 39))
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"{expectedInstruction}");

                return codeInstructions;
            }
            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldc_R4, (float)1f / 60f),
                new CodeMatch(OpCodes.Ldarg_S),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Stloc_S)
                );
            matcher.Advance(-2);
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------ max drone speed -------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  before  ------------");
                foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 39))
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");

            }
            if (matcher.IsValid)
            {
                /*matcher.Set(
                    OpCodes.Ldc_R4, (float)maxDroneSpeed
                );*/
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  after  -------------");
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 39))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"{expectedInstruction}");
                }
            }
            else
            {

                foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 39))
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"{expectedInstruction}");

                return codeInstructions;
            }
            return matcher.InstructionEnumeration();

        }

        [HarmonyPatch(nameof(StationComponent.DetermineDispatch))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DetermineDispatch_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {


            // get the orginal instructions
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var matcher = new CodeMatcher(codeInstructions, il);

            StringCollection expectedInstructions;
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent DetermineDispatch method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------+");
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


            int patches = 0;
            while (matcher.Pos < instructions.Count() && patches < 2)
            {
                matcher.MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_I4 && (Convert.ToInt32(i.operand) == 6000000))
                );
                if (matcher.IsValid)
                {
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------- vessels energy takeoff -------");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"--------------- before ---------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: (patches == 0) ? 393 : 762);

                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }
                    matcher.Set(opcode: OpCodes.Ldc_I4, operand: (int)Config.Logistic_SHIP_CONFIG.ShipEnergyCostTakeOff.Value);
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------  after ---------------");

                        foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: (patches == 0) ? 393 : 762))
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }
                    patches++;
                }
                else
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"No match found in InternalTickLocal_Transpiler code incompatible with mod Version {DSP_Speed_and_Consumption_Tweaks_Plugin.VersionString}");
                    foreach (string expectedInstruction in DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: (patches == 0) ? 393 : 762))
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

                    return codeInstructions;
                }
            }
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("Match found and patched");
            }
            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(nameof(StationComponent.CalcLocalSingleTripTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcLocalSingleTripTime_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            StringCollection expectedInstructions;

            var matcher = new CodeMatcher(codeInstructions, il);
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent CalcLocalSingleTripTime method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+---------------------------------------------------------------+");
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
                new CodeMatch(i => i.opcode == OpCodes.Ldc_R8 && (Convert.ToDouble(i.operand) == 1.5)),
                new CodeMatch(OpCodes.Ldarg_2)
                );
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---- vessels max Drone Taxi Speed ----");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"--------------- before ---------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 28);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
            }

            double maxDroneTaxiSpeed = Config.Logistic_DRONE_CONFIG.DroneMaxTaxiSpeed.Value / 100.0;
            //double maxDroneSpeed = Config.Logistic_DRONE_CONFIG.DroneMaxSpeed.Value;

            if (matcher.IsValid)
            {
                matcher.Set(OpCodes.Ldc_R4, (float)maxDroneTaxiSpeed);
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------  après  ---------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 28);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------------------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("Match found and patched");
                }

                matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_2)
                );
                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---- vessels max Drone Speed ----");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"--------------- before ---------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 28);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                if (matcher.IsValid)
                {
                    //matcher.Set(OpCodes.Ldc_R4, (float)maxDroneSpeed);
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------  après  ---------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 28);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------------------------");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("Match found and patched");
                    }

                }

            }

            if (matcher.IsInvalid)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($"No match found in InternalTickLocal_Transpiler code incompatible with mod Version {DSP_Speed_and_Consumption_Tweaks_Plugin.VersionString}");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 28);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($" {expectedInstruction}");

                return codeInstructions;
            }

            return matcher.InstructionEnumeration();


        }


        /// <summary>
        /// Patches the StationComponent CalcTripEnergyCost method with Prefix code.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(nameof(StationComponent.CalcTripEnergyCost))]
        [HarmonyPrefix]
        public static bool CalcTripEnergyCost_Prefix(ref long __result, StationComponent __instance, double trip, float maxSpeed, bool canWarp)
        {
            double num = trip * 0.03 + 100.0;
            if (num > (double)maxSpeed)
            {
                num = maxSpeed;
            }
            if (num > 3000.0)
            {
                num = 3000.0;
            }
            double num2 = num * Config.Logistic_SHIP_CONFIG.ShipEnergyCostForMaxSpeed.Value;
            if (canWarp && trip > __instance.warpEnableDist)
            {
                num2 += Config.Logistic_SHIP_CONFIG.ShipEnergyCostPerWarp.Value;
            }

            __result = (long)(Config.Logistic_SHIP_CONFIG.ShipEnergyCostTakeOff.Value + trip * Config.Logistic_SHIP_CONFIG.ShipEneregyCostPerMeter.Value + num2);

            return false;
        }




        //    }
        //    return true;
        //}

        /// <summary>
        /// Patches the StationComponent internalTickRemote method with Transpiler code.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            var matcher = new CodeMatcher(codeInstructions, il);
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent InternalTickRemote method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------+");
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



            double maxCruiseShipSpeed = (double)Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeed.Value * (
                Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "LY" ? Config.LY
                : Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "AU" ? Config.AU
                : 1.0);
            StringCollection expectedInstructions;

            double shipApprochSpeed = (double)Config.Logistic_SHIP_CONFIG.approchSpeed.Value;
            shipApprochSpeed = shipApprochSpeed > maxCruiseShipSpeed ? maxCruiseShipSpeed : shipApprochSpeed;
            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"maxCruiseShipSpeed : {maxCruiseShipSpeed,12}");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"shipApprochSpeed   : {shipApprochSpeed,12}");
            }

            matcher.MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldarg_3),
                new CodeMatch(i => i.opcode == OpCodes.Ldc_R4),
                new CodeMatch(i => i.opcode == OpCodes.Mul),
                new CodeMatch(i => i.opcode == OpCodes.Stloc_S)
                );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"--------------  ship Approch Speed  --------------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("-- Find where to replace atmospheric flight  --");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 152);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

            }

            if (matcher.IsValid)
            {
                matcher.Advance(-3);
                matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(shipApprochSpeed));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  apres  ---------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 152);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                matcher.MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Ldloc_S),
                new CodeMatch(i => i.opcode == OpCodes.Ldc_R4),
                new CodeMatch(i => i.opcode == OpCodes.Mul),
                new CodeMatch(i => i.opcode == OpCodes.Ldc_R4),
                new CodeMatch(i => i.opcode == OpCodes.Add),
                new CodeMatch(i => i.opcode == OpCodes.Stloc_S)
                );

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("-- Find where to replace so the takeoff and Landing is smooth");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 152);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

                }


                if (matcher.IsValid)
                {
                    matcher.Advance(-5);
                    matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(Math.Pow((shipApprochSpeed / 600.0), 0.4)));

                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 152);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }





                    matcher.MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && Convert.ToSingle(i.operand) == 6f),   //ldc.r4 6
                        new CodeMatch(i => i.opcode == OpCodes.Ldloc_S),                                        //ldloc.s 9
                        new CodeMatch(i => i.opcode == OpCodes.Mul),                                            //mul
                        new CodeMatch(i => i.opcode == OpCodes.Add),                                            //add
                        new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && Convert.ToSingle(i.operand) == 0.15f)  //ldc.r4 0.15
                        );
                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-- Find where to modify inbound atmospheric flight");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 1287);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }

                    if (matcher.IsValid)
                    {
                        matcher.Advance(-8);
                        matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(
                                Math.Pow(shipApprochSpeed / 600, 0.4) > 1.0
                                ? Math.Log(Math.Pow(shipApprochSpeed / 600, 0.4)) + 1.0
                                : Math.Pow(shipApprochSpeed / 600, 0.4)
                            )
                        );
                        matcher.Advance(5);
                        matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)Math.Pow(shipApprochSpeed / 600.0, 0.4));

                        matcher.Advance(4);
                        matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(Math.Min(500.0, shipApprochSpeed / 600.0)));

                        if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                        {
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                            expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 1287);
                            foreach (string expectedInstruction in expectedInstructions)
                                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                        }


                        matcher.MatchForward(true,
                            new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && Convert.ToSingle(i.operand) == 0.016666668f),    //ldc.r4 0.016666668
                            new CodeMatch(i => i.opcode == OpCodes.Ldloc_S)                                                  //ldloc.s 9
                        );

                        if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                        {
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-- Find where to modify inbound atmospheric flight");
                            expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 1297);
                            foreach (string expectedInstruction in expectedInstructions)
                                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                        }

                        if (matcher.IsValid)
                        {
                            matcher.Advance(4);
                            matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(shipApprochSpeed * 0.03)
                            );

                            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                            {
                                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 1297);
                                foreach (string expectedInstruction in expectedInstructions)
                                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                            }

                        }
                    }
                }
            }


            if (matcher.IsInvalid)
            {

                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  failed  ---------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 20, expectedInstructionPosition: 177);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

                return codeInstructions;
            }

            return matcher.InstructionEnumeration();

        }

        /// <summary>
        /// Patches the StationComponent internalTickRemote method with Transpiler code.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(nameof(StationComponent.CalcRemoteSingleTripTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcRemoteSingleTripTime_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            float shipSailSpeed = (float)Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeed.Value;
            StringCollection expectedInstructions;

            var matcher = new CodeMatcher(codeInstructions, il);

            double maxCruiseShipSpeed = (double)Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeed.Value * (
                Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "LY" ? Config.LY
                : Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "AU" ? Config.AU
                : 1.0);

            double shipApprochSpeed = (double)Config.Logistic_SHIP_CONFIG.approchSpeed.Value;
            shipApprochSpeed = shipApprochSpeed > maxCruiseShipSpeed ? maxCruiseShipSpeed : shipApprochSpeed;


            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent CalcRemoteSingleTripTime method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------------+");
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
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_R4, (object)0.4f),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_S)
                );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  ship Approch Speed  --------------");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

            }

            if (matcher.IsValid)
            {
                matcher.Advance(-3);
                matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(shipApprochSpeed / 600.0));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_3),
                    new CodeMatch(OpCodes.Ldc_R4, (object)0.03f),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Stloc_S)
                    );

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  ship Approch Speed  --------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

                }

                if (matcher.IsValid)
                {
                    matcher.Advance(-3);
                    matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)shipApprochSpeed);

                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }

                    matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_R4, (object)0.15f),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Ldc_R4, (object)6f),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Add)
                    );

                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"-------------  ship Approch Speed  --------------");
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  avant  ---------------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");

                    }

                    if (matcher.IsValid)
                    {
                        matcher.Advance(-5);
                        matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(shipApprochSpeed / 600));

                        if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                        {
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  après  ---------------------");
                            expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10, expectedInstructionPosition: 81);
                            foreach (string expectedInstruction in expectedInstructions)
                                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                        }
                    }

                }

            }

            if (matcher.IsInvalid)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  FAILED  ---------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10000);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($" {expectedInstruction}");
                return codeInstructions;
            }
            return matcher.InstructionEnumeration();
        }


        /// <summary>
        /// Patches the StationComponent internalTickRemote method with Transpiler code.
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(nameof(StationComponent.CalcArrivalRemainingTime))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CalcArrivalRemainingTime_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {

            var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
            StringCollection expectedInstructions;

            var matcher = new CodeMatcher(codeInstructions, il);

            double maxCruiseShipSpeed = (double)Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeed.Value * (
                Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "LY" ? Config.LY
                : Config.Logistic_SHIP_CONFIG.ShipMaxCruiseSpeedUnits.Value == "AU" ? Config.AU
                : 1.0);

            double shipApprochSpeed = (double)Config.Logistic_SHIP_CONFIG.approchSpeed.Value;
            shipApprochSpeed = shipApprochSpeed > maxCruiseShipSpeed ? maxCruiseShipSpeed : shipApprochSpeed;


            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------------+");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("| In StationComponent CalcArrivalRemainingTime method Transpiler |");
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("+----------------------------------------------------------------+");
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

            for (int i = 1; i <= 4; i++)
            {
                matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_3),
                    new CodeMatch(OpCodes.Ldc_R4, (object)600f),
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Ldc_R4, (object)0.4f),
                    new CodeMatch(OpCodes.Call)
                );

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"--------------------- atmospheric speed {i} found ---------------------");
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------------------------------- before --------------------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 81);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }

                if (matcher.IsValid)
                {


                    matcher.Advance(-4);
                    matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)shipApprochSpeed);

                    if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                    {
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------------------------------- after --------------------------------");
                        expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 81);
                        foreach (string expectedInstruction in expectedInstructions)
                            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                    }
                }
            }

            matcher.MatchForward(true,
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldc_R4, (object)0.4f),
                new CodeMatch(OpCodes.Call)
            );

            if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------------------------------- before --------------------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 81);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
            }

            if (matcher.IsValid)
            {


                matcher.Advance(-2);
                matcher.Set(opcode: OpCodes.Ldc_R4, operand: (float)(shipApprochSpeed / 600.0));

                if (DSP_Speed_and_Consumption_Tweaks_Plugin.DEBUG)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"------------------------------- after --------------------------------");
                    expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 5, expectedInstructionPosition: 81);
                    foreach (string expectedInstruction in expectedInstructions)
                        DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($" {expectedInstruction}");
                }
            }

            if (matcher.IsInvalid)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo($"---------------------  FAILED  ---------------------");
                expectedInstructions = DSP_Speed_and_Consumption_Tweaks_Plugin.returnInstructions(ref matcher, 10000);
                foreach (string expectedInstruction in expectedInstructions)
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogError($" {expectedInstruction}");
                return codeInstructions;
            }

            return matcher.InstructionEnumeration();
        }
    }
}
