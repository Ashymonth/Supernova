using SupernovaSchool.Telegram.Steps;

namespace SupernovaSchool.Telegram.Workflows.RegisterStudent;

public class RegisterStudentWorkflowData :  MessagePaginator, IUserStep
{
    public string UserId { get; set; } = null!;

    public string StudentName { get; set; } = null!;
}