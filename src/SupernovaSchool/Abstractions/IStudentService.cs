using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface IStudentService
{
    Task AddOrUpdateAsync(Student student, CancellationToken ct = default);

    Task<Student?> GetStudentAsync(string studentId);
}