namespace SupernovaSchool.Abstractions;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}