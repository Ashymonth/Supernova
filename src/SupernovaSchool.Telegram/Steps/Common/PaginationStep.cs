using WorkflowCore.Interface;
using WorkflowCore.Primitives;

namespace SupernovaSchool.Telegram.Steps.Common;

internal static class PaginationStep
{
    public static IStepBuilder<TData, While> SendMessageWithPagination<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, Func<TData, bool> paginationEndCondition,
        Action<IWorkflowBuilder<TData>> onPagination)
        where TStep : IStepBody
    {
        return builder.While(data => paginationEndCondition(data)).Do(onPagination);
    }
}