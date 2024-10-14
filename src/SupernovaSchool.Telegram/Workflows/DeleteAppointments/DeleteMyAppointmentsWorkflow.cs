using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments.Steps;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.DeleteAppointments;

public class DeleteMyAppointmentsWorkflow : IWorkflow<DeleteMyAppointmentsWorkflowData>
{
    public string Id => Commands.DeleteAppointmentCommand;

    public int Version => 1;

    public void Build(IWorkflowBuilder<DeleteMyAppointmentsWorkflowData> builder)
    {
        builder
            .StartWith<LoadMyAppointmentsStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.StudentAppointmentInfo, step => step.StudentAppointments)
            .If(data => data.StudentAppointmentInfo.Count == 0)
            .Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(DeleteMyAppointmentsStepMessage.DontHaveAnyAppointments))
            .SendMessageToUser(DeleteMyAppointmentsStepMessage.SelectAppointmentToDelete)
            .Then<SendAppointmentToDelete>()
            .Input(delete => delete.UserId, data => data.UserId)
            .Input(delete => delete.StudentAppointments, data => data.StudentAppointmentInfo)
            .WaitForUserInlineData(data => data.AppointmentDateToDelete,
                o => DateTime.Parse((o as UserMessage)!.Message!))
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<DeleteAppointmentStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.AppointmentDay, data => data.AppointmentDateToDelete)
            .CleanupAndEndWorkflow(data => DeleteMyAppointmentsStepMessage.CreateSuccessMessage(data.AppointmentDateToDelete));
    }
}