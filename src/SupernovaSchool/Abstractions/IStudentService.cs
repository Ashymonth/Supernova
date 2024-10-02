namespace SupernovaSchool.Abstractions;

public interface IStudentService
{
    Task<bool> IsStudentExistAsync(string studentId, CancellationToken ct = default);
}