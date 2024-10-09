namespace SupernovaSchool.AppHost;

public class HttpResource (string name) : ContainerResource(name), IResourceWithConnectionString
{
    private const string HttpEndpointName = "http";
    private EndpointReference? _httpReference;

    private EndpointReference HttpEndpoint => _httpReference ??= new EndpointReference(this, HttpEndpointName);

    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"https://{HttpEndpoint.Property(EndpointProperty.Host)}:{HttpEndpoint.Property(EndpointProperty.Port)}");
}