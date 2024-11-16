using SupernovaSchool.Telegram.Workflows.CreateAppointment;
using SupernovaSchool.Telegram.Workflows.CreateTeacher;
using SupernovaSchool.Telegram.Workflows.DeleteAppointments;
using SupernovaSchool.Telegram.Workflows.RegisterStudent;
using WorkflowCore.Interface;

namespace SupernovaSchool.Host.Extensions;

internal static class WorkflowHostExtensions
{
    public static void UserWorkflowsAndStartHost(this WebApplication webApplication)
    {
        var workflowHost = webApplication.Services.GetRequiredService<IWorkflowHost>();
        
        workflowHost.RegisterWorkflow<CreateAppointmentWorkflow, CreateAppointmentWorkflowData>();
        workflowHost.RegisterWorkflow<RegisterStudentWorkflow, RegisterStudentWorkflowData>();
        workflowHost.RegisterWorkflow<DeleteMyAppointmentsWorkflow, DeleteMyAppointmentsWorkflowData>();
        workflowHost.RegisterWorkflow<CreateTeacherWorkflow, CreateTeacherWorkflowData>();
        workflowHost.Start();
    }
}