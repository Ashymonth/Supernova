using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;

public class EnsureThatUserIsAdminStep : IStepBody, IUserStep
{
    private readonly IAdminsProvider _iAdminsProvider;

    public EnsureThatUserIsAdminStep(IAdminsProvider iAdminsProvider)
    {
        _iAdminsProvider = iAdminsProvider;
    }

    public string UserId { get; set; } = null!;

    public bool IsAdmin { get; set; }
    
    public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(UserId);
        
        IsAdmin = _iAdminsProvider.IsAdmin(UserId);

        return Task.FromResult(ExecutionResult.Next());
    }
}