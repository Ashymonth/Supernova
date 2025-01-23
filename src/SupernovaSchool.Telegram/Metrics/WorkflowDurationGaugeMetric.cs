using System.Diagnostics.Metrics;

namespace SupernovaSchool.Telegram.Metrics;

public class WorkflowDurationGaugeMetric
{
    public const string MeterName = "supernova.workflow";

    private readonly Gauge<double> _gauge;
 
    public WorkflowDurationGaugeMetric(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _gauge = meter.CreateGauge<double>($"{MeterName}.duration", "ms");
    }

    public void RecordDuration(string stepName, TimeSpan duration)
    {
        _gauge.Record(duration.TotalMilliseconds, new KeyValuePair<string, object?>("workflow_name", stepName));
    }
}