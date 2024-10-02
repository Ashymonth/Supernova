using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class SendAvailableTimeSlotsStep : IStepBody
{
    private readonly ITeacherService _teacherService;

    public SendAvailableTimeSlotsStep(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    public Guid TeacherId { get; set; }

    public DateTime DueDate { get; set; }

    public TimeRange[] AvailableSlots { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        AvailableSlots = await _teacherService.FindAvailableTimeSlots(TeacherId, DueDate, context.CancellationToken);

        return ExecutionResult.Next();
    }
}