using SupernovaSchool.Abstractions;
using SupernovaSchool.Extensions;
using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;

public class LoadMyAppointmentsStep : IStepBody, IUserStep
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDateTimeProvider _timeProvider;

    public LoadMyAppointmentsStep(IAppointmentService appointmentService, IDateTimeProvider timeProvider)
    {
        _appointmentService = appointmentService;
        _timeProvider = timeProvider;
    }

    public IReadOnlyCollection<StudentAppointmentInfo> StudentAppointments { get; set; } = [];

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(UserId);

        var workingDays = _timeProvider.Now.GetTeacherWorkingDays().ToArray();
        StudentAppointments = await _appointmentService.GetStudentAppointmentsAsync(workingDays.Min().Date,
            workingDays.Max().Date.AddHours(23).AddMinutes(59), UserId, context.CancellationToken);

        return ExecutionResult.Next();
    }

    public string UserId { get; set; } = null!;
}