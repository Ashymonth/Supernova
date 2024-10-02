using SupernovaSchool.Abstractions.Repositories;
using SupernovaSchool.Models;
using SupernovaSchool.Specifications;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.RegisterStudent.Steps;

public class RegisterStudentStep : IStepBody
{
    private readonly IRepository<Student> _studentRepository;

    public RegisterStudentStep(IRepository<Student> studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public string UserId { get; set; } = null!;

    public string StudentName { get; set; } = null!;

    public string StudentClass { get; set; } = null!;

    public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(StudentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(StudentClass);
        
        var existedStudent = await _studentRepository.FirstOrDefaultAsync(new StudentByIdSpecification(UserId));

        if (existedStudent is null)
        {
            await _studentRepository.AddAsync(new Student { Id = UserId, Name = StudentName, Class = StudentClass },
                context.CancellationToken);
        }
        else
        {
            existedStudent.Name = StudentName;
            existedStudent.Class = StudentClass;
        }

        await _studentRepository.UnitOfWork.SaveChangesAsync(context.CancellationToken);

        return ExecutionResult.Next();
    }
}