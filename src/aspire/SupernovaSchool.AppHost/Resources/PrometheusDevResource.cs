namespace SupernovaSchool.AppHost;

public class PrometheusDevResource(string name) : ContainerResource(name) , IResourceWithConnectionString
{
    internal const string HttpEndpointName = "http";
    private EndpointReference? _smtpReference;

    public EndpointReference HttpEndpoint =>
        _smtpReference ??= new(this, HttpEndpointName);
    
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"https://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}");

}