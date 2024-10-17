using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SupernovaSchool.Telegram.Tests.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void ReplaceRequiredService<TServiceToReplace>(this IServiceCollection services,
        TServiceToReplace newService) where TServiceToReplace : class
    {
        var descriptor = services.First(d => d.ServiceType == newService.GetType());

        services.Remove(descriptor);

        services.AddSingleton(newService);
    }
    
    public static void ReplaceRequiredService(this IServiceCollection services,
        Type serviceToReplace, object newService)
    {
        var descriptor = services.First(d => d.ServiceType == serviceToReplace);

        services.Remove(descriptor);

        services.AddSingleton(serviceToReplace, newService);
    }
}