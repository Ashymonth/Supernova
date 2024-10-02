using SupernovaSchool.Abstractions;
using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using SupernovaSchool.Specifications;

namespace SupernovaSchool.Application.Services;

public class StudentService : IStudentService
{
    private readonly IRepository<Student> _studentRepository;

    public StudentService(IRepository<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> IsStudentExistAsync(string studentId, CancellationToken ct = default)
    {
        return await _studentRepository.AnyAsync(new StudentByIdSpecification(studentId), ct);
    }
    
    
}