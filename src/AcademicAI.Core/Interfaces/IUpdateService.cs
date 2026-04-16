namespace AcademicAI.Core.Interfaces;

public interface IUpdateService
{
    Task DownloadUpdateAsync(string downloadUrl, string targetPath, IProgress<double>? progress);
}
