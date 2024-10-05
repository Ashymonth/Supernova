namespace SupernovaSchool.Abstractions;

public interface IAdminsProvider
{
    bool IsAdmin(string userId);
}