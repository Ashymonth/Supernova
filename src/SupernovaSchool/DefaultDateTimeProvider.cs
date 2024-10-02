using SupernovaSchool.Abstractions;

namespace SupernovaSchool;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}