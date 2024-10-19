using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;

namespace SupernovaSchool.Telegram.Extensions.Steps;

public static class BaseStepExtensions
{
    public static void AddBaseSteps(this IServiceCollection services)
    {
        services.AddTransient<SendMessageToUser>();
        services.AddTransient<SendMessageWithOptionsToUser>();
        services.AddTransient<SendInitialMessageToUserStep>();
        services.AddTransient<SendInlineDataMessageToUser>();
        services.AddTransient<CleanupStep>();
    }
}