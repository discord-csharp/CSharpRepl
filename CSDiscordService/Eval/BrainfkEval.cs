using CSDiscordService.Eval.ResultModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using System.IO;
using System.Runtime.InteropServices;

namespace CSDiscordService.Eval
{
    public class BrainfkEval
    {
        public static void ZeroOutMemory(IntPtr ptr, int size)
        {
            for (int I = 0; I < size; ++I)
                Marshal.WriteByte(ptr, I, 0);
        }
        public delegate IntPtr BrainFkCompile(StringBuilder builder);
        public string CompileAndEval(string code)
        {
            var WhileBeginMarkStack = new Stack<Label>();
            var WhileEndMarkStack = new Stack<Label>();
            var reader = new StringReader(code);

            var method = new DynamicMethod($"BrainFk{Guid.NewGuid().ToString().Replace("-", "_")}", typeof(IntPtr), new Type[] { typeof(StringBuilder) });
            var il = method.GetILGenerator();
            // Set the locals
            il.DeclareLocal(typeof(byte*));
            il.DeclareLocal(typeof(IntPtr));
            // Allocate pointer
            il.Emit(OpCodes.Ldc_I4, 1048576); // 1 MB Memory
            il.EmitCall(OpCodes.Call, typeof(Marshal).GetMethod("AllocHGlobal", new Type[] { typeof(int) }), null);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloca_S, 1);
            il.EmitCall(OpCodes.Call, typeof(IntPtr).GetMethod("ToPointer"), null);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4, 1048576); // 1 MB Memory
            il.EmitCall(OpCodes.Call, typeof(BrainfkEval).GetMethod("ZeroOutMemory"), null);
            int currentCharacter = -1;
            do
            {
                currentCharacter = reader.Read();
                switch ((char)currentCharacter)
                {
                    case '>':
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Add);
                            il.Emit(OpCodes.Stloc_0);
                            break;
                        }
                    case '<':
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldc_I4_1);
                            il.Emit(OpCodes.Sub);
                            il.Emit(OpCodes.Stloc_0);
                            break;
                        }
                    case '+':
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldind_U1);
                            int increment = 1;
                            while (reader.Peek() == '+')
                            {
                                ++increment;
                                reader.Read();
                            }
                            il.Emit(OpCodes.Ldc_I4, increment);
                            il.Emit(OpCodes.Add);
                            il.Emit(OpCodes.Conv_U1);
                            il.Emit(OpCodes.Stind_I1);
                            break;
                        }
                    case '-':
                        {
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldind_U1);
                            int decrement = 1;
                            while (reader.Peek() == '-')
                            {
                                ++decrement;
                                reader.Read();
                            }
                            il.Emit(OpCodes.Ldc_I4, decrement);
                            il.Emit(OpCodes.Sub);
                            il.Emit(OpCodes.Conv_U1);
                            il.Emit(OpCodes.Stind_I1);
                            break;
                        }
                    case '.':
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldind_U1);
                            il.EmitCall(OpCodes.Call, typeof(StringBuilder).GetMethod("Append", new[] { typeof(byte) }), null);
                            break;
                        }
                    case '[':
                        {
                            Label begin = il.DefineLabel();
                            Label end = il.DefineLabel();
                            il.Emit(OpCodes.Br, end);
                            il.MarkLabel(begin);
                            WhileBeginMarkStack.Push(begin);
                            WhileEndMarkStack.Push(end);
                            break;
                        }
                    case ']':
                        {
                            il.MarkLabel(WhileEndMarkStack.Pop());
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldind_U1);
                            il.Emit(OpCodes.Brtrue, WhileBeginMarkStack.Pop());
                            break;
                        }
                    default:
                        continue;
                }
            } while (currentCharacter > -1);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ret);
            var builder = new StringBuilder();
            ((BrainFkCompile)method.CreateDelegate(typeof(BrainFkCompile))).Invoke(builder);
            return builder.ToString();
        }

        public EvalResult RunEval(string code)
        {
            var sw = Stopwatch.StartNew();
            var eval = CompileAndEval(code);
            sw.Stop();

            var compileTime = sw.Elapsed;
            var evalResult = new EvalResult(null, eval, sw.Elapsed, compileTime);

            //this hack is to test if we're about to send an object that can't be serialized back to the caller.
            //if the object can't be serialized, return a failure instead.
            try
            {
                JsonConvert.SerializeObject(evalResult);
            }
            catch (Exception ex)
            {
                evalResult = new EvalResult
                {
                    Code = code,
                    CompileTime = compileTime,
                    ConsoleOut = eval,
                    ExecutionTime = sw.Elapsed,
                    ReturnTypeName = "Brainfk Type",
                    ReturnValue = $"An exception occurred when serializing the response: {ex.GetType().Name}: {ex.Message}"
                };
            }
            return evalResult;
        }
    }
}
