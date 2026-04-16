using System.IO;
using AcademicAI.Core.Helpers;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

public class LicenseService : ILicenseService
{
    private readonly string _filePath;
    private LicenseInfo? _cachedInfo;

    public LicenseService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AcademicAI");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "license.enc");
    }

    public LicenseInfo GetLicenseInfo()
    {
        if (_cachedInfo != null) return _cachedInfo;

        if (!File.Exists(_filePath))
        {
            var key = AesHelper.GenerateLicenseKey();
            var info = new LicenseInfo
            {
                Key = key,
                GeneratedAt = DateTime.UtcNow,
                IsValid = true
            };
            SaveLicense(info);
            _cachedInfo = info;
            return info;
        }

        try
        {
            var encrypted = File.ReadAllText(_filePath);
            var json = AesHelper.Decrypt(encrypted);
            _cachedInfo = System.Text.Json.JsonSerializer.Deserialize<LicenseInfo>(json) ?? CreateNewLicense();
        }
        catch
        {
            _cachedInfo = CreateNewLicense();
        }

        return _cachedInfo;
    }

    public bool ValidateLicense()
    {
        var info = GetLicenseInfo();
        return info.IsValid;
    }

    private LicenseInfo CreateNewLicense()
    {
        var info = new LicenseInfo
        {
            Key = AesHelper.GenerateLicenseKey(),
            GeneratedAt = DateTime.UtcNow,
            IsValid = true
        };
        SaveLicense(info);
        return info;
    }

    private void SaveLicense(LicenseInfo info)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(info);
        var encrypted = AesHelper.Encrypt(json);
        File.WriteAllText(_filePath, encrypted);
    }
}
