using System.Net.Http;
using System.Text.Json;
using AcademicAI.Core.Helpers;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

public class RemoteControlService : IRemoteControlService
{
    private readonly HttpClient _http;
    private readonly ILicenseService _licenseService;
    private const string ControlUrl = "https://raw.githubusercontent.com/MrBlizo/AcademicAI-Suite/main/control.json";

    public RemoteControlService(ILicenseService licenseService)
    {
        _licenseService = licenseService;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
    }

    public async Task<RemoteCheckResult> CheckAsync()
    {
        var result = new RemoteCheckResult();

        try
        {
            var response = await _http.GetStringAsync(ControlUrl).ConfigureAwait(false);
            var data = JsonSerializer.Deserialize<ControlData>(response);
            if (data == null) return result;

            result.IsAlive = data.Alive;
            if (!data.Alive)
            {
                result.KillMessage = "This application has been deactivated. Please check for updates.";
            }

            var licenseInfo = _licenseService.GetLicenseInfo();
            if (data.RevokedKeys.Contains(licenseInfo.Key))
            {
                result.IsRevoked = true;
                result.KillMessage = "Your license key has been revoked.";
            }

            var currentVersion = data.Version;
            if (!string.IsNullOrEmpty(data.LatestVersion) && data.LatestVersion != currentVersion)
            {
                result.HasUpdate = true;
                result.LatestVersion = data.LatestVersion;
                result.DownloadUrl = data.DownloadUrl;
                result.MandatoryUpdate = data.MandatoryUpdate;
            }
        }
        catch
        {
        }

        return result;
    }
}
