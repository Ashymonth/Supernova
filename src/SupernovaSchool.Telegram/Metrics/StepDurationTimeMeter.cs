using System.Diagnostics.Metrics;

namespace SupernovaSchool.Telegram.Metrics;

public class StepDurationTimeMeter
{
    public const string MeterName = "supernova.worklofw.step";

    private readonly Gauge<double> _counter;

    public StepDurationTimeMeter(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _counter = meter.CreateGauge<double>($"{MeterName}.duration", "ms");
    }

    public void RecordStepDuration(string stepName, TimeSpan duration)
    {
        _counter.Record(duration.TotalMilliseconds, new KeyValuePair<string, object?>("step_name", stepName));
    }
}