using Microsoft.Extensions.Configuration;
using System;
using DealEngine.Infrastructure.Ldap.Interfaces;

namespace DealEngine.Infrastructure.Ldap
{
	public class LdapConfiguration : ILdapConfiguration
	{
        private IConfiguration _configuration { get; set; }
        public LdapConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
		public string AdminDn {
			get {
                //return _configuration.GetValue<string>("OpenLdapBindDN") + BaseDn;
                return _configuration.GetValue<string>("ApacheLdapBindDN");// + BaseDn);
                //return string.Format (ConfigurationManager.AppSettings ["OpenLdapBindDN"], BaseDn);
			}
		}

		public string AdminPassword {
			get {
                return _configuration.GetValue<string>("ApacheLdapBindPW"); // OpenLdapBindPw
                //return ConfigurationManager.AppSettings ["OpenLdapBindPW"];
            }
		}

		public string LdapHost {
			get {
                return _configuration.GetValue<string>("ApacheLdapServer"); // OpenLdapServer
                //return ConfigurationManager.AppSettings ["OpenLdapServer"];
            }
		}

		public int LdapPort {
			get {
                return _configuration.GetValue<int>("ApacheLdapPort"); // OpenLdapPort
                //return int.Parse (ConfigurationManager.AppSettings ["OpenLdapPort"]);
            }
		}

		public string BaseDn {
			get {
                return _configuration.GetValue<string>("ApacheLdapBaseDN"); // OpenLdapBaseDN
                //return ConfigurationManager.AppSettings ["OpenLdapBaseDN"];
            }
		}

        public string UserDN
        {
            get
            {
                string baseDN = _configuration.GetValue<string>("ApacheLdapBaseDN"); // OpenLdapBaseDN
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

