using SupernovaSchool.Abstractions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class SendAvailableTimeSlotsStep : IStepBody
{
    private readonly IAppointmentService _appointmentService;

    public SendAvailableTimeSlotsStep(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public Guid TeacherId { get; set; }

    public DateTime DueDate { get; set; }

    public TimeRange[] AvailableSlots { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        AvailableSlots =
            await _appointmentService.FindTeacherAvailableAppointmentSlotsAsync(TeacherId, DueDate,
                context.CancellationToken);

        return ExecutionResult.Next();
    }
}