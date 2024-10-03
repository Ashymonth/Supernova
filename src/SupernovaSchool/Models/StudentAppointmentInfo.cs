namespace SupernovaSchool.Models;

public record StudentAppointmentInfo
{
    public string EventId { get; set; } = null!;

    public string TeacherName { get; init; } = null!;

    public DateTime DueDate { get; init; }
}