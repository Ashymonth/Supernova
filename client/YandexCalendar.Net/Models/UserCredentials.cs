using System.Text;

namespace YandexCalendar.Net.Models;

public record UserCredentials
{
    public UserCredentials(string userName, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(userName);
        ArgumentException.ThrowIfNullOrEmpty(password);

        UserName = userName;
        Password = password;
    }

    public string UserName { get; }

    public string Password { get; }

    public string ToBasic64Credentials()
    {
        return Convert.ToBase64String(Encoding.ASCII.GetBytes($"{UserName}:{Password}"));
    }
}