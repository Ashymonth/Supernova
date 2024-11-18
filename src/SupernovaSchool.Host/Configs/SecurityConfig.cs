using Microsoft.Extensions.Options;

namespace SupernovaSchool.Host.Configs;

public class SecurityConfig
{
    public string SecretKey { get; set; } = null!;

    public string InitVector { get; set; } = null!;
}

public class SecurityConfigSetup : IConfigureOptions<SecurityConfig>
{
    private readonly IConfiguration _configuration;

    public SecurityConfigSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SecurityConfig options)
    {
        _configuration.GetSection(nameof(SecurityConfig)).Bind(options);
    }
}