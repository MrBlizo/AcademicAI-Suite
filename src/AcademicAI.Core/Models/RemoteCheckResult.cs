namespace AcademicAI.Core.Models;

public class RemoteCheckResult
{
    public bool IsAlive { get; set; } = true;
    public string KillMessage { get; set; } = "";
    public bool IsRevoked { get; set; } = false;
    public bool HasUpdate { get; set; } = false;
    public bool MandatoryUpdate { get; set; } = false;
    public string LatestVersion { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
}
