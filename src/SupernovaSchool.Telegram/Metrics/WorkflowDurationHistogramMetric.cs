using System.Diagnostics.Metrics;

namespace SupernovaSchool.Telegram.Metrics;

public class WorkflowDurationHistogramMetric
{
    public const string MeterName = "supernova.work_flow";

    private readonly Histogram<double> _counter;

    public WorkflowDurationHistogramMetric(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _counter = meter.CreateHistogram<double>($"{MeterName}.duration", "ms");
    }

    public void RecordDuration(string stepName, TimeSpan duration)
    {
        _counter.Record(duration.TotalMilliseconds, new KeyValuePair<string, object?>("workflow_name", stepName));
    }
}