using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SupernovaSchool.Host.Extensions;

internal static class HealthExtensions
{
    public static void AddConfiguredHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => new HealthCheckResult(HealthStatus.Healthy))
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!);

        // Configure the Health Checks UI Client
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(10);
            options.MaximumHistoryEntriesPerEndpoint(50);
        }).AddInMemoryStorage();
    }

    public static void UseConfiguredHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecksUI(options => options.ApiPath = "/health-ui");
        app.UseHealthChecksPrometheusExporter(new PathString("/health-prometheus"));
    }
}