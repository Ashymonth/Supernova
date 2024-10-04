namespace UtilityBills.Aggregates;

public interface IPasswordProtector
{
    string Protect(string password);

    string Unprotect(string protectedPassword);
}