using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class EnsureThatStudentRegisteredStep : IUserStep, IStepBody
{
    private readonly IStudentService _studentService;

    public EnsureThatStudentRegisteredStep(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public bool IsStudentRegistered { get; set; }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(UserId);

        IsStudentRegistered = await _studentService.GetStudentAsync(UserId) is not null;

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}