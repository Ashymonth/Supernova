using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class LoadTeachersStep : IStepBody
{
    private readonly IRepository<Teacher> _teacherRepository;
    public LoadTeachersStep(IRepository<Teacher> teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public string UserId { get; set; } = null!;

    public List<Teacher> Teachers { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(UserId);
        
        Teachers = await _teacherRepository.ListAsync();

        return ExecutionResult.Next();
    }
}