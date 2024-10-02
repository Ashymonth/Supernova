using SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.RegisterStudent.Extensions;

internal static class RegisterStudentStepExtension
{
    public static IStepBuilder<TData, RegisterStudentStep> RegisterStudent<TData, TStep>(
        this IStepBuilder<TData, TStep> builder) where TData : RegisterStudentWorkflowData where TStep : IStepBody
    {
        return builder.Then<RegisterStudentStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.StudentName, data => data.StudentName)
            .Input(step => step.StudentClass, data => data.PaginationMessage);
    }
}