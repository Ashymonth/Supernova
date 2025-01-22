using System.Security.Cryptography;
using SupernovaSchool.Abstractions.Security;

namespace SupernovaSchool.Application.Security;

public class PasswordProtector(ISecurityKeyProvider securityKeyProvider) : IPasswordProtector
{
    public string Protect(string password)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = securityKeyProvider.GetKey();
        aesAlg.IV = securityKeyProvider.GetInitVector();

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
        aesAlg.Key = securityKeyProvider.GetKey();
        aesAlg.IV = securityKeyProvider.GetInitVector();

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}