using System;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace CSDiscordFunction.EvalTrigger
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
            ReturnValue = state.ReturnValue == null ? null : JsonConvert.SerializeObject(state.ReturnValue);
            Type = state.ReturnValue?.GetType();
            Code = state.Script.Code;
            Exception = state.Exception?.Message;
            ExceptionType = state.Exception?.GetType().Name;
        }

        public string ReturnValue { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public string Code { get; set; }

        public string ConsoleOut { get; set; }
        
        public Type Type { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public TimeSpan CompileTime { get; set; }
    }

}