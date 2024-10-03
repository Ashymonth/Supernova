namespace SupernovaSchool.Models;

public class Student
{
    public string Id { get; init; } = null!;

    public string Name { get; set; } = null!;

    public string Class { get; set; } = null!;

    public string CreateAppointmentSummary()
    {
        return $"{Name} - Поток: {Class}";
    }
}