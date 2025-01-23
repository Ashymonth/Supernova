using Microsoft.Extensions.Logging;
using SupernovaSchool.Abstractions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;

public class CreateTeacherStep : IStepBody
{
    private readonly ITeacherService _teacherService;
    private readonly ILogger<CreateTeacherStep> _logger;
    
    public CreateTeacherStep(ITeacherService teacherService, ILogger<CreateTeacherStep> logger)
    {
        _teacherService = teacherService;
        _logger = logger;
    }

    public string Name { get; set; } = null!;

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsTeacherCreated { get; set; }
    
    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(Login);
        ArgumentException.ThrowIfNullOrWhiteSpace(Password);

        try
        {
            await _teacherService.CreateAsync(Name, Login, Password, context.CancellationToken);
            IsTeacherCreated = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating a teacher");
            IsTeacherCreated = false;
        }
        
        return ExecutionResult.Next();
    }
}