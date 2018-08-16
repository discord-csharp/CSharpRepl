using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

public class FilterStatusProbeTelemetryProcessor : ITelemetryProcessor
{
    private ITelemetryProcessor _next;
    public FilterStatusProbeTelemetryProcessor(ITelemetryProcessor next) => _next = next;
    public void Process(ITelemetry item)
    {
        if (!string.IsNullOrEmpty(item.Context.Operation.SyntheticSource))
        {
            return;
        }

        if(item is RequestTelemetry request && request.Name.Contains("Status/StatusProbeAsync"))
        {
            return;
        }

        _next.Process(item);
    }
}