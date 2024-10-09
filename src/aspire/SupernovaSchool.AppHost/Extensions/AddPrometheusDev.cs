namespace SupernovaSchool.AppHost.Extensions;

public static class AddPrometheusDev
{
    public static void AddPrometheus(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new HttpResource(name);

        builder.AddResource(resource)
            .WithImage("prom/prometheus")
            .WithHttpEndpoint(
                targetPort: 9090,
                port: 9090,
                name: "prometheus")
            .WithBindMount("C:/prometheus.yml", "/etc/prometheus/prometheus.yml");
    }
}