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

    public async Task<Student> AddOrUpdateAsync(Student student, CancellationToken ct = default)
    {
        var existedStudent = await _studentRepository.GetByIdAsync(student.Id, ct);

        if (existedStudent is null)
        {
            existedStudent = student;
            await _studentRepository.AddAsync(student, ct);
        }
        else
        {
            existedStudent.Name = student.Name;
            existedStudent.Class = student.Class;
            _studentRepository.Update(existedStudent);
        }

        await _studentRepository.UnitOfWork.SaveChangesAsync(ct);
        _memoryCache.Set(string.Format(CacheKeyTemplate, student.Id), existedStudent);

        return student;
    }

    public async Task<Student?> GetStudentAsync(string studentId, CancellationToken ct = default)
    {
        return await _memoryCache.GetOrCreateAsync(CreateCacheKey(studentId), async entry =>
        {
            var student = await _studentRepository.GetByIdAsync(studentId, ct);

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