namespace SupernovaSchool.AppHost.Extensions;

public static class AddPrometheusDev
{
    public static IResourceBuilder<GrafanaDevResource> AddPrometheus(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new GrafanaDevResource(name);

        return builder.AddResource(resource)
            .WithImage("prom/prometheus")
            .WithHttpEndpoint(
                targetPort: 9090,
                port: 9090,
                name: "prometheus")
            .WithBindMount("C:/prometheus.yml", "/etc/prometheus/prometheus.yml");
    }
}