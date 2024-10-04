
namespace UtilityBills.Aggregates.UtilityPaymentPlatformAggregate.ValueObjects;

public record Password
{
    private string Value { get;  }
    
    private Password(string value)
    {
        Value = value;
    }

    public string GetUnprotected(IPasswordProtector passwordProtector)
    {
        return passwordProtector.Unprotect(Value);
    }

    public static Password Create(string password, IPasswordProtector passwordProtector)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException();
        }

        return new Password(passwordProtector.Protect(password));
    }
}