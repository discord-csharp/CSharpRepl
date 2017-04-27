using System;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace CSDiscordService
{
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

            ReturnValue = state.ReturnValue;
            var type = state.ReturnValue?.GetType();
            
            if (type?.GetTypeInfo()?.ImplementedInterfaces?.Contains(typeof(IEnumerator)) ?? false)
            {
                var genericParams = type.GetGenericArguments();

                if (genericParams.Length == 2)
                {
                    type = typeof(List<>).MakeGenericType(genericParams[1]);

                    ReturnValue = Activator.CreateInstance(type, ReturnValue);
                }
            }

            ReturnTypeName = type?.Name;
            ExecutionTime = executionTime;
            CompileTime = compileTime;
            ConsoleOut = consoleOut;
            Code = state.Script.Code;
            Exception = state.Exception?.Message;
            ExceptionType = state.Exception?.GetType().Name;


            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length > 0)
            {
                ReturnTypeName = ReturnTypeName.Replace($"`{genericArgs.Length}", $"<{string.Join(", ", genericArgs.Select(a => a.Name))}>");
            }
        }

        public object ReturnValue { get; set; }

        public string ReturnTypeName { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public string Code { get; set; }

        public string ConsoleOut { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public TimeSpan CompileTime { get; set; }

    }
}