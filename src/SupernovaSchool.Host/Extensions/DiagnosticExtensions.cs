using OpenTelemetry.Metrics;
using Serilog;
using SupernovaSchool.Telegram.Metrics;

namespace SupernovaSchool.Host.Extensions;

internal static class DiagnosticExtensions
{
    public static void AddSerilogAndOpenTelemetry(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, configuration) => configuration.ReadFrom.Configuration(builder.Configuration));
        
        builder.Services.AddOpenTelemetry()
            .WithTracing()
            .WithMetrics(providerBuilder => providerBuilder
                .AddMeter(WorkflowStaterCounterMetric.MeterName)
                .AddMeter(StepDurationTimeMeter.MeterName)
                .AddPrometheusExporter()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation());
        
        
        builder.Services.AddSingleton<WorkflowStaterCounterMetric>();
        builder.Services.AddSingleton<StepDurationTimeMeter>();
    }
}