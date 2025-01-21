using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupernovaSchool.Data;
using SupernovaSchool.Models;
using SupernovaSchool.Tests.Extensions;
using Testcontainers.PostgreSql;

namespace SupernovaSchool.Tests.Fixtures;

public class WebAppFactoryBuilder : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("supernova_school.test")
        .WithUsername("testUser")
        .WithPassword("testPassword")
        .WithExposedPort(5432)
        .Build();

    private Student? _student;
    private Func<IServiceProvider, List<Teacher>>? _teachers;
    private readonly ConcurrentDictionary<Type, object> _replacedServices = [];
    private readonly List<Func<IConfigurationBuilder, IConfigurationBuilder>> _additionalConfigurations = [];
    private WebAppFactory _factory = null!;

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
        _replacedServices.AddOrUpdate(typeof(TService), x => service!, (x, y) => service!);
        return this;
    }

    public WebAppFactory Build()
    {
        var configureServicesAction = _replacedServices.Count != 0
            ? _replacedServices.Select(func => new Action<IServiceCollection>(
                collection => collection.ReplaceRequiredService(func.Key, func.Value))).ToArray()
            : null;

        var configureAppConfigurationAction = _additionalConfigurations.Count != 0
            ? _additionalConfigurations.Select(func => new Action<IConfigurationBuilder>(builder => func(builder)))
                .ToArray()
            : null;

        var seedDataActions = new List<Action<IServiceProvider, SupernovaSchoolDbContext>>();

        if (_student is not null)
        {
            seedDataActions.Add((_, context) => context.Students.Add(_student));
        }

        if (_teachers is not null)
        {
            seedDataActions.Add((provider, context) => context.Teachers.AddRange(_teachers(provider)));
        }

        _factory = new WebAppFactory(configureServicesAction, configureAppConfigurationAction,
            seedDataActions.Count != 0 ? seedDataActions.ToArray() : null, _container);

        return _factory;
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}