using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SupernovaSchool.Telegram.Metrics;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Middlewares;

public class StepDurationMiddleware : IWorkflowStepMiddleware
{
    private readonly StepDurationTimeMeter _meter;
    private readonly ILogger<StepDurationMiddleware> _logger;
    
    public StepDurationMiddleware(StepDurationTimeMeter meter, ILogger<StepDurationMiddleware> logger)
    {
        _meter = meter;
        _logger = logger;
    }

    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body,
        WorkflowStepDelegate next)
    {
        _logger.BeginScope("Executing workflow with id: {WorkflowId}", context.Workflow.Id);
        
        var sw = Stopwatch.GetTimestamp();

        var result = await next();

        var duration = Stopwatch.GetElapsedTime(sw);

        _meter.RecordStepDuration(context.Step.Name, duration);

        return result;
    }
}