using System.Diagnostics;
using SupernovaSchool.Telegram.Metrics;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Middlewares;

public class StepDurationMiddleware : IWorkflowStepMiddleware
{
    private readonly StepDurationTimeMeter _meter;

    public StepDurationMiddleware(StepDurationTimeMeter meter)
    {
        _meter = meter;
    }

    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body, WorkflowStepDelegate next)
    {
        var sw = Stopwatch.GetTimestamp();

        var result = await next();

        var duration = Stopwatch.GetElapsedTime(sw);

        _meter.RecordStepDuration(context.Step.Name, duration);

        return result;
    }
}