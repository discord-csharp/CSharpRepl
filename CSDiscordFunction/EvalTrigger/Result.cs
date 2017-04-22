using System;

namespace CSDiscordFunction
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
            return new Result
            {
                ReturnValue = er.ReturnValue,
                Code = er.Code,
                CompileTime = er.CompileTime,
                ConsoleOut = er.ConsoleOut,
                Exception = er.Exception,
                ExceptionType = er.ExceptionType,
                ExecutionTime = er.ExecutionTime
            };
        }
    }

}