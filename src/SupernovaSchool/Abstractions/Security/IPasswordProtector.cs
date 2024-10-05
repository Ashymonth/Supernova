namespace SupernovaSchool.Abstractions.Security;

public interface IPasswordProtector
{
    string Protect(string password);

    string Unprotect(string protectedPassword);
}