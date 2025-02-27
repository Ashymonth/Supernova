using SupernovaSchool.Extensions;
using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Steps;
using SupernovaSchool.Telegram.Steps.Common;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Extensions;
using SupernovaSchool.Telegram.Workflows.CreateAppointment.Steps;
using WorkflowCore.Interface;

namespace SupernovaSchool.Telegram.Workflows.CreateAppointment;

public class CreateAppointmentWorkflow : IWorkflow<CreateAppointmentWorkflowData>
{
    public string Id => Commands.CreateAppointmentCommand;

    public int Version => 1;

    public void Build(IWorkflowBuilder<CreateAppointmentWorkflowData> builder)
    {
        builder
            .EnsureThatStudentRegistered()
            .If(data => !data.IsStudentRegistered).Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage.UserNotRegistered))
            .SendMessageToUser(CreateAppointmentStepMessage.InitialMessage)
            .LoadTeachers()
            .RequestToSelectTeacher()
            .RequestToSelectAppointmentDay()
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<EnsureThatUserDosentRegisteredOnMeeting>()
            .Input(meeting => meeting.UserId, data => data.UserId)
            .Input(meeting => meeting.Date, data => data.PaginationMessage.ParseApplicationDateOnlyFormat())
            .Output(data => data.UserHasAppointment, user => user.HasAppointment)
            .If(data => data.UserHasAppointment).Do(
                workflowBuilder => workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage
                    .AlreadyHaveAppointmentOnSelectedDay)
            )
            .Then<LoadTeacherAvailableTimeSlotsStep>()
            .Input(slots => slots.SelectedTeacherIndex, data => int.Parse(data.SelectedTeacherIndex!))
            .Input(slots => slots.Teachers, data => data.Teachers)
            .Input(slots => slots.DueDate, data => data.AppointmentDate)
            .Output(data => data.AvailableTimeSlots, slots => slots.AvailableSlots)
            .If(data => data.AvailableTimeSlots.Length == 0)
            //
            .Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage.NoAvailableTimeSlots))
            .While(data => data.GetTimeSlot() == null || !data.AvailableTimeSlots.Contains(data.GetTimeSlot()))
            .Do(builder1 =>
            {
                builder1.SendVariantsPage(CreateAppointmentStepMessage.SelectTimeSlot,
                        data => data.AvailableTimeSlots
                            .Select(CreateAppointmentStepMessage.CreateTimeSlotMessage)
                            .ToArray())
                    .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
            })
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<CreateMeetingStep>()
            .Input(appointment => appointment.UserId, data => data.UserId)
            .Input(appointment => appointment.TeacherId, data => data.SelectedTeacher.Id)
            .Input(appointment => appointment.AppointmentSlot, data => data.GetTimeSlot())
            .Input(appointment => appointment.AppointmentDate, data => data.AppointmentDate)
            .CleanupAndEndWorkflow(data => CreateAppointmentStepMessage.CreateSuccessMessage(data.SelectedTeacher.Name,
                data.AppointmentDate, data.PaginationMessage));
    }
}