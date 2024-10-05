using SupernovaSchool.Abstractions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;

public class CreateTeacherStep : IStepBody
{
    private readonly ITeacherService _teacherService;

    public CreateTeacherStep(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    public string Name { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;
    
    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(Login);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);

        await _teacherService.CreateAsync(Name, Login, Password, context.CancellationToken);
        
        return ExecutionResult.Next();
    }
}