using AcademicAI.Core.Models;

namespace AcademicAI.Core.Interfaces;

public interface IRemoteControlService
{
    Task<RemoteCheckResult> CheckAsync();
}
