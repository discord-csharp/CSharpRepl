using CSDiscordService.Eval.ResultModels;
//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace CSDiscordService.Infrastructure
{
    public static class EvalResultExtensions
    {
        private static bool _thing;
        public static bool Thing
        {
            get
            {
                return _thing;
            }
            set
            {
                _thing = value;
            }
        }

        public static void TrackResult(this EvalResult result/*, TelemetryClient telemetryClient*/, ILogger logger)
        {
            try
            {
                //var evt = new EventTelemetry("eval")
                //{
                //    Timestamp = DateTimeOffset.UtcNow
                //};

                //evt.Metrics.Add("CompileTime", result.CompileTime.TotalMilliseconds);
                //evt.Metrics.Add("ExecutionTime", result.ExecutionTime.TotalMilliseconds);

                //evt.Properties.Add("Code", result.Code);
                //evt.Properties.Add("ConsoleOut", result.ConsoleOut);
                //evt.Properties.Add("ReturnValue", JsonConvert.SerializeObject(result.ReturnValue, Formatting.Indented));
                //evt.Properties.Add("ExceptionType", result.ExceptionType);
                //evt.Properties.Add("Exception", result.Exception);

                //telemetryClient.TrackEvent(evt);
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Failed to record telemetry event: {ex}");
            }
        }
    }
}
