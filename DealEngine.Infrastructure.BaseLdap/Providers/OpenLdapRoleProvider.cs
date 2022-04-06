using System;
using System.DirectoryServices;
using System.Configuration;

namespace DealEngine.Infrastructure.BaseLdap.Providers
{
	public class OpenLdapRoleProvider //: RoleProvider
	{
		protected string ServerAddress
		{
			get {
				string serverPort = ConfigurationManager.AppSettings["ApacheLdapPort"]; // OpenLdapPort
				return "LDAP://" + ConfigurationManager.AppSettings["ApacheLdapServer"] + ":" + serverPort; // OpenLdapServer
			}
		}

		protected string UserDN
		{
			get {
				string baseDN = ConfigurationManager.AppSettings["ApacheLdapBaseDN"]; // OpenLdapBaseDN
				return string.Format(ConfigurationManager.AppSettings["OpenLdapBaseUserDN"], baseDN);
			}
		}

		protected string OrganisationDN
		{
			get {
				string baseDN = ConfigurationManager.AppSettings["ApacheLdapBaseDN"]; // OpenLdapBaseDN
				return string.Format(ConfigurationManager.AppSettings["OpenLdapBaseOrganisationDN"], baseDN);
			}
		}

		protected TimeSpan ClientTimeout {
			get;
			set;
		}

		public OpenLdapRoleProvider()
		{
			ClientTimeout = new TimeSpan (0, 0, 10);
		}

		//#region implemented abstract members of RoleProvider

		//public override void AddUsersToRoles (string[] usernames, string[] rolenames)
		//{
		//	throw new NotSupportedException ("Roles cannot be added directly to a user, and are instead inherited from a Users Organisation only.");
		//}

		//public override void CreateRole (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool DeleteRole (string rolename, bool throwOnPopulatedRole)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string[] GetAllRoles ()
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string[] GetRolesForUser (string username)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof (username));

		//	List<string> roles = new List<string> ();

		//	try
		//	{
		//		DirectoryEntry root = new DirectoryEntry(ServerAddress + "/" + UserDN, null, null, AuthenticationTypes.None);
		//		DirectorySearcher searcher = new DirectorySearcher(root);

		//		searcher.SearchScope = SearchScope.Subtree;
		//		searcher.Filter = "uid=" + username;
		//		searcher.ClientTimeout = ClientTimeout;

		//		SearchResult findResult = searcher.FindOne();
		//		DirectoryEntry entry = findResult.GetDirectoryEntry();
		//		if (entry != null)
		//		{
		//			ResultPropertyValueCollection organisations = findResult.Properties["o"];
		//			foreach (var orgId in organisations) {
		//				string role = GetRoleForOrganisation (Guid.Parse (orgId.ToString ()));
		//				//Console.WriteLine ("Role for {0} : '{1}'", orgId, role);
		//				if (!string.IsNullOrWhiteSpace(role))
		//					roles.Add (role);
		//			}
		//		}
		//	}
		//	catch (Exception ex) {
		//		throw new Exception ("Unable to find roles for user " + username, ex);
		//	}
		//	return roles.Distinct().ToArray ();
		//}

		//public override string[] GetUsersInRole (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool IsUserInRole (string username, string rolename)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof(username));
		//	if (string.IsNullOrWhiteSpace (rolename))
		//		throw new ArgumentNullException (nameof(rolename));
			
		//	foreach (string role in GetRolesForUser(username))
		//		if (role == rolename)
		//			return true;
		//	return false;
		//}

		//public override void RemoveUsersFromRoles (string[] usernames, string[] rolenames)
		//{
		//	throw new NotSupportedException ("User roles can only be removed by removing the corresponding Organisation(s).");
		//}

		//public override bool RoleExists (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string ApplicationName {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//#endregion

		string GetRoleForOrganisation(Guid orgId)
		{
			try
			{
				DirectoryEntry root = new DirectoryEntry(ServerAddress + "/" + OrganisationDN, null, null, AuthenticationTypes.None);
				DirectorySearcher searcher = new DirectorySearcher(root);

				searcher.SearchScope = SearchScope.Subtree;
				searcher.Filter = "o=" + orgId;
				searcher.ClientTimeout = ClientTimeout;

				SearchResult findResult = searcher.FindOne();
				DirectoryEntry entry = findResult.GetDirectoryEntry();
				if (entry != null)
				{
					var prop = findResult.Properties ["businessCategory"];
					if (prop != null && prop.Count > 0) {
						var role = prop[0];
						return role.ToString ();
					}
				}
			}
			catch (Exception ex) {
				Console.WriteLine ("Unable to get role for organisationId " + orgId);
				throw new Exception ("Unable to get role for organisationId " + orgId, ex);
			}
			return string.Empty;
		}

		//RoleManagerSection GetConfig ()
		//{
		//	RoleManagerSection section = (RoleManagerSection)WebConfigurationManager.GetSection ("system.web/roleManager");
		//	return section;
		//}
	}
}

