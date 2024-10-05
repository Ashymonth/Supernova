using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher;

public class CreateTeacherWorkflow : IWorkflow<CreateTeacherWorkflowData>
{
    public string Id => nameof(CreateTeacherWorkflow);

    public int Version => 1;

    public void Build(IWorkflowBuilder<CreateTeacherWorkflowData> builder)
    {
        builder.StartWith<EnsureThatUserIsAdminStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsAdmin, step => step.IsAdmin)
            .If(data => !data.IsAdmin).Do(workflowBuilder =>
                workflowBuilder.SendMessageToUser("У вас недостаточно прав для этой команды").EndWorkflow())
            .SendMessageToUser("Введите Фио")
            .WaitForUserMessage(data => data.TeacherName, message => message.Message)
            .SendMessageToUser("Введите логин от яндекс календаря")
            .WaitForUserMessage(data => data.YandexCalendarLogin, message => message.Message)
            .SendMessageToUser(
                "Введите пароль от яндекс календаря\n Как получить пароль: https://id.yandex.ru/security/app-passwords")
            .WaitForUserMessage(data => data.YandexCalendarPassword, message => message.Message)
            .SendMessageToUser("Обработка запроса")
            .Then<CreateTeacherStep>()
            .Input(step => step.Name, data => data.TeacherName)
            .Input(step => step.Login, data => data.YandexCalendarLogin)
            .Input(step => step.Password, data => data.YandexCalendarPassword)
            .SendMessageToUser("Учитель успешно добавлен")
            .OnError(WorkflowErrorHandling.Terminate)
            .EndWorkflow();
    }
}