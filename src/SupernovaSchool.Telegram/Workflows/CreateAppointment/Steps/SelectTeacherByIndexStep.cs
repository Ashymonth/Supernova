using SupernovaSchool.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;

public class SelectTeacherByIndexStep : IStepBody
{
    public int TeacherIndex { get; set; } 

    public List<Teacher> Teachers { get; set; } = null!;
    
    public Teacher SelectedTeacher { get; set; } = null!;
    
    public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        SelectedTeacher = Teachers[TeacherIndex];

        return Task.FromResult(ExecutionResult.Next());
    }

}