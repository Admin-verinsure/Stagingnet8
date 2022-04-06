using Bismuth.Ldap;
using Bismuth.Ldap.Requests;
using Bismuth.Ldap.Responses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Identity;



namespace DealEngine.Infrastructure.Ldap.Providers
{
	public class BismuthLdapRoleProvider //: Identity
	{
		protected string ServerAddress {
			get {
				return ConfigurationManager.AppSettings ["ApacheLdapServer"]; // OpenLdapServer
			}
		}

		protected int ServerPort {
			get {
				return Convert.ToInt32 (ConfigurationManager.AppSettings ["ApacheLdapPort"]); // OpenLdapPort
			}
		}

		protected string UserDN {
			get {
				string baseDN = ConfigurationManager.AppSettings ["ApacheLdapBaseDN"]; // OpenLdapBaseDN
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseUserDN"], baseDN);
			}
		}

		protected string OrganisationDN {
			get {
				string baseDN = ConfigurationManager.AppSettings["ApacheLdapBaseDN"]; // OpenLdapBaseDN
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseOrganisationDN"], baseDN);
			}
		}

		//public override string ApplicationName {
		//	get {
		//		throw new NotImplementedException ();
		//	}

		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override void AddUsersToRoles (string [] usernames, string [] roleNames)
		//{
		//	throw new NotSupportedException ("Roles cannot be added directly to a user, and are instead inherited from a Users Organisation only");
		//}

		//public override void CreateRole (string roleName)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string [] FindUsersInRole (string roleName, string usernameToMatch)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string [] GetAllRoles ()
		//{
		//	throw new NotImplementedException ();
		//}

		LdapClient GetLdapConnection ()
		{
			//string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"]; // 
			//string adminUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBindDN"], baseDN);
			string adminUserDN = string.Format(ConfigurationManager.AppSettings["ApacheLdapBindDN"]);		
			string adminUserPassword = ConfigurationManager.AppSettings ["ApacheLdapBindPW"]; // OpenLdapBindPW

			return GetLdapConnection (adminUserDN, adminUserPassword);
		}

		LdapClient GetLdapConnection (string ldapUser, string password)
		{
			LdapClient ldapClient = new LdapClient (ServerAddress, ServerPort);
			ldapClient.Bind (ldapUser, password, BindAuthentication.Simple);

			return ldapClient;
		}

		string GetRoleForOrganisation (string orgId, LdapClient client)
		{
			string role = string.Empty;
			var searchRequest = new SearchRequest (client.NextMessageId) {
				BaseObject = OrganisationDN,
				Scope = SearchScope.SingleLevel,
				SearchFilter = "(o=" + orgId + ")",
				Attributes = new string [] { "businessCategory" },
			};
			var searchResponse = client.Send <SearchResponse> (searchRequest);
			if (searchResponse != null && searchResponse.Results.Count > 0)
				role = searchResponse.Results [0].GetAttributeValue ("businessCategory");

			return role;
		}

		string GetUsernameDN (string username)
		{
			return string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], username, UserDN);
		}
	}
}

