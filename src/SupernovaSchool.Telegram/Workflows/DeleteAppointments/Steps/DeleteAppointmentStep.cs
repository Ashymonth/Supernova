using SupernovaSchool.Abstractions;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;

public class DeleteAppointmentStep : IUserStep, IStepBody
{
    private readonly IAppointmentService _appointmentService;

    public DeleteAppointmentStep(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }
    
    public string UserId { get; set; } = null!;
    
    public DateTime AppointmentDay { get; set; }
    
    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        await _appointmentService.DeleteStudentAppointmentAsync(AppointmentDay, UserId, context.CancellationToken);

        return ExecutionResult.Next();
    }

}