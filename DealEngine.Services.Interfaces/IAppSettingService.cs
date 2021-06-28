using System;
using System.Linq;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IAppSettingService
    {
        string CarJamEndpoint { get; }
        string CarJamApiKey { get; }
        string IntermediatePassword { get; }
        string domainQueryString { get; }
        string CKImagePath { get; }
        string RequireRSA { get; }
        string GetMarineInsuranceSpecialistEmail { get; }
        string GetCompanyTitle { get; }
        string GetConnectionString { get; }
        string GetSuperUser { get; }
        string NRecoUserName { get; }
        string NRecoLicense { get; }
        string MarshRSAEndPoint { get; }
        string MarshRSAOrgName { get; }
        string MarshRSACredentials { get; }
        string NRecoPdfToolPath { get; }
        string IsLinuxEnv { get; }
    }
}

 
