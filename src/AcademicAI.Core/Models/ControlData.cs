using System.Text.Json.Serialization;

namespace AcademicAI.Core.Models;

public class ControlData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "3.0.0";

    [JsonPropertyName("alive")]
    public bool Alive { get; set; } = true;

    [JsonPropertyName("revokedKeys")]
    public List<string> RevokedKeys { get; set; } = [];

    [JsonPropertyName("latestVersion")]
    public string LatestVersion { get; set; } = "3.0.0";

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = "";

    [JsonPropertyName("mandatoryUpdate")]
    public bool MandatoryUpdate { get; set; } = false;
}
