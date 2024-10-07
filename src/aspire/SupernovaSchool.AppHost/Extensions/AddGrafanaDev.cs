namespace SupernovaSchool.AppHost.Extensions;

public static class AddGrafanaDev
{
    public static IResourceBuilder<GrafanaDevResource> AddGrafana(
        this IDistributedApplicationBuilder builder,
        string name)
    {
        var resource  = new GrafanaDevResource(name);

        return builder.AddResource(resource)
            .WithImage("grafana/grafana-enterprise")
            .WithHttpEndpoint(
                targetPort: 3000,
                port: 3000,
                name: "grafana");
    }
}