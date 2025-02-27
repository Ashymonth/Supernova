using SupernovaSchool.Models;
using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.DeleteTeacher;

public class DeleteTeacherWorkflowData : IUserStep
{
    public string UserId { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public List<Teacher> Teachers { get; set; } = [];

    public string? SelectedTeacherIndex { get; set; }

    public Teacher? TeacherToDelete =>
        int.TryParse(SelectedTeacherIndex, out var index) ? Teachers.ElementAtOrDefault(index) : null;
 
    public string CreateSelectTeacherIndexMessage()
    {
        return string.Join("\n", Teachers.Select((teacher, index) => $"{index}. {teacher.Name}"));
    }
}