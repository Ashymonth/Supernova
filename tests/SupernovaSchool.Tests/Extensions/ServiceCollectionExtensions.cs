using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace SupernovaSchool.Tests.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void ReplaceRequiredService(this IServiceCollection services,
        Type serviceToReplace, object newService)
    {
        var descriptor = services.First(d => d.ServiceType == serviceToReplace);

        services.Remove(descriptor);

        services.AddSingleton(serviceToReplace, newService);
    }
}