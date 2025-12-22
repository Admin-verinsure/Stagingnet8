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
        string OktaIntermediatePassword { get; }
        string domainQueryString { get; }
        string oktaCallBackServiceURL { get; }
        string ClientIdClientSecret { get; }
        string AMPSUrl { get; }
        string CKImagePath { get; }
        string TCDE_internal_users { get; }
        string TCDE_external_Underwriters { get; }
        string TCDE_external_Admins { get; }
        string TCDE_external_users { get; }
        string RequireRSA { get; }
        string AuthenticationService { get; }
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
        string MarshRSANotificationEmail { get; }
        string MarshEglobalEndpoint { get; }
        string MarshEglobalUsername { get; }
        string MarshEglobalPassword { get; }
        string oktaServiceURL { get; }
        string OdooServerworkingendpoint { get; }
        string OdooServerDB { get; }
        string LoginID { get; }
        string LoginKey { get; }
        string FileBasePhysicalPath { get; }
    }
}

 
