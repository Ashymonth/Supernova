using Microsoft.Extensions.Logging;
using SupernovaSchool.Telegram.Metrics;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows;

public class DiagnosticMiddleware : IWorkflowMiddleware
{
    private readonly WorkflowStaterCounterMetric _counterMetric;
    private readonly ILogger<DiagnosticMiddleware> _logger;

    public DiagnosticMiddleware(WorkflowStaterCounterMetric counterMetric, ILogger<DiagnosticMiddleware> logger)
    {
        _counterMetric = counterMetric;
        _logger = logger;
    }

    public WorkflowMiddlewarePhase Phase => WorkflowMiddlewarePhase.PreWorkflow;

    public async Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
    {
        _counterMetric.WorkflowStarted(workflow.WorkflowDefinitionId);

        var workflowId = workflow.Id;

        using (_logger.BeginScope("WorkflowId => {@WorkflowId}", workflowId))
        {
            await next();
        }
    }
}