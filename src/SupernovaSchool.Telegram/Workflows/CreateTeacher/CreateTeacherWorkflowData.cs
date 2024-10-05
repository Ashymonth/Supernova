using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.CreateTeacher;

public class CreateTeacherWorkflowData : IUserStep
{
    public string UserId { get; set; } = null!;
    
    public bool IsAdmin { get; set; }

    public string TeacherName { get; set; } = null!;

    public string YandexCalendarLogin { get; set; } = null!;
    
    public string YandexCalendarPassword { get; set; } = null!;
}