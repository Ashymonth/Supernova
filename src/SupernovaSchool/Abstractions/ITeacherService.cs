using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface ITeacherService
{
    Task<Teacher?> GetTeacherAsync(Guid teacherId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(CancellationToken ct = default);
}