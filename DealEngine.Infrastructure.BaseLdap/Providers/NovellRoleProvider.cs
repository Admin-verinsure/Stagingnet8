using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Novell.Directory.Ldap;

namespace DealEngine.Infrastructure.BaseLdap.Providers
{
	public class NovellRoleProvider //: RoleProvider
	{
		protected string ServerAddress {
			get {
				return ConfigurationManager.AppSettings ["OpenLdapServer"];
			}
		}

		protected int ServerPort {
			get {
				return Convert.ToInt32(ConfigurationManager.AppSettings ["OpenLdapPort"]);
			}
		}

		protected string BaseDN {
			get {
				return ConfigurationManager.AppSettings ["OpenLdapBaseDN"];
			}
		}

		protected string BaseUserDN {
			get {
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseUserDN"], BaseDN);
			}
		}

		protected string OrganisationDN {
			get {
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseOrganisationDN"], BaseDN);
			}
		}

		public NovellRoleProvider ()
		{
		}

		//#region implemented abstract members of RoleProvider

		//public override string ApplicationName {
		//	get {
		//		throw new NotImplementedException ();
		//	}

		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override void AddUsersToRoles (string [] usernames, string [] rolenames)
		//{
		//	throw new NotSupportedException ("Roles cannot be added directly to a user, and are instead inherited from a Users Organisation only");
		//}

		//public override void CreateRole (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool DeleteRole (string rolename, bool throwOnPopulatedRole)
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

		//public override string [] GetRolesForUser (string username)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof (username));

		//	List<string> roles = new List<string> ();

		//	try {
		//		LdapConnection conn = GetConnection ();
		//		if (conn != null) {
		//			StringBuilder userdn = new StringBuilder ();
		//			userdn.Append ("uid=");
		//			userdn.Append (username);
		//			userdn.Append (",ou=users,");
		//			userdn.Append (BaseDN);

		//			LdapEntry entry = conn.Read(userdn.ToString(), new string[]{ "o" });
		//			if (entry != null) {
		//				LdapAttribute attr = entry.getAttribute ("o");
		//				if (attr != null)
		//					foreach (string s in attr.StringValueArray)
		//						roles.Add (GetRoleForOrganisation (s));
		//			}
		//		}
		//	}
		//	catch (Exception ex) {
		//		throw new Exception ("Unable to find roles for user " + username, ex);
		//	}
		//	return roles.Distinct ().ToArray ();
		//}

		//public override string [] GetUsersInRole (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool IsUserInRole (string username, string rolename)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof (username));
		//	if (string.IsNullOrWhiteSpace (rolename))
		//		throw new ArgumentNullException (nameof (rolename));

		//	foreach (string role in GetRolesForUser (username))
		//		if (role == rolename)
		//			return true;
		//	return false;
		//}

		//public override void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		//{
		//	throw new NotSupportedException ("User roles can only be removed by removing the corresponding Organisation(s)");
		//}

		//public override bool RoleExists (string rolename)
		//{
		//	throw new NotImplementedException ();
		//}

		//#endregion

		LdapConnection GetConnection ()
		{
			LdapConnection connection = new LdapConnection ();
			connection.Connect (ServerAddress, ServerPort);

			string adminUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBindDN"]);//, BaseDN);
			string adminUserPassword = ConfigurationManager.AppSettings ["OpenLdapBindPW"];
			connection.Bind (adminUserDN, adminUserPassword);

			if (connection.Bound)
				return connection;
			throw new Exception ("Unable to connect to ldap");
		}

		string GetRoleForOrganisation (string orgId)
		{
			try {
				Guid guid = Guid.Empty;
				if (Guid.TryParse(orgId, out guid))
					return "superuser";
				return string.Empty;
			}
			catch (Exception ex) {
				throw new Exception ("Unable to get role for organisationId " + orgId, ex);
			}
		}
	}
}

