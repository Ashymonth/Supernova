using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.DeleteTeacher.Steps;

public class DeleteTeacherStep : IUserStep, IStepBody
{
    private readonly ITeacherService _teacherService;

    public DeleteTeacherStep(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    public string UserId { get; set; } = null!;

    public Guid TeacherId { get; set; }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(UserId);

        await _teacherService.DeleteAsync(TeacherId, context.CancellationToken);

        return ExecutionResult.Next();
    }
}