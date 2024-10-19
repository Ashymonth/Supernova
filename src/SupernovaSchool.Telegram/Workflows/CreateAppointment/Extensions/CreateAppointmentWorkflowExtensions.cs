using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Primitives;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Extensions;

internal static class CreateAppointmentWorkflowExtensions
{
    public static IStepBuilder<CreateAppointmentWorkflowData, EnsureThatStudentRegisteredStep>
        EnsureThatStudentRegistered(this IWorkflowBuilder<CreateAppointmentWorkflowData> builder)
    {
        return builder
            .Then<EnsureThatStudentRegisteredStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsStudentRegistered, step => step.IsStudentRegistered);
    }

    public static IStepBuilder<CreateAppointmentWorkflowData, LoadTeachersStep> LoadTeachers<TStep>(
        this IStepBuilder<CreateAppointmentWorkflowData, TStep> builder) where TStep : IStepBody
    {
        return builder.Then<LoadTeachersStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.Teachers, step => step.Teachers.OrderBy(teacher => teacher.Name).ToList());
    }

    public static IStepBuilder<CreateAppointmentWorkflowData, SelectTeacherByIndexStep> RequestToSelectTeacher<TStep>(
        this IStepBuilder<CreateAppointmentWorkflowData, TStep> builder) where TStep : IStepBody
    {
        return builder.SendMessageWithPagination(data =>! data.IsTest(), workflowBuilder =>
            {
                workflowBuilder
                    .SendVariantsPage(data => CreateAppointmentStepMessage.CreateChooseTeacherMessage(data.Teachers),
                    data => data.Teachers.Select((_, index) => index.ToString()).ToArray())
                    .WaitForUserMessage(data => data.SelectedTeacherIndex, message => message.Message);
            })
            .Then<SelectTeacherByIndexStep>()
            .Input(step => step.Teachers, data => data.Teachers)
            .Input(step => step.TeacherIndex, data => int.Parse(data.SelectedTeacherIndex!))
            .Output(data => data.SelectedTeacher, step => step.SelectedTeacher);
    }

    public static IStepBuilder<CreateAppointmentWorkflowData, While> RequestToSelectAppointmentDay<TStep>(
        this IStepBuilder<CreateAppointmentWorkflowData, TStep> builder) where TStep : IStepBody
    {
        return builder.Then<LoadAvailableMeetingDays>()
            .Output(data => data.AvailableMeetingDays, days => days.AvailableMeetingDays)
            .SendMessageWithPagination(data => !data.IsMeetingDateValid(),
                workflowBuilder =>
                {
                    workflowBuilder
                        .SendVariantsPage(CreateAppointmentStepMessage.SelectAppointmentDay, data => data.AvailableMeetingDays
                            .Select(slot => slot.ToShortDateString())
                            .ToArray())
                        .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
                })
            .Output((_, data) => data.AppointmentDate = DateTime.Parse(data.PaginationMessage));
    }
}