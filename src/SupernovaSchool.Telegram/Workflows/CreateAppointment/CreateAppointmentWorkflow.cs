using SupernovaSchool.Telegram.Extensions;
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
                workflowBuilder
                    .Then<CleanupStep>()
                    .Input(step => step.UserId, data => data.UserId)
                    .SendMessageToUser("Сначала вы должна зарегистрироваться с помощью команды /register_as_student",
                        false)
                    .EndWorkflow())
            .SendInitialMessageToUser("Для записи к психологу выберите сотрудника из списка.")
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
            .Do(workflowBuilder => workflowBuilder
                .Then<CleanupStep>()
                .Input(step => step.UserId, data => data.UserId)
                .SendMessageToUser(
                    "На выбранный день нет доступх мест для записи. Выберите другой день или другого психолога", false)
                .EndWorkflow())
            .SendMessageToUser("Обработка запроса...")
            .Then<EnsureThatUserDosentRegisteredOnMeeting>()
            .Input(meeting => meeting.UserId, data => data.UserId)
            .Input(meeting => meeting.Date, data => DateOnly.Parse(data.PaginationMessage))
            .Output(data => data.UserHasAppointment, user => user.HasAppointment)
            .If(data => data.UserHasAppointment).Do(workflowBuilder =>
                workflowBuilder
                    .Then<CleanupStep>()
                    .Input(step => step.UserId, data => data.UserId)
                    .SendMessageToUser(
                        "У вас уже есть запись на этот день. На 1 день можно записать не больше 1 раза", false)
                    .EndWorkflow())
            .SendVariants("Выберите время для записи",
                data => data.AvailableTimeSlots.Select(slot => $"{slot.Start} -{slot.End}").ToArray())
            .WaitForUserMessage(data => data.PaginationMessage, message => message.Message)
            //
            .While(data => data.GetTimeSlot() == null || !data.AvailableTimeSlots.Contains(data.GetTimeSlot()))
            .Do(builder1 =>
            {
                builder1.SendVariantsPage("Выберите время для записи",
                        data => data.AvailableTimeSlots
                            .Select(slot => $"{slot.Start} -{slot.End}")
                            .ToArray())
                    .WaitForUserMessage(data => data.PaginationMessage, message => message.Message);
            })
            .SendMessageToUser("Обработка запроса...")
            .Then<CreateMeetingStep>()
            .Input(appointment => appointment.UserId, data => data.UserId)
            .Input(appointment => appointment.TeacherId, data => data.TeacherId)
            .Input(appointment => appointment.AppointmentSlot, data => data.GetTimeSlot())
            .Input(appointment => appointment.AppointmentDate, data => data.AppointmentDate)
            .Then<CleanupStep>()
            .Input(step => step.UserId, data => data.UserId)
            .SendMessageToUser("Вы успешно записаны", false)
            .EndWorkflow();
    }
}