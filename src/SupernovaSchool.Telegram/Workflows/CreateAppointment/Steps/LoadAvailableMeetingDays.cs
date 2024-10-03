using SupernovaSchool.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using IDateTimeProvider = SupernovaSchool.Abstractions.IDateTimeProvider;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

internal class LoadAvailableMeetingDays : IStepBody
{
    private readonly IDateTimeProvider _timeProvider;

    public LoadAvailableMeetingDays(IDateTimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTime[] AvailableMeetingDays { get; set; } = null!;

    public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        AvailableMeetingDays = _timeProvider.Now.GetTeacherWorkingDays().ToArray();

        return Task.FromResult(ExecutionResult.Next());
    }
}