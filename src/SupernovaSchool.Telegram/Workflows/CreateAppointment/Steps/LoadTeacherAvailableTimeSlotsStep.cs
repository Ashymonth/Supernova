using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class LoadTeacherAvailableTimeSlotsStep : IStepBody
{
    private readonly IAppointmentService _appointmentService;

    public LoadTeacherAvailableTimeSlotsStep(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public int SelectedTeacherIndex { get; set; }

    public List<Teacher> Teachers { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public TimeRange[] AvailableSlots { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        AvailableSlots = await _appointmentService.FindTeacherAvailableAppointmentSlotsAsync(
            Teachers[SelectedTeacherIndex].Id, DueDate, context.CancellationToken);

        return ExecutionResult.Next();
    }
}