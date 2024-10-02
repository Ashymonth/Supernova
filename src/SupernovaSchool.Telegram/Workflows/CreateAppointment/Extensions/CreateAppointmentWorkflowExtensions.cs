using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Primitives;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Extensions;

internal static class CreateAppointmentWorkflowExtensions
{
    public static IStepBuilder<CreateAppointmentWorkflowData, While> SendAvailableMetingDates<TStep>(
        this IStepBuilder<CreateAppointmentWorkflowData, TStep> builder) where TStep : IStepBody
    {
        return builder.Then<LoadAvailableMeetingDays>()
            .Output(data => data.AvailableMeetingDays, days => days.AvailableMeetingDays)
            .SendMessageWithPagination(data => !data.IsMeetingDateValid(),
                workflowBuilder =>
                {
                    workflowBuilder
                        .SendVariantsPage("Выберите дату для записи", data => data.AvailableMeetingDays
                            .Select(slot => slot.ToShortDateString())
                            .ToArray())
                        .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
                })
            .Output((_, data) => data.AppointmentDate = DateTime.Parse(data.PaginationMessage));
    }
}