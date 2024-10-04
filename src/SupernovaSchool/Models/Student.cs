using System.ComponentModel.DataAnnotations;

namespace SupernovaSchool.Models;

public class Student
{
    [MaxLength(100)]
    public string Id { get; init; } = null!;

    [MaxLength(255)]
    public string Name { get; set; } = null!;

    [MaxLength(15)]
    public string Class { get; set; } = null!;

    public string CreateAppointmentSummary()
    {
        return $"{Name} - Поток: {Class}";
    }
}