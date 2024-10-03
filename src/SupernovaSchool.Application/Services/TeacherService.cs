using Microsoft.Extensions.Caching.Memory;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;

namespace SupernovaSchool.Application.Services;

public class TeacherService : ITeacherService
{
    private const string TeachersCacheKey = "teacher-service";
    private readonly IMemoryCache _memoryCache;

    private readonly IRepository<Teacher> _teacherRepository;

    public TeacherService(IRepository<Teacher> teacherRepository, IMemoryCache memoryCache)
    {
        _teacherRepository = teacherRepository;

        _memoryCache = memoryCache;
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