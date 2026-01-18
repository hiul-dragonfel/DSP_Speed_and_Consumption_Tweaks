using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DSP_Speed_and_Consumption_Tweaks.Utils
{
    public class runtime_logger
    {
        private long methodId;
        private long intervalMs;
        private ILGenerator ilGenerator;
        private CodeMatcher codeMatcher;
        public runtime_logger(ILGenerator IL, CodeMatcher cm, long refresh_interval = 1_000L) {
            methodId = Helpers.createMethodId(cm.Instructions());
            ilGenerator = IL;
            codeMatcher = cm;
            intervalMs = refresh_interval;
            var pos = codeMatcher.Pos;
            codeMatcher.Start();
            codeMatcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I8, methodId),
                new CodeInstruction(OpCodes.Ldc_I8, intervalMs),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.reset_timer), new Type[] { typeof(object), typeof(long), typeof(long)}))
            );
            codeMatcher.Advance(pos);
        }

        public void print(string msg)
        {
            Helpers.log_message(ref ilGenerator, ref codeMatcher, methodId, msg);
        }

        public void print(string msg, int local_bool_index)
        {
            Helpers.log_message(ref ilGenerator, ref codeMatcher, methodId, msg, local_bool_index);
        }

        public void print_var(int local_var_index, Type var_type)
        {
            Helpers.log_local_value(ref ilGenerator, ref codeMatcher, methodId, local_var_index, var_type);
        }

        public void print_var(int local_var_index, Type var_type, int local_bool_index)
        {
            Helpers.log_local_value(ref ilGenerator, ref codeMatcher, methodId, local_var_index, var_type, local_bool_index);
        }

        public void print_last_loaded(Type var_type, bool consume = true)
        {
            Helpers.log_local_value(ref ilGenerator, ref codeMatcher, methodId, null, var_type, null, consume);
        }
        public void print_last_loaded(Type var_type, int local_bool_index, bool consume = true)
        {
            Helpers.log_local_value(ref ilGenerator, ref codeMatcher, methodId, null, var_type, local_bool_index, consume);
        }
    }
    public static class Helpers
    {
        public static long createMethodId(IEnumerable<CodeInstruction> instructions)
        {
            var sb = new StringBuilder();
            foreach (var instruction in instructions)
            {
                sb.Append(instruction.opcode.Value);
                if(instruction.operand != null)
                {
                    sb.Append(instruction.operand.GetHashCode());
                }
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);

            return BitConverter.ToInt64(hashBytes, 0);
        }

        public class sw
        {
            private Stopwatch sw_stopwatch = Stopwatch.StartNew();
            private bool b_fire = false;
            public bool fire() { return b_fire; }
            public void restart(long intervalMs) {
                if (b_fire = intervalMs <= sw_stopwatch.ElapsedMilliseconds) sw_stopwatch.Restart(); 
            }
        }

        public sealed class TimerKey
        {
            public object Owner { get; }
            public long Method { get; }

            public TimerKey(object owner, long method)
            {
                Owner = owner;
                Method = method;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(this, obj)) return true;
                if (!(obj is TimerKey other)) return false;
                return ReferenceEquals(Owner, other.Owner) 
                    && Method == other.Method;
            }

            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + RuntimeHelpers.GetHashCode(Owner);
                hash = hash * 31 + Method.GetHashCode();
                return hash;
            }

        }

        private static readonly ConditionalWeakTable<object, Dictionary<TimerKey, sw>> _watches = new ConditionalWeakTable<object, Dictionary<TimerKey, sw>>();

        public static bool ShouldPrint(object owner, long methodeId)
        {
            
            var map = _watches.GetValue(owner, _ => new Dictionary<TimerKey, sw>());
            lock (map)
            {
                var key = new TimerKey(owner, methodeId);
                if (!map.TryGetValue(key, out var cur_sw))
                {
                    cur_sw = new sw();
                    map[key] = cur_sw;
                }
                return cur_sw.fire();
            }
        }

        public static void reset_timer(object owner, long methodeId, long intervalMs)
        {
            var map = _watches.GetValue(owner, _ => new Dictionary<TimerKey, sw>());
            lock (map)
            {
                var key = new TimerKey(owner, methodeId);
                if (!map.TryGetValue(key, out var cur_sw))
                {
                    cur_sw = new sw();
                    map[key] = cur_sw;
                }
                cur_sw.restart(intervalMs);
            }
        }

        /// <summary>
        /// Helper to insert a message to the log at runtime when localBool is true
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="message">string to log</param>
        /// <param name="localBoolIndex">a bool to allow the printing dynamicaly</param>
        public static void log_message(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, string message, int localBoolIndex)
        {
            log_message(ref IL, ref matcher, methodeId, message, localBoolIndex);
        }

        /// <summary>
        /// Helper to insert a message to the log at runtime every interval
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="message">string to log</param>
        public static void log_message(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, string message)
        {
            log_message(ref IL, ref matcher, methodeId, message, null);
        }

        /// <summary>
        /// Helper to insert a message to the log at runtime every interval if localBool is true
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="message">string to log</param>
        /// <param name="localBoolIndex">a bool to allow the printing dynamicaly</param>
        public static void log_message(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, string message, int? localBoolIndex)
        {
            create_jump(ref IL, ref matcher, methodeId, localBoolIndex);
            CodeInstruction[] instructions =
            {
                new CodeInstruction(OpCodes.Ldstr, $"{message}")
            };
            call_logger(ref matcher, instructions);
        }

        /// <summary>
        /// Helper to insert a log of a local variable at runtime
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="localIndex">local variable's index</param>
        /// <param name="type">local variable's type</param>
        public static void log_local_value(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, int localIndex, Type type)
        {
            log_local_value(ref IL, ref matcher, methodeId, localIndex, type, null);
        }

        /// <summary>
        /// Helper to insert a log of the last var loaded at runtime every interval if localBool is true
        /// the value will be consumed
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="type">local variable's type</param>
        /// <param name="localBoolIndex">local bool index to allow the printing dynamicaly</param>
        public static void log_local_value(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, Type type, int? localBoolIndex = null)
        {
            log_local_value(ref IL, ref matcher, methodeId, null, type, localBoolIndex);
        }

        /// <summary>
        /// Helper to insert a log of a local variable at runtime when localBool is true
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="localIndex">local variable's index</param>
        /// <param name="type">local variable's type</param>
        /// <param name="localBoolIndex">local bool index to allow the printing dynamicaly</param>
        public static void log_local_value(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, int localIndex, Type type, int localBoolIndex)
        {
            log_local_value(ref IL, ref matcher, methodeId, localIndex, type, localBoolIndex);
        }

        /// <summary>
        /// Helper to insert a log of a local variable at runtime every interval if localBool is true
        /// </summary>
        /// <param name="IL">IL_Generator from the transpiler</param>
        /// <param name="matcher"></param>
        /// <param name="methodeId">Methode Id created with Helpers.createMethodeId(CodeInstruction [] instrs)</param>
        /// <param name="localIndex">local variable's index</param>
        /// <param name="type">local variable's type</param>
        /// <param name="localBoolIndex">local bool index to allow the printing dynamicaly</param>
        public static void log_local_value(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, int? localIndex, Type type, int? localBoolIndex, bool consume = true)
        {
            
            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("entered in Log_Local_Value:");
            var instructions = new List<CodeInstruction>();
            if (localIndex.HasValue)
            {
                create_jump(ref IL, ref matcher, methodeId, localBoolIndex);
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("has ref to local value");
                if (typeof(string) == type)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("is string");
                    instructions.AddRange(new CodeInstruction[]{
                        new CodeInstruction(OpCodes.Ldstr, $"Valeur Locale ${localIndex} Type : {type} => "),
                        new CodeInstruction(OpCodes.Ldloc_S, localIndex),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), new[] { typeof(string), typeof(string) }))
                    });
                }
                else
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("is not a string");
                    instructions.AddRange(new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldstr, $"Valeur Locale ${localIndex} Type : {type} => "),
                        new CodeInstruction(OpCodes.Ldloc_S, localIndex),
                        new CodeInstruction(OpCodes.Box, type),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), new[] { typeof(object), typeof(object) }))
                    });
                }
            }
            else
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("Has no ref to local value");
                var local = IL.DeclareLocal(type);
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Stloc, local.LocalIndex));
                if (consume) create_jump(ref IL, ref matcher, methodeId, localBoolIndex);
                else create_jump(ref IL, ref matcher, methodeId, localBoolIndex, localIndex.Value);
                if (typeof(string) == type)
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("is a string");
                    instructions.AddRange(new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldstr, $"Loaded Local Value Type : {type} => "),
                        new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), new[] { typeof(string), typeof(string) }))
                    });
                }
                else
                {
                    DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("is not a string");
                    instructions.AddRange(new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldstr, $"Loaded Local Value Type : {type} => "),
                        new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                        new CodeInstruction(OpCodes.Box, type),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), new[] { typeof(object), typeof(object) }))
                    });
                }
            }
            call_logger(ref matcher, instructions.ToArray());
        }
        private static void create_jump(ref ILGenerator IL, ref CodeMatcher matcher, long methodeId, int? printNow = null, int? local_index = null)
        {
            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("entered create_jump");
            var skip_print = IL.DefineLabel();
            if (local_index.HasValue) matcher.Insert(new CodeInstruction(OpCodes.Ldloc, local_index.Value));
            matcher.AddLabels(new Label[] { skip_print });
            if (printNow.HasValue)
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("uses a runtime bool");
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc, printNow.Value),
                    new CodeInstruction(OpCodes.Brfalse, skip_print)
                );
            }
            else
            {
                DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("uses a fixed interval");
                matcher.InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldc_I8, methodeId),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(ShouldPrint))),
                    new CodeInstruction(OpCodes.Brfalse, skip_print)
                );
            }
        }
        private static void call_logger(ref CodeMatcher matcher, CodeInstruction[] instructions)
        {
            DSP_Speed_and_Consumption_Tweaks_Plugin.Log.LogInfo("entered call_logger");
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(DSP_Speed_and_Consumption_Tweaks_Plugin), nameof(DSP_Speed_and_Consumption_Tweaks_Plugin.Log)))
                );
            matcher.InsertAndAdvance(instructions);
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ManualLogSource), nameof(ManualLogSource.LogInfo)))
                );
        }

        /// <summary>
        /// Returns the instructions inside a CodeMatcher in CSV format.
        /// Can also be used to get only a certain number of instructions 
        /// around the current cursor position
        /// </summary>
        /// <param name="codeMatcher"></param>
        /// <param name="NumberOfNeibouringInstructions"></param>
        public static StringCollection returnInstructions(
            ref CodeMatcher codeMatcher,
            int NumberOfNeibouringInstructions = 1_000_000,
            [CallerMemberName] string functionName = "")
        {
            int currentPos = codeMatcher.Pos;
            int last = codeMatcher.Length;
            codeMatcher.Start();
            int[,] IL_address = new int[last, 2];
            int index = 0;
            foreach (var instruction in codeMatcher.Instructions())
            {
                IL_address[index, 0] = GetInstructionSize(instruction);
                if (index == 0) { IL_address[index, 1] = 0; }
                else { IL_address[index, 1] = IL_address[index - 1, 0] + IL_address[index - 1, 1]; }
                index++;
            }
            codeMatcher.Advance(currentPos);

            StringCollection instructions = new StringCollection();
            instructions.Add("FunctionName,Cursor,N°Instruction,IL_SIZE,IL_ADD,OpCode,Operand,Labels");
            int min = (
                codeMatcher.Pos > NumberOfNeibouringInstructions + 1
                    ? -(NumberOfNeibouringInstructions + 1)
                    : -codeMatcher.Pos
            );
            int max = (
                NumberOfNeibouringInstructions + codeMatcher.Pos + 1 > codeMatcher.Length - 1
                    ? codeMatcher.Length - (codeMatcher.Pos + 1)
                    : NumberOfNeibouringInstructions + 1
            );

            int lastInstructionPos = codeMatcher.Length;

            for (int i = min; i <= max; i++)
            {
                instructions.Add(
                    $"{functionName}," +
                    $"{(i == 0 ? ">>>>" : ""), 4}," +
                    $"{codeMatcher.Pos + i,6}, " +
                    $"{IL_address[codeMatcher.Pos + i, 0],4}," +
                    $"Il_{IL_address[codeMatcher.Pos + i, 1],5:X4}," +
                    $"{codeMatcher.InstructionAt(i).opcode.Name,16}, " +
                    $"\"{(codeMatcher.InstructionAt(i).opcode.OperandType == OperandType.InlineBrTarget ? string.Join(" ", Regex.Matches(codeMatcher.InstructionAt(i).ToString(), @"Label[0-9]+").Cast<Match>().Select(m => m.Value)).Trim() : codeMatcher.InstructionAt(i).operand),32}\","
                );
                instructions[instructions.Count - 1] += string.Join(" ", Regex.Matches(codeMatcher.InstructionAt(i).ToString(), @"\[[^]]+\]").Cast<Match>().Select(m => m.Value)).Trim();
            }

            return instructions;
        }

        private static int GetInstructionSize(CodeInstruction instr)
        {
            int size = instr.opcode.Size; // 1 ou 2 bytes

            switch (instr.opcode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                case OperandType.ShortInlineBrTarget:
                    size += 1;
                    break;
                case OperandType.InlineVar:
                    size += 2;
                    break;
                case OperandType.InlineI:
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    size += 4;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    size += 8;
                    break;
                case OperandType.InlineSwitch:
                    int count =
                        instr.operand is int[] ia ? ia.Length :
                        instr.operand is Label[] la ? la.Length :
                        instr.operand is Array a ? a.Length :
                        0;

                    size += 4 + 4 * count;
                    break;
            }
            return size;
        }
        public static double ClampDouble(double val, double min, double max)
        {
            val = MaxDouble(val, min);
            return MinDouble(val, max);
        }
        public static double MinDouble(double a, double b)
        {
            long ia = BitConverter.DoubleToInt64Bits(a);
            long ib = BitConverter.DoubleToInt64Bits(b);

            // MSB = signe → if a > b, (ia - ib) < 0
            long mask = ia - ib >> 63;  // -1 if b > a, 0 if b <= a

            // branchless selection : a*(~mask) + b*mask
            long resultBits = (ia & mask) | (ib & ~mask);
            return BitConverter.Int64BitsToDouble(resultBits);
        }
        public static double MaxDouble(double a, double b)
        {
            long ia = BitConverter.DoubleToInt64Bits(a);
            long ib = BitConverter.DoubleToInt64Bits(b);

            long mask = ib - ia >> 63; // -1 if a > b, 0 if a <= b
            // branchless selection : a*(~mask) + b*mask
            long resultBits = (ia & mask) | (ib & ~mask);
            return BitConverter.Int64BitsToDouble(resultBits);
        }

        public static readonly Func<bool, int> BoolToInt = Create();
        static Func<bool, int> Create()
        {
            var dm = new DynamicMethod(
                "BoolToInt",
                typeof(int),
                new Type[] { typeof(bool) },
                typeof(Helpers).Module,
                true
                );
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Ret);

            return (Func<bool, int>)dm.CreateDelegate(typeof(Func<bool, int>));
        }
    }
}

