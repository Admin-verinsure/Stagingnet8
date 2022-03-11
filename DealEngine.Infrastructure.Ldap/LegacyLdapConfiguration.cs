using Microsoft.Extensions.Configuration;
using System.Configuration;
using DealEngine.Infrastructure.Ldap.Interfaces;

namespace DealEngine.Infrastructure.Ldap
{
	public class LegacyLdapConfiguration : ILegacyLdapConfiguration
	{
        private IConfiguration _configuration { get; set; }
        public LegacyLdapConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string AdminDn {
			get {
                return _configuration.GetValue<string>("LegacyLdapBindDN");
                //return ConfigurationManager.AppSettings ["LegacyLdapBindDN"];
			}
		}

		public string AdminPassword {
			get {
                return _configuration.GetValue<string>("LegacyLdapBindPW");
                //return ConfigurationManager.AppSettings ["LegacyLdapBindPW"];
			}
		}

		public string LdapHost {
			get {
                return _configuration.GetValue<string>("LegacyLdapServer");
                //return ConfigurationManager.AppSettings ["LegacyLdapServer"];
			}
		}

		public int LdapPort {
			get {
                return _configuration.GetValue<int>("LegacyLdapPort");
                //return int.Parse (ConfigurationManager.AppSettings ["LegacyLdapPort"]);
			}
		}

		public string BaseDn {
			get {
                return _configuration.GetValue<string>("LegacyLdapBaseDN");
                //return ConfigurationManager.AppSettings ["LegacyLdapBaseDN"];
			}
		}

        public string UserDN
        {
            get
            {
                string baseDN = _configuration.GetValue<string>("OpenLdapBaseDN"); // ApacheLdapBaseDN
                return string.Format(_configuration.GetValue<string>("OpenLdapBaseUserDN"), baseDN);
            }
        }

        public string OpenLdapUserDNFromUsername
        {
            get
            {
                return _configuration.GetValue<string>("OpenLdapUserDNFromUsername");
            }
        }
    }
}

