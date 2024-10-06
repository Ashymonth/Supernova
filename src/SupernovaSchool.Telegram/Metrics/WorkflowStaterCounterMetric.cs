using System.Diagnostics.Metrics;

namespace SupernovaSchool.Telegram.Metrics;

public class WorkflowStaterCounterMetric
{
    private readonly Counter<int> _counter;

    public WorkflowStaterCounterMetric(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("SupernovaSchool.Telegram");
        _counter = meter.CreateCounter<int>("supernova.workflow.started");
    }

    public void WorkflowStarted(string workflowName)
    {
        _counter.Add(1, new KeyValuePair<string, object?>("supernova.workflow.name", workflowName));
    }
}