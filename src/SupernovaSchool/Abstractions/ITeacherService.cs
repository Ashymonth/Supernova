using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface ITeacherService
{
    Task<Teacher> CreateAsync(string name, string login, string password, CancellationToken ct = default);
    
    Task<Teacher?> GetTeacherAsync(Guid teacherId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Teacher>> GetTeachersAsync(CancellationToken ct = default);
    
    Task DeleteAsync(Guid teacherId, CancellationToken ct = default);
}