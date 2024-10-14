using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Steps;
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

    public string Id => Commands.RegisterAsStudentCommand;

    public int Version => 1;

    public void Build(IWorkflowBuilder<RegisterStudentWorkflowData> builder)
    {
        builder.SendMessageToUser(
                DefaultStepMessage.CreateInitialMessage(RegisterStudentStepMessage.CommandStartMessage), true)
            .SendMessageToUser(RegisterStudentStepMessage.InputName)
            .WaitForUserMessage(data => data.StudentName, message => message.Message)
            .SendMessageWithPagination(data => !AvailableClasses.Contains(data.PaginationMessage), workflowBuilder =>
            {
                workflowBuilder
                    .SendVariantsPage(RegisterStudentStepMessage.InputClass, data => AvailableClasses)
                    .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
            })
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .RegisterStudent()
            .CleanupAndEndWorkflow(data =>
                RegisterStudentStepMessage.CreateSuccessMessage(data.StudentName, data.PaginationMessage));
    }
}