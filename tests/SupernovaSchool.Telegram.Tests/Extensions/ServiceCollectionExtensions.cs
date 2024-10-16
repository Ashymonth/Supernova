using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SupernovaSchool.Telegram.Tests.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void ReplaceRequiredService<TServiceToReplace>(this IServiceCollection services,
        TServiceToReplace newService) where TServiceToReplace : class
    {
        var descriptor = services.First(d => d.ServiceType == typeof(TServiceToReplace));

        services.Remove(descriptor);

        services.AddSingleton(newService);
    }
}