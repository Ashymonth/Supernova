using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
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
        builder
            .UseDefaultErrorBehavior(WorkflowErrorHandling.Terminate)
            .StartWith<EnsureThatUserIsAdminStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsAdmin, step => step.IsAdmin)
            .If(data => !data.IsAdmin).Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateTeacherStepMessage.NotEnoughRightToCreateATeacher))
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
            .Output(data => data.IsTeacherCreated, step => step.IsTeacherCreated)
            .If(data => !data.IsTeacherCreated).Do(workflowBuilder =>
                workflowBuilder
                    .CleanupAndEndWorkflow("Не удалось создать учителя. Проверьте корректности логина и пароля."))
            .CleanupAndEndWorkflow(data =>
                CreateTeacherStepMessage.CreateSuccessMessage(data.TeacherName, data.YandexCalendarLogin));
    }
}