using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Steps;

public class CleanupStep : IUserStep, IStepBody
{
    private readonly IUserSessionStorage _userSessionStorage;

    public CleanupStep(IUserSessionStorage userSessionStorage)
    {
        _userSessionStorage = userSessionStorage;
    }

    public string UserId { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(UserId);

        await _userSessionStorage.TerminateWorkflow(UserId, false, context.CancellationToken);

        return ExecutionResult.Next();
    }
}