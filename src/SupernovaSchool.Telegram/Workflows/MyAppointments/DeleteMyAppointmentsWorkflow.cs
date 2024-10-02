using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Workflows.MyAppointments.Steps;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.MyAppointments;

public class DeleteMyAppointmentsWorkflow : IWorkflow<DeleteMyAppointmentsWorkflowData>
{
    public string Id => nameof(DeleteMyAppointmentsWorkflow);

    public int Version => 1;

    public void Build(IWorkflowBuilder<DeleteMyAppointmentsWorkflowData> builder)
    {
        builder
            .StartWith<LoadMyAppointmentsStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.StudentAppointmentInfo, step => step.StudentAppointments)
            .If(data => data.StudentAppointmentInfo.Count == 0)
            .Do(workflowBuilder =>
                workflowBuilder.SendMessageToUser("У вас нет ни одной активной записи").EndWorkflow())
            .SendInlineData("Выберите запись для удаления", data => data.CreateButtonsToDeleteAppointment())
            .WaitForUserInlineData(data => data.AppointmentDateToDelete, o => DateTime.Parse((o as UserMessage)!.Message!))
            .SendMessageToUser("Обработка запроса...")
            .Then<DeleteAppointmentStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.AppointmentDay, data => data.AppointmentDateToDelete)
            .SendMessageToUser("Запись успешно удалена")
            .EndWorkflow();
    }
}