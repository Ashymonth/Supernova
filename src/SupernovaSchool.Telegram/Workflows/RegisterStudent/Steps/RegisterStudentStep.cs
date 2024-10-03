using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;

public class RegisterStudentStep : IStepBody
{
    private readonly IStudentService _studentService;

    public RegisterStudentStep(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public string UserId { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public string StudentClass { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(StudentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(StudentClass);

        await _studentService.AddOrUpdateAsync(new Student { Id = UserId, Name = StudentName, Class = StudentClass },
            context.CancellationToken);

        return ExecutionResult.Next();
    }
}