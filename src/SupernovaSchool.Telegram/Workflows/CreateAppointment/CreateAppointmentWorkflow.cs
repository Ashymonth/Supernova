using SupernovaSchool.Telegram.Extensions;
using SupernovaSchool.Telegram.Extensions.Steps;
using SupernovaSchool.Telegram.Steps;
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
            .Then<EnsureThatStudentRegisteredStep>()
            .Input(step => step.UserId, data => data.UserId)
            .Output(data => data.IsStudentRegistered, step => step.IsStudentRegistered)
            .If(data => !data.IsStudentRegistered).Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage.UserNotRegistered))
            .SendMessageToUser(
                DefaultStepMessage.CreateInitialMessage(CreateAppointmentStepMessage.ChooseTeacherFromList))
            .Then<SendTeacherListStep>()
            .Input(step => step.UserId, data => data.UserId)
            .WaitForUserInlineData(data => data.TeacherId, o => Guid.Parse((o as UserMessage)!.Message))
            .SendAvailableMetingDates()
            .Then<SendAvailableTimeSlotsStep>()
            .Input(slots => slots.TeacherId, data => data.TeacherId)
            .Input(slots => slots.DueDate, data => data.AppointmentDate)
            .Output(data => data.AvailableTimeSlots, slots => slots.AvailableSlots)
            .If(data => data.AvailableTimeSlots.Length == 0)
            //
            .Do(workflowBuilder =>
                workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage.NoAvailableTimeSlots))
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<EnsureThatUserDosentRegisteredOnMeeting>()
            .Input(meeting => meeting.UserId, data => data.UserId)
            .Input(meeting => meeting.Date, data => DateOnly.Parse(data.PaginationMessage))
            .Output(data => data.UserHasAppointment, user => user.HasAppointment)
            .If(data => data.UserHasAppointment).Do(
                workflowBuilder => workflowBuilder.CleanupAndEndWorkflow(CreateAppointmentStepMessage
                    .AlreadyHaveAppointmentOnSelectedDay)
            )
            .While(data => data.GetTimeSlot() == null || !data.AvailableTimeSlots.Contains(data.GetTimeSlot()))
            .Do(builder1 =>
            {
                builder1.SendVariantsPage(CreateAppointmentStepMessage.SelectTimeSlot,
                        data => data.AvailableTimeSlots
                            .Select(slot => $"{slot.Start} -{slot.End}")
                            .ToArray())
                    .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
            })
            .SendMessageToUser(DefaultStepMessage.ProcessingRequest)
            .Then<CreateMeetingStep>()
            .Input(appointment => appointment.UserId, data => data.UserId)
            .Input(appointment => appointment.TeacherId, data => data.TeacherId)
            .Input(appointment => appointment.AppointmentSlot, data => data.GetTimeSlot())
            .Input(appointment => appointment.AppointmentDate, data => data.AppointmentDate)
            .CleanupAndEndWorkflow(data => CreateAppointmentStepMessage.CreateSuccessMessage(data.TeacherId.ToString(),
                data.AppointmentDate.ToShortDateString(), data.PaginationMessage));
    }
}