using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AcademicAI.Core.Helpers;

public static class AesHelper
{
    private static readonly byte[] MasterKey = Convert.FromBase64String("3vctpeqkc89i8JoJco/+AFudci/RKCA1sJaZOhU4n2s=");
    private static readonly byte[] IV = Convert.FromBase64String("r2wcrxhHZ9IAFP2AlGyfYA==");

    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = MasterKey;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public static string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = MasterKey;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public static string GenerateLicenseKey()
    {
        var segments = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(4);
            segments.Add(Convert.ToHexString(bytes));
        }
        return string.Join("-", segments);
    }
}
