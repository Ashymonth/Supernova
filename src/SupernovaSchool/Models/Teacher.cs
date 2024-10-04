using System.ComponentModel.DataAnnotations;
using UtilityBills.Aggregates.UtilityPaymentPlatformAggregate.ValueObjects;

namespace SupernovaSchool.Models;

public class Teacher
{
    //for ef
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // ReSharper disable once UnusedMember.Local
    private Teacher()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    private Teacher(string name, string login, Password yandexCalendarPassword)
    {
        Name = name;
        Login = login;
        YandexCalendarPassword = yandexCalendarPassword;
    }

    /// <summary>
    ///     Unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     The teacher name.
    /// </summary>
    [MaxLength(255)]
    public string Name { get; private set; }

    /// <summary>
    ///     The teacher email on yandex.
    /// </summary>
    [MaxLength(255)]
    public string Login { get; private set; }

    /// <summary>
    ///     A password to access yandex calendar.
    /// </summary>
    public Password YandexCalendarPassword { get; private set; }

    /// <summary>
    ///     Create a new model of teacher.
    /// </summary>
    /// <param name="name">Teacher name.</param>
    /// <param name="email">Teacher email on yandex</param>
    /// <param name="yandexCalendarPassword">A password to access yandex calendar.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Teacher Create(string name, string email, Password yandexCalendarPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentNullException.ThrowIfNull(yandexCalendarPassword);

        return new Teacher(name, email, yandexCalendarPassword);
    }
}