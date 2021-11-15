using DealEngine.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DealEngine.Services.Impl
{
    public class AppSettingService : IAppSettingService
    {
        private IConfiguration _configuration { get; set; }
        public AppSettingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string IsLinuxEnv
        {
            get
            {
                return _configuration.GetValue<string>("IsLinuxEnv");
            }
        }

        public string CarJamEndpoint
        {
            get
            {
                return _configuration.GetValue<string>("CarJamEndpoint");
            }
        }

        public string CarJamApiKey
        {
            get
            {
                return _configuration.GetValue<string>("CarJamApiKey");
            }
        }

        public string IntermediatePassword
        {
            get
            {
                return _configuration.GetValue<string>("IntermediatePassword");
            }
        }

        public string domainQueryString
        {
            get
            {
                return _configuration.GetValue<string>("domainQueryString");
            }
        }

        public string CKImagePath
        {
            get
            {
                return _configuration.GetValue<string>("CKImageFolderPath");
            }
        }

        public string MarshRSACredentials
        {
            get
            {
                return _configuration.GetValue<string>("MarshRSACredentials");
            }
        }
        public string MarshRSAOrgName
        {
            get
            {
                return _configuration.GetValue<string>("MarshRSAOrgName");
            }
        }

        public string MarshRSANotificationEmail
        {
            get
            {
                return _configuration.GetValue<string>("MarshRSANotificationEmail");
            }
        }

        public string MarshRSAEndPoint
        {
            get
            {
                return _configuration.GetValue<string>("MarshRSAEndPoint");
            }
        }

        public string RequireRSA
        {
            get
            {
                return _configuration.GetValue<string>("RequireRSA");
            }
        }

        public string GetMarineInsuranceSpecialistEmail
        {
            get
            {
                return _configuration.GetValue<string>("MarineInsuranceSpecialistEmail");
            }
        }

        public string GetCompanyTitle
        {
            get
            {
                return _configuration.GetValue<string>("companyLogo");
            }
        }

        public string GetConnectionString
        {
            get
            {
                return _configuration.GetConnectionString("DealEngineConnection");
            }
        }

        public string GetSuperUser
        {
            get
            {
                return _configuration.GetValue<string>("SuperUsers");
            }
        }

       
        public string NRecoUserName
        {
            get
            {
                return _configuration.GetValue<string>("NRecoUserName");
            }
        }

        public string NRecoLicense
        {
            get
            {
                return _configuration.GetValue<string>("NRecoLicense");
            }
        }

        public string NRecoPdfToolPath
        {
            get
            {
                return _configuration.GetValue<string>("NRecoPdfToolPath");
            }
        }

        public string MarshEglobalEndpoint
        {
            get
            {
                return _configuration.GetValue<string>("MarshEglobalEndpoint");
            }
        }

        public string MarshEglobalUsername
        {
            get
            {
                return _configuration.GetValue<string>("MarshEglobalUsername");
            }
        }

        public string MarshEglobalPassword
        {
            get
            {
                return _configuration.GetValue<string>("MarshEglobalPassword");
            }
        }
    }
}
