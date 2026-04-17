using System.IO;
using AcademicAI.Core.Helpers;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.Core.Services;

public class AesSecretStore : ISecretStore
{
    private readonly string _dir;

    public AesSecretStore()
    {
        _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcademicAI", "secrets");
        Directory.CreateDirectory(_dir);
    }

    public void Save(string key, string value)
    {
        var encrypted = AesHelper.Encrypt(value);
        var filePath = GetFilePath(key);
        File.WriteAllText(filePath, encrypted);
    }

    public string? Load(string key)
    {
        var filePath = GetFilePath(key);
        if (!File.Exists(filePath)) return null;
        try
        {
            var encrypted = File.ReadAllText(filePath);
            return AesHelper.Decrypt(encrypted);
        }
        catch
        {
            try { File.Delete(filePath); } catch { }
            return null;
        }
    }

    public void Delete(string key)
    {
        var filePath = GetFilePath(key);
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    public bool Exists(string key)
    {
        return File.Exists(GetFilePath(key));
    }

    private string GetFilePath(string key)
    {
        var safeName = string.Concat(key.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_dir, $"{safeName}.enc");
    }
}
