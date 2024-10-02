namespace YandexCalendar.Net.Models;

public class Appointment
{
    public Appointment(string summary, DateTime startDate, DateTime endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        Summary = summary;
        StartDate = startDate;
        EndDate = endDate;
    }

    public string Summary { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate { get; }
}