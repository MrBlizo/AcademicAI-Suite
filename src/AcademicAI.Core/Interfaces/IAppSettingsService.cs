using AcademicAI.Core.Models;

namespace AcademicAI.Core.Interfaces;

public interface IAppSettingsService
{
    AppSettings Settings { get; }
    void Load();
    void Save();
    FeatureProviderConfig? GetFeatureDefault(string feature);
    void SetFeatureDefault(string feature, string provider, string model);
}
