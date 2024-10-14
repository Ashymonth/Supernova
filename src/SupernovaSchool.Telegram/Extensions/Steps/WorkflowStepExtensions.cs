using SupernovaSchool.Telegram.Steps;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Extensions.Steps;

internal static class WorkflowStepExtensions
{
    public static void CleanupAndEndWorkflow<TStep, TData>(this IStepBuilder<TStep, TData> builder, string message)
        where TData : IStepBody
        where TStep : IUserStep
    {
        builder.Then<CleanupStep>()
            .Input(step => step.UserId, step => step.UserId)
            .SendMessageToUser(message, false)
            .EndWorkflow();
    }
    
    public static void CleanupAndEndWorkflow<TStep, TData>(this IStepBuilder<TStep, TData> builder, Func<TStep, string> messageFactory)
        where TData : IStepBody
        where TStep : IUserStep
    {
        builder.Then<CleanupStep>()
            .Input(step => step.UserId, step => step.UserId)
            .SendMessageToUser(messageFactory, false)
            .EndWorkflow();
    }

    public static void CleanupAndEndWorkflow<TData>(this IWorkflowBuilder<TData> builder, string message)
        where TData : IUserStep
    {
        builder.Then<CleanupStep>()
            .Input(step => step.UserId, step => step.UserId)
            .SendMessageToUser(message, false)
            .EndWorkflow();
    }
}