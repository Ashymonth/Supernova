using SupernovaSchool.Models;

namespace SupernovaSchool.Abstractions;

public interface ICalendarService
{
    ValueTask<string> GetDefaultCalendarUrlAsync(Teacher teacher, CancellationToken ct = default);
}