using System.IO;
using System.Net.Http;
using AcademicAI.Core.Interfaces;

namespace AcademicAI.Core.Services;

public class UpdateService : IUpdateService
{
    private readonly HttpClient _http;

    public UpdateService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
    }

    public async Task DownloadUpdateAsync(string downloadUrl, string targetPath, IProgress<double>? progress)
    {
        using var response = await _http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        using var stream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;

            if (totalBytes > 0 && progress != null)
            {
                var percent = (double)totalRead / totalBytes * 100.0;
                progress.Report(percent);
            }
        }
    }
}
