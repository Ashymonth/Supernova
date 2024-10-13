using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher;

public class CreateTeacherWorkflow : IWorkflow<CreateTeacherWorkflowData>
{
    public string Id => Commands.CreateTeacherCommand;

    public int Version => 1;

    public void Build(IWorkflowBuilder<CreateTeacherWorkflowData> builder)
    {
        builder.StartWith<EnsureThatUserIsAdminStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsAdmin, step => step.IsAdmin)
            .If(data => !data.IsAdmin).Do(workflowBuilder =>
                workflowBuilder
                    .Then<CleanupStep>()
                    .Input(step => step.UserId, data => data.UserId)
                    .SendMessageToUser(CreateTeacherStepMessage.NotEnoughRightToCreateATeacher, false)
                    .EndWorkflow())
            .SendMessageToUser(CreateTeacherStepMessage.InputName)
            .WaitForUserMessage(data => data.TeacherName, message => message.Message)
            .SendMessageToUser(CreateTeacherStepMessage.InputLoginFromYandexCalendar)
            .WaitForUserMessage(data => data.YandexCalendarLogin, message => message.Message)
            .SendMessageToUser(CreateTeacherStepMessage.InputPasswordFromYandexCalendar)
            .WaitForUserMessage(data => data.YandexCalendarPassword, message => message.Message)
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<CreateTeacherStep>()
            .Input(step => step.Name, data => data.TeacherName)
            .Input(step => step.Login, data => data.YandexCalendarLogin)
            .Input(step => step.Password, data => data.YandexCalendarPassword)
            .Then<CleanupStep>()
            .Input(step => step.UserId, data => data.UserId)
            .SendMessageToUser(
                data => CreateTeacherStepMessage.CreateSuccessMessage(data.TeacherName, data.YandexCalendarLogin),
                false)
            .OnError(WorkflowErrorHandling.Terminate)
            .EndWorkflow();
    }
}