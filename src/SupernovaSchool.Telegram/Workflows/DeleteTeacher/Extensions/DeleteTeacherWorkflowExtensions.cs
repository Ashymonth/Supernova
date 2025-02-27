using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using SupernovaSchool.Telegram.Workflows.DeleteTeacher.Steps;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.DeleteTeacher.Extensions;

internal static class DeleteTeacherWorkflowExtensions
{
    public static IStepBuilder<TData, LoadTeachersStep> LoadTeachers<TData, TStep>(
        this IStepBuilder<TData, TStep> builder) where TData : DeleteTeacherWorkflowData where TStep : IStepBody
    {
        return builder.Then<LoadTeachersStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(step => step.Teachers, step => step.Teachers);
    }

    public static IStepBuilder<TData, DeleteTeacherStep> DeleteSelectedTeacher<TData, TStep>(
        this IStepBuilder<TData, TStep> builder) where TData : DeleteTeacherWorkflowData where TStep : IStepBody
    {
        return builder.Then<DeleteTeacherStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.TeacherId, step => step.TeacherToDelete!.Id);
    }
}