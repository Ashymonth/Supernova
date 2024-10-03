using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class EnsureThatUserDosentRegisteredOnMeeting : IUserStep, IStepBody
{
    private readonly IAppointmentService _appointmentService;

    public EnsureThatUserDosentRegisteredOnMeeting(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public DateOnly Date { get; set; }

    public bool HasAppointment { get; set; }

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var appointments =
            await _appointmentService.GetStudentAppointmentsAsync(Date.ToDateTime(new TimeOnly()),
                Date.ToDateTime(new TimeOnly(23, 59)), UserId, context.CancellationToken);

        HasAppointment = appointments.Count != 0;

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}