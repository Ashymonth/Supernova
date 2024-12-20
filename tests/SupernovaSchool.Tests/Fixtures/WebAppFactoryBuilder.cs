using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Data;
using SupernovaSchool.Models;
using SupernovaSchool.Tests.Extensions;

namespace SupernovaSchool.Tests.Fixtures;

public class WebAppFactoryBuilder : WebAppFactory
{
    private Student? _student;
    private Func<IServiceProvider, List<Teacher>>? _teachers;
    private readonly Dictionary<Type, object> _replacedServices = [];
    private readonly List<Func<IConfigurationBuilder, IConfigurationBuilder>> _additionalConfigurations = [];

    public WebAppFactoryBuilder WithStudent(Student student)
    {
        _student = student;
        return this;
    }

    public WebAppFactoryBuilder WithAdditionalConfiguration(Func<IConfigurationBuilder, IConfigurationBuilder> func)
    {
        _additionalConfigurations.Add(func);
        return this;
    }

    public WebAppFactoryBuilder WithTeachers(Func<IServiceProvider, List<Teacher>> teachersFactory)
    {
        _teachers = teachersFactory;
        return this;
    }

    public WebAppFactoryBuilder WithReplacedService<TService>(TService service)
    {
        _replacedServices.Add(typeof(TService), service!);
        return this;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        foreach (var (serviceType, newService) in _replacedServices)
        {
            services.ReplaceRequiredService(serviceType, newService);
        }

        base.ConfigureServices(services);
    }

    protected override void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        foreach (var additionalConfiguration in _additionalConfigurations)
        {
            additionalConfiguration(configurationBuilder);
        }
    }

    protected override void SeedData(IServiceProvider provider, SupernovaSchoolDbContext dbContext)
    {
        if (_student is not null)
        {
            dbContext.Students.Add(_student);
        }

        if (_teachers is not null)
        {
            dbContext.Teachers.AddRange(_teachers(provider));
        }

        base.SeedData(provider, dbContext);
    }
}