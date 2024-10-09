namespace SupernovaSchool.AppHost.Extensions;

public static class AddGrafanaDev
{
    public static void AddGrafana(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource = new HttpResource(name);

        builder.AddResource(resource)
            .WithImage("grafana/grafana-enterprise")
            .WithHttpEndpoint(
                targetPort: 3000,
                port: 3000,
                name: "grafana")
            .WithBindMount("grafana-storage", "/var/lib/grafana");
    }
}