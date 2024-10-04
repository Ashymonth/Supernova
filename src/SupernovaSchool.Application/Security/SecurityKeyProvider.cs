using System.Text;
using UtilityBills.Aggregates;

namespace UtilityBills.Host.Security;

public class SecurityKeyProvider : ISecurityKeyProvider
{
    private readonly string _secretKey;
    private readonly string _initVector;

    public SecurityKeyProvider(string secretKey, string initVector)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentNullException(nameof(secretKey), "Secret key can't be null");
        }
        
        if (string.IsNullOrWhiteSpace(initVector))
        {
            throw new ArgumentNullException(nameof(secretKey), "InitVector key can't be null");
        }

        _secretKey = secretKey;
        _initVector = initVector;
    }


    public byte[] GetKey() => Encoding.UTF8.GetBytes(_secretKey);

    public byte[] GetInitVector() => Encoding.UTF8.GetBytes(_initVector);
}