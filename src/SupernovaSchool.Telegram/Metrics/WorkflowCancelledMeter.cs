using System.Diagnostics.Metrics;

namespace SupernovaSchool.Telegram.Metrics;

public class WorkflowCancelledMeter
{
    public const string MeterName = "supernova.workflow";
    
    private readonly Counter<int> _counter;

    public WorkflowCancelledMeter(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _counter = meter.CreateCounter<int>($"{MeterName}.canceled");
    }

    public void WorkflowStarted(string workflowName)
    {
        _counter.Add(1, new KeyValuePair<string, object?>("workflow_name", workflowName));
    }
}