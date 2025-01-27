using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.CreateTeacher.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteTeacher.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.DeleteTeacher;

public class DeleteTeacherWorkflow : IWorkflow<DeleteTeacherWorkflowData>
{
    public string Id => Commands.DeleteTeacherCommand;

    public int Version => 1;

    public void Build(IWorkflowBuilder<DeleteTeacherWorkflowData> builder)
    {
        builder
            .UseDefaultErrorBehavior(WorkflowErrorHandling.Terminate)
            .StartWith<EnsureThatUserIsAdminStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsAdmin, step => step.IsAdmin)
            .If(data => !data.IsAdmin).Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateTeacherStepMessage.NotEnoughRightToCreateATeacher))
            .LoadTeachers()
            .If(data => data.Teachers.Count == 0).Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow("В системе нет ни одого учителя"))
            .SendMessageWithPagination(data => data.TeacherToDelete is null,
                workflowBuilder => workflowBuilder.SendVariants(data => "Выберите номер учителя для удаления: \n" +
                                                                        $"{data.CreateSelectTeacherIndexMessage()}",
                    data => data.Teachers.Select((_, index) => index.ToString()).ToArray())
                    .WaitForUserMessage(data => data.SelectedTeacherIndex, message => message.Message))
            .DeleteSelectedTeacher()
            .CleanupAndEndWorkflow("Учитель удален");
    }
}