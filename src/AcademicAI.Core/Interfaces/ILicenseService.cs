using AcademicAI.Core.Models;

namespace AcademicAI.Core.Interfaces;

public interface ILicenseService
{
    LicenseInfo GetLicenseInfo();
    bool ValidateLicense();
}
