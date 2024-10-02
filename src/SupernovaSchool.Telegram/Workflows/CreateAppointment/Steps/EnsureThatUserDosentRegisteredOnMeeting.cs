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

    public string UserId { get; set; } = null!;

    public DateOnly Date { get; set; }

    public bool HasAppointment { get; set; }
    
    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        HasAppointment =
            await _appointmentService.IsUserHasAppointmentForDateAsync(Date, UserId, context.CancellationToken);
        
        return ExecutionResult.Next();
    }
}