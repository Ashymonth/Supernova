using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.RegisterStudent.Extensions;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.RegisterStudent;

public class RegisterStudentWorkflow : IWorkflow<RegisterStudentWorkflowData>
{
    private static readonly string[] AvailableClasses =
    [
        ..Enumerable.Range(5, 7).Select(i => i.ToString())
    ];

    public string Id => nameof(RegisterStudentWorkflow);

    public int Version => 1;

    public void Build(IWorkflowBuilder<RegisterStudentWorkflowData> builder)
    {
        builder.SendInitialMessageToUser("Для того, чтобы записаться к психологу, вам нужно указать свои данные.")
            .SendMessageToUser("Введите имя")
            .WaitForUserMessage(data => data.StudentName, message => message.Message)
            .SendMessageWithPagination(data => !AvailableClasses.Contains(data.PaginationMessage), workflowBuilder =>
            {
                workflowBuilder
                    .SendVariantsPage("Укажите ваш поток", data => AvailableClasses)
                    .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
            })
            .SendMessageToUser("Обработка запроса...")
            .RegisterStudent()
            .SendMessageToUser("Вы успешно зарегестрировались. Теперь вы можете записаться к психологу")
            .EndWorkflow();
    }
}