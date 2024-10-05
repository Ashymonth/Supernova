using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;

namespace SupernovaSchool.Telegram;

public class CommandRegistry
{
    private readonly Dictionary<string, Func<string, IUserStep>> _commandNames = new()
    {
        [Commands.CreateTeacherCommand] = userId => new CreateTeacherWorkflowData { UserId = userId },
        [Commands.CreateAppointmentCommand] = userId => new CreateAppointmentWorkflowData { UserId = userId },
        [Commands.DeleteAppointmentCommand] = userId => new DeleteMyAppointmentsWorkflowData { UserId = userId },
        [Commands.RegisterAsStudentCommand] = userId => new RegisterStudentWorkflowData { UserId = userId },
    };

    public bool TryGetWorkflowByCommandName(string workflowName, out Func<string, IUserStep>? workflowDataFactory)
    {
        if (_commandNames.TryGetValue(workflowName, out var commandName))
        {
            workflowDataFactory = commandName;
            return true;
        }

        workflowDataFactory = null;
        return false;
    }
}