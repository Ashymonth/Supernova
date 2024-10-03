using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class CreateMeetingStep : IStepBody, IUserStep
{
    private readonly IAppointmentService _appointmentService;

    public CreateMeetingStep(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public Guid TeacherId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public TimeRange AppointmentSlot { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var from = AppointmentDate.Date.Add(AppointmentSlot.Start.ToTimeSpan());
        var to = AppointmentDate.Date.Add(AppointmentSlot.End.ToTimeSpan());

        await _appointmentService.CreateAppointment(TeacherId, UserId, from, to);

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}