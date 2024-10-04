using System.Security.Cryptography;
using UtilityBills.Aggregates;

namespace UtilityBills.Host.Security;

public class PasswordProtector : IPasswordProtector
{
    private readonly ISecurityKeyProvider _securityKeyProvider;

    public PasswordProtector(ISecurityKeyProvider securityKeyProvider)
    {
        _securityKeyProvider = securityKeyProvider;
    }

    public string Protect(string password)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = _securityKeyProvider.GetKey();
        aesAlg.IV = _securityKeyProvider.GetInitVector();

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        byte[] encryptedBytes;

        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(password);
                }

                encryptedBytes = msEncrypt.ToArray();
            }
        }

        return Convert.ToBase64String(encryptedBytes);
    }

    public string Unprotect(string protectedPassword)
    {
        var cipherBytes = Convert.FromBase64String(protectedPassword);

        using var aesAlg = Aes.Create();
        aesAlg.Key = _securityKeyProvider.GetKey();
        aesAlg.IV = _securityKeyProvider.GetInitVector();

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}