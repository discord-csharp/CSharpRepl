using System;
using Microsoft.CodeAnalysis.Scripting;

namespace CSDiscordFunction
{
    [Serializable]
    public class EvalResult
    {
        public EvalResult()
        {
        }

        public EvalResult(ScriptState<object> state, string consoleOut, TimeSpan executionTime, TimeSpan compileTime)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            ExecutionTime = executionTime;
            CompileTime = compileTime;
            ConsoleOut = consoleOut;
            ReturnValue = state.ReturnValue;
            Code = state.Script.Code;
            Exception = state.Exception?.Message;
            ExceptionType = state.Exception?.GetType().Name;
        }

        public object ReturnValue { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public string Code { get; set; }

        public string ConsoleOut { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public TimeSpan CompileTime { get; set; }
    }

}