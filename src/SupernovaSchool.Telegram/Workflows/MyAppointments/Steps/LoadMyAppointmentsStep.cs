using SupernovaSchool.Abstractions;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

namespace SupernovaSchool.Telegram.Workflows.MyAppointments.Steps;

public class LoadMyAppointmentsStep : IStepBody, IUserStep
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDateTimeProvider _timeProvider;

    public LoadMyAppointmentsStep(IAppointmentService appointmentService, IDateTimeProvider timeProvider)
    {
        _appointmentService = appointmentService;
        _timeProvider = timeProvider;
    }

    public string UserId { get; set; } = null!;

    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointments { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(UserId);

        StudentAppointments = await _appointmentService.GetStudentAppointmentsAsync(
            DateOnly.FromDateTime(_timeProvider.Now), UserId, context.CancellationToken);
        
        return ExecutionResult.Next();
    }
}