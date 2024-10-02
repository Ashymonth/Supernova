using SupernovaSchool.Telegram.Steps;
using Telegram.Bot.Types.Enums;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Extensions;

internal static class WorkflowHostExtensions
{
    private static readonly Dictionary<UpdateType, string> EventMap = new()
    {
        [UpdateType.Message] = "UserMessage",
        [UpdateType.CallbackQuery] = "CallbackMessage",
    };

    public static async Task PublishUserMessageAsync(this IWorkflowHost workflowHost, UpdateType updateType, string
        userId, UserMessage message)
    {
        await workflowHost.PublishEvent(EventMap[updateType], userId, message);
    }
}