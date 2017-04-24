using System;
using Newtonsoft.Json;

namespace CSDiscordFunction.EvalTrigger
{
    public class Result
    {
        public object ReturnValue { get; set; }

        public string Exception { get; set; }

        public string ExceptionType { get; set; }

        public string Code { get; set; }

        public string ConsoleOut { get; set; }

        public TimeSpan ExecutionTime { get; set; }

        public TimeSpan CompileTime { get; set; }

        public static Result FromEvalResult(EvalResult er)
        {
            var result = new Result
            {

                Code = er.Code,
                CompileTime = er.CompileTime,
                ConsoleOut = er.ConsoleOut,
                Exception = er.Exception,
                ExceptionType = er.ExceptionType,
                ExecutionTime = er.ExecutionTime
            };

            if (er.Type != null && er.ReturnValue != null)
            {
                result.ReturnValue = JsonConvert.DeserializeObject(er.ReturnValue, er.Type);
            }

            return result;
        }
    }

}