﻿using System.Linq.Expressions;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using WorkflowCore.Interface;
using WorkflowCore.Primitives;

namespace SupernovaSchool.Telegram.Extensions;

public static class StepBuilderMessageExtensions
{
    public static IStepBuilder<TData, SendMessageToUser> SendMessageToUser<TData>(this IWorkflowBuilder<TData> builder,
        string message, bool b)
        where TData : IUserStep
    {
        return builder
            .StartWith<SendMessageToUser>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, _ => message)
            .Input(step => step.ShouldBeDeleted, _ => b);
    }

    public static IStepBuilder<TData, SendMessageToUser> SendMessageToUser<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, string message, bool shouldBeDeleted = true)
        where TData : IUserStep where TStep : IStepBody
    {
        return builder
            .Then<SendMessageToUser>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, _ => message)
            .Input(step => step.ShouldBeDeleted, step => shouldBeDeleted);
    }

    public static IStepBuilder<TData, SendMessageToUser> SendMessageToUser<TData>(
        this IWorkflowBuilder<TData> builder, Func<TData, string> messageFunc)
        where TData : IUserStep
    {
        return builder
            .StartWith<SendMessageToUser>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, data => messageFunc(data));
    }

    public static IStepBuilder<TData, SendMessageToUser> SendMessageToUser<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> messageFunc, bool deleteMessage = true)
        where TStep : IStepBody
        where TData : IUserStep
    {
        return builder
            .Then<SendMessageToUser>()
            .Input(step => step.UserId, data => data.UserId)
            .Input(step => step.Message, data => messageFunc(data))
            .Input(step => step.ShouldBeDeleted, step => deleteMessage);
    }

    public static IStepBuilder<TData, WaitFor> WaitForUserMessage<TData, TStep, TOutput>(
        this IStepBuilder<TData, TStep> builder,
        Expression<Func<TData, TOutput>> dataProperty, Func<UserMessage, TOutput> dataConverter)
        where TStep : IStepBody
        where TData : IUserStep
    {
        return builder
            .WaitFor("UserMessage", data => data.UserId, _ => DateTime.UtcNow)
            .Output(dataProperty, step => dataConverter((step.EventData as UserMessage)!));
    }
}