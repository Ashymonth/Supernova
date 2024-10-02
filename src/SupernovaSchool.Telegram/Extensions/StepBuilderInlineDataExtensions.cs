using System.Linq.Expressions;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using Telegram.Bot.Types.ReplyMarkups;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Extensions;

public static class StepBuilderInlineDataExtensions
{
    public static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariants<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, string message, string[] variants)
        where TStep : IStepBody
        where TData : IUserStep
    {
        var step = builder.Then<SendMessageWithOptionsToUser>();
        return SendVariantsInternal(step, message, _ => variants);
    }

    public static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariants<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, string message, Expression<Func<TData, string[]>> getVariants)
        where TStep : IStepBody
        where TData : IUserStep
    {
        var step = builder.Then<SendMessageWithOptionsToUser>();
        return SendVariantsInternal(step, message, getVariants);
    }
    
    public static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariants<TData>(
        this IWorkflowBuilder<TData> builder, string message, Expression<Func<TData, string[]>> getVariants)
        where TData : IUserStep
    {
        var step = builder.Then<SendMessageWithOptionsToUser>();
        return SendVariantsInternal(step, message, getVariants);
    }
    
    public static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariantsPage<TData>(
        this IWorkflowBuilder<TData> builder, string message, Expression<Func<TData, string[]>> getVariants)
        where TData : MessagePaginator, IUserStep
    {
        var step = builder.Then<SendMessageWithOptionsToUser>();
        step.Input(user => user.Page, data => data.GetPage());
        return SendVariantsInternal(step, message, getVariants);
    }

    public static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariantsPage<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, string message,
        Expression<Func<TData, string[]>> getVariants)
        where TData : MessagePaginator, IUserStep
        where TStep : IStepBody
    {
        var step = builder.Then<SendMessageWithOptionsToUser>();
        step.Input(user => user.Page, data => data.GetPage());
        return SendVariantsInternal(step, message, getVariants);
    }

    public static IStepBuilder<TData, SendInlineDataMessageToUser> SendInlineData<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, string message,
        Expression<Func<TData, InlineKeyboardButton[]>> getVariants)
        where TStep : IStepBody
        where TData : IUserStep
    {
        return builder
            .Then<SendInlineDataMessageToUser>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, _ => message)
            .Input(step => step.Options, getVariants);
    }

    private static IStepBuilder<TData, SendMessageWithOptionsToUser> SendVariantsInternal<TData>(
        this IStepBuilder<TData, SendMessageWithOptionsToUser> builder, string message,
        Expression<Func<TData, string[]>> getVariants)
        where TData : IUserStep
    {
        return builder
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, _ => message)
            .Input(step => step.Options, getVariants);
    }
}