using SupernovaSchool.Abstractions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

internal class LoadAvailableMeetingDays : IStepBody
{
    private readonly ITeacherService _meetingService;

    public LoadAvailableMeetingDays(ITeacherService meetingService)
    {
        _meetingService = meetingService;
    }

    public DateTime[] AvailableMeetingDays { get; set; } = null!;

    public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        AvailableMeetingDays = _meetingService.GetAvailableMeetingDates().ToArray();

        return Task.FromResult(ExecutionResult.Next());
    }
}