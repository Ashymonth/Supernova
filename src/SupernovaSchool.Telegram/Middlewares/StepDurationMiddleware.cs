using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SupernovaSchool.Telegram.Metrics;
using SupernovaSchool.Telegram.Steps;
using Telegram.Bot;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Exception = System.Exception;

namespace SupernovaSchool.Telegram.Middlewares;

public class StepDurationMiddleware : IWorkflowStepMiddleware
{
    private readonly StepDurationTimeMeter _meter;
    private readonly IUserSessionStorage _userSessionStorage;
    private readonly ITelegramBotClientWrapper _botClient;
    private readonly ILogger<StepDurationMiddleware> _logger;

    public StepDurationMiddleware(StepDurationTimeMeter meter, ILogger<StepDurationMiddleware> logger,
        IUserSessionStorage userSessionStorage, ITelegramBotClientWrapper botClient)
    {
        _meter = meter;
        _logger = logger;
        _userSessionStorage = userSessionStorage;
        _botClient = botClient;
    }

    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body,
        WorkflowStepDelegate next)
    {
        _logger.BeginScope("Executing workflow with id: {WorkflowId}", context.Workflow.Id);

        var sw = Stopwatch.GetTimestamp();

        try
        {
            var result = await next();

            var duration = Stopwatch.GetElapsedTime(sw);

           // _meter.RecordStepDuration(context.Step.Name, duration);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Step execution failed");
            if (context.Workflow.Data is IUserStep userStep)
            {
                await _userSessionStorage.TerminateWorkflow(userStep.UserId, false);
                await _botClient.SendMessage(userStep.UserId, "Во время выполнение произошла ошибка. Попробуйте снова.");
            }

            return null!;
        }
    }
}