using Microsoft.Extensions.Caching.Memory;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Abstractions.Security;
using SupernovaSchool.Models;
using YandexCalendar.Net;
using YandexCalendar.Net.Models;

namespace SupernovaSchool.Application.Services;

public class TeacherService : ITeacherService
{
    private const string TeachersCacheKey = "teacher-service";
    private readonly IMemoryCache _memoryCache;
    private readonly IPasswordProtector _passwordProtector;
    private readonly IRepository<Teacher> _teacherRepository;
    private readonly IAuthorizationResource _iAuthorizationResource;

    public TeacherService(IRepository<Teacher> teacherRepository, IMemoryCache memoryCache,
        IPasswordProtector passwordProtector, IAuthorizationResource iAuthorizationResource)
    {
        _teacherRepository = teacherRepository;

        _memoryCache = memoryCache;
        _passwordProtector = passwordProtector;
        _iAuthorizationResource = iAuthorizationResource;
    }

    public async Task<Teacher> CreateAsync(string name, string login, string password, CancellationToken ct = default)
    {
        if (!await _iAuthorizationResource.AuthorizeAsync(new UserCredentials(login, password), ct))
        {
            throw new InvalidOperationException("Invalid login or password.");
        }
        
        var teacher = Teacher.Create(name, login, Password.Create(password, _passwordProtector));

        await _teacherRepository.AddAsync(teacher, ct);

        await _teacherRepository.UnitOfWork.SaveChangesAsync(ct);

        return teacher;
    }

    public async Task<Teacher?> GetTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        var teachers = await GetTeachersAsync(ct);
        return teachers.FirstOrDefault(teacher1 => teacher1.Id == teacherId);
    }

    public async Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(CancellationToken ct = default)
    {
        return (await _memoryCache.GetOrCreateAsync(TeachersCacheKey, async entry =>
        {
            var result = await _teacherRepository.ListAsync(ct);

            entry.SetValue(result);
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));

            return result;
        }))!;
    }
}