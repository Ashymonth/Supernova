using Microsoft.Extensions.Caching.Memory;
using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;

namespace SupernovaSchool.Application.Services;

public class StudentService : IStudentService
{
    private const string CacheKeyTemplate = "student-service_{0}";
    private readonly IMemoryCache _memoryCache;

    private readonly IRepository<Student> _studentRepository;

    public StudentService(IRepository<Student> studentRepository, IMemoryCache memoryCache)
    {
        _studentRepository = studentRepository;
        _memoryCache = memoryCache;
    }

    public async Task AddOrUpdateAsync(Student student, CancellationToken ct = default)
    {
        if (!_memoryCache.TryGetValue(CreateCacheKey(student.Id), out Student? existedStudent))
        {
            await _studentRepository.AddAsync(student, ct);
        }
        else
        {
            existedStudent!.Name = student.Name;
            existedStudent.Class = student.Class;
            _studentRepository.Update(existedStudent);
        }

        await _studentRepository.UnitOfWork.SaveChangesAsync(ct);
        _memoryCache.Set(string.Format(CacheKeyTemplate, student.Id), existedStudent);
    }

    public async Task<Student?> GetStudentAsync(string studentId)
    {
        return await _memoryCache.GetOrCreateAsync(CreateCacheKey(studentId), async entry =>
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            entry.SetValue(student);
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            return student;
        });
    }

    private static string CreateCacheKey(string studentId)
    {
        return string.Format(CacheKeyTemplate, studentId);
    }
}