using System;
using System.Configuration;
using DealEngine.Infrastructure.BaseLdap.Interfaces;

namespace DealEngine.Infrastructure.BaseLdap.Services
{
	public class LdapConfigService : ILdapConfigService
	{
		#region New Ldap Configuration

		public string ServerAddress {
			get {
				return GetConfigValue("ApacheLdapServer"); // OpenLdapServer
			}
		}

		public int ServerPort {
			get {
				return Convert.ToInt32(GetConfigValue("ApacheLdapPort")); // OpenLdapPort
			}
		}

		public string[] SearchAttributes {
			get {
				return new string[] { "*", "+" };
			}
		}

		public string BaseDN {
			get {
				return GetConfigValue("ApacheLdapBaseDN"); // OpenLdapBaseDN
			}
		}

		public string AdminBindDN {
			get {
				//return string.Format(GetConfigValue("OpenLdapBindDN")), BaseDN);
				return string.Format(GetConfigValue("ApacheLdapBindDN"));

			}
		}

		public string AdminBindPassword {
			get {
				return GetConfigValue("ApacheLdapBindPW"); // OpenLdapBindPW
			}
		}


		public string BasePoliciesDN {
			get {
				return string.Format(GetConfigValue("OpenLdapBasePoliciesDN"), BaseDN);
			}
		}

		public string BaseUserDN {
			get {
				return string.Format(GetConfigValue("OpenLdapBaseUserDN"), BaseDN);
			}
		}

		public string BaseOrganisationDN { 
			get {
				return string.Format(GetConfigValue("OpenLdapBaseOrganisationDN"), BaseDN);
			}
		}

		public string BaseBranchDN {
			get {
				return string.Format(GetConfigValue("OpenLdapBaseBranchDN"), BaseDN);
			}
		}

		public string BaseDepartmentDN { 
			get {
				return string.Format(GetConfigValue("OpenLdapBaseDepartmentDN"), BaseDN);
			}
		}

		public string BaseRoleDN { 
			get {
				return string.Format(GetConfigValue("OpenLdapBaseRoleDN"), BaseDN);
			}
		}


		public string GetUserDN (string userName)
		{
			string s = GetConfigValue("OpenLdapUserDNFromUsername");
			return string.Format (s, userName, BaseUserDN);
   		}

		public string GetOrganisationDN (Guid organisationID)
		{
			string s = GetConfigValue("OpenLdapPilotOrgDNFromID");
			return string.Format (s, organisationID.ToString(), BaseOrganisationDN);
   		}

		public string GetDepartmentDN (Guid departmentID)
		{
			string s = GetConfigValue("OpenLdapPilotOrgDNFromID");
			return string.Format (s, departmentID.ToString(), BaseDepartmentDN);
		}

		public string GetBranchDN (Guid branchId)
		{
			string s = GetConfigValue("OpenLdapPilotOrgDNFromID");
			return string.Format (s, branchId.ToString(), BaseBranchDN);
		}

		public string GetRoleDN (string roleName)
		{
			string s = GetConfigValue("OpenLdapRoleDNFromID");
			return string.Format (s, roleName, BaseRoleDN);
		}


		public string GetBranchesDNByOrganisation (Guid organisationID)
		{
			string s = GetConfigValue("OpenLdapPilotOrgDNFromParentID");
			return string.Format (s, organisationID.ToString(), BaseBranchDN);
		}

		public string GetDepartmentsDNByOrganisation (Guid organisationID)
		{
			string s = GetConfigValue("OpenLdapPilotOrgDNFromParentID");
			return string.Format (s, organisationID.ToString(), BaseDepartmentDN);
		}


		public string GetUsersByIdSearch (Guid userID)
		{
			string s = GetConfigValue("OpenLdapSearchUserByID");
			return string.Format (s, userID.ToString());
		}

		public string GetUsersByEmailSearch(string email)
		{
			string s = GetConfigValue("OpenLdapSearchUserByEmail");
			return string.Format (s, email);
		}

		public string GetUsersByOrganisationSearch (Guid organisationID)
		{
			string s = GetConfigValue("OpenLdapSearchUserByOrganisationID");
			return string.Format (s, organisationID.ToString());
		}

		public string GetUsersByBranchSearch (Guid branchID)
		{
			string s = GetConfigValue("OpenLdapSearchUserByBranchID");
			return string.Format (s, branchID.ToString());
		}

		public string GetUsersByDepartmentSearch (Guid departmentID)
		{
			string s = GetConfigValue("OpenLdapSearchUserByDepartmentID");
			return string.Format (s, departmentID.ToString());
		}

		#endregion        

		protected string GetConfigValue(string key)
		{
			return ConfigurationManager.AppSettings [key];
		}
	}
}

