using System;
using System.Collections.Generic;
using Bismuth.Ldap;
using Bismuth.Ldap.Requests;
using Bismuth.Ldap.Responses;
using DealEngine.Domain.Entities;
using DealEngine.Domain.Exceptions;
using DealEngine.Infrastructure.Ldap.Interfaces;

namespace DealEngine.Infrastructure.Ldap.Services
{
	public class LdapService : ILdapService
	{
		ILdapConfiguration _ldapConfiguration;

		ILdapEntityMapping<User> _userMapping;
		ILdapEntityMapping<Organisation> _organisationMapping;

		public LdapService (ILdapConfiguration ldapConfiguration, ILdapEntityMapping<User> userMapping, ILdapEntityMapping<Organisation> organisationMapping)
		{
			_ldapConfiguration = ldapConfiguration;
			_userMapping = userMapping;
			_organisationMapping = organisationMapping;
		}

		public User GetUser (string username)
		{
			using (LdapClient client = GetLdapServer (false)) {
				LdapEntry entry = SearchForSingle (client, "(uid=" + username + ")", "ou=users," + _ldapConfiguration.BaseDn);
				if (entry != null) {
					User user = _userMapping.FromLdap (entry);
					//user.Organisations = GetOrganisationsForUser (client, user.UserName);
					return user;
				}
			}
			return null;
		}

		public User GetUser (Guid userId)
		{
			using (LdapClient client = GetLdapServer (false)) {
				LdapEntry entry = SearchForSingle (client, "(employeeNumber=" + userId + ")", "ou=users," + _ldapConfiguration.BaseDn);
				if (entry != null) {
					User user = _userMapping.FromLdap (entry);
					//user.Organisations = GetOrganisationsForUser (client, user.UserName);
					return user;
				}
			}
			return null;
		}

		public User GetUserByEmail (string email)
		{
			using (LdapClient client = GetLdapServer (false)) {
				LdapEntry entry = SearchForSingle (client, "(mail=" + email + ")", "ou=users," + _ldapConfiguration.BaseDn);
				if (entry != null) {
					User user = _userMapping.FromLdap (entry);
					//user.Organisations = GetOrganisationsForUser (client, user.UserName);
					return user;
				}
			}
			return null;
		}

		public void Validate (string username, string password, out int resultCode, out string resultMessage)
		{
			using (LdapClient client = GetLdapServer ()) {
				BindResponse response = client.Send<BindResponse> (new BindRequest (client.NextMessageId) {
					Authentication = (int)BindAuthentication.Simple,
					BindDN = string.Format("uid={0},ou=users,{1}", username, _ldapConfiguration.BaseDn),
					Password = password
				});
				resultCode = response.ResultCode;
				resultMessage = response.ErrorMessage;
			}
		}

		public Organisation GetOrganisation (string organisationName)
		{
			using (LdapClient client = GetLdapServer (false)) {
				LdapEntry entry = SearchForSingle (client, "(buildingName=" + organisationName + ")", "ou=organisations," + _ldapConfiguration.BaseDn);
				if (entry != null)
					return _organisationMapping.FromLdap (entry);
			}
			return null;
		}

		public Organisation GetOrganisation (Guid organisationId)
		{
			using (LdapClient client = GetLdapServer (false)) {
				LdapEntry entry = SearchForSingle (client, "(o=" + organisationId + ")", "ou=organisations," + _ldapConfiguration.BaseDn);
				if (entry != null)
					return _organisationMapping.FromLdap (entry);
			}
			return null;
		}

		//public PasswordPolicy GetPasswordPolicy (string passwordPolicyName)
		//{
		//	throw new NotImplementedException ();
		//}

		public void SetPasswordPolicyFor (User user, string passwordPolicyName)
		{

		}

		public void Create (User user)
		{
			using (LdapClient client = GetLdapServer (true)) {
				LdapEntry entry = _userMapping.ToLdap (user, _ldapConfiguration.BaseDn);
				var addRequest = new AddRequest (client.NextMessageId, entry);
				var response = client.Send <AddResponse> (addRequest);
				client.Unbind ();
				if (response.ResultCode > 0)
					throw new Exception ("Unable to create user in Ldap: " + response.ErrorMessage);
			}
			//foreach (Organisation org in user.Organisations)
			//	if (GetOrganisation(org.Id) == null)
			//		Create (org);
		}

		public void Create (Organisation organisation)
        {
            using (LdapClient client = GetLdapServer(true))
            {
                LdapEntry entry = _organisationMapping.ToLdap(organisation, _ldapConfiguration.BaseDn);
                var addRequest = new AddRequest(client.NextMessageId, entry);
                var response = client.Send<AddResponse>(addRequest);
                client.Unbind();
                if (response.ResultCode == 68)
                {
                    var nameExistsException = new Exception("NameAlreadyBoundException");
                    nameExistsException.HResult = 68;
                    throw nameExistsException;
                }
                else if (response.ResultCode == 0)
                {
                    var ldapTimeOut = new Exception("LdapTimeOut");
					ldapTimeOut.HResult = 68;
                    throw ldapTimeOut;
                }
                else if (response.ResultCode > 0)
                {
                    throw new Exception("Unable to create organisation in Ldap: " + response.ErrorMessage);
                }
            }
        }

        public void  Update (User user)
		{
			using (LdapClient client = GetLdapServer (true)) {
				var mods = _userMapping.ToModify (user);
				var modifyRequest = new ModifyRequest (client.NextMessageId) {
					EntryName = _userMapping.GetDn (user, _ldapConfiguration.BaseDn),
					Attributes = mods
				};
				var response = client.Send <ModifyResponse> (modifyRequest);
				client.Unbind ();
				if (response.ResultCode > 0)
					throw new Exception ("Unable to modify user in Ldap: " + response.ErrorMessage);
			}
		}

		public void Update (Organisation organisation)
		{
			using (LdapClient client = GetLdapServer (true)) {
				var mods = _organisationMapping.ToModify (organisation);
				var modifyRequest = new ModifyRequest (client.NextMessageId) {
					EntryName = _organisationMapping.GetDn (organisation, _ldapConfiguration.BaseDn),
					Attributes = mods
				};
				var response = client.Send<ModifyResponse> (modifyRequest);
				client.Unbind ();
				if (response.ResultCode > 0)
					throw new Exception ("Unable to modify organisation in Ldap: " + response.ErrorMessage);
			}
		}

		public void GlobalBan (User user)
		{
			// Note: Cannot add operational attributes
			using (LdapClient client = GetLdapServer (true)) {
				client.Send<ModifyResponse> (new ModifyRequest (client.NextMessageId) {
					EntryName = "uid=" + user.UserName + ",ou=users,dc=tcauth,dc=proposalonline,dc=com",
					Attributes = new List<ModifyAttribute> {
						new ModifyAttribute ("pwdAccountLockedTime", ModificationType.Add, LdapDateTime.UtcNow)
					}
				});
			}
		}

		public void RemoveGlobalBan (User user)
		{
			using (LdapClient client = GetLdapServer (true)) {
				SearchResponse response = client.Send<SearchResponse> (new SearchRequest (client.NextMessageId) {
					SearchFilter = "(uid=" + user.UserName + ")",
					BaseObject = "ou=users,dc=tcauth,dc=proposalonline,dc=com",
					Scope = SearchScope.SingleLevel,
					Attributes = new string[] { "pwdAccountLockedTime" }
				});
				if (response.HasResults) {
					var result = response.Results [0];

					client.Send<ModifyResponse> (new ModifyRequest (client.NextMessageId) {
						EntryName = "uid=" + user.UserName + ",ou=users,dc=tcauth,dc=proposalonline,dc=com",
						Attributes = new List<ModifyAttribute> {
							new ModifyAttribute ("pwdAccountLockedTime", ModificationType.Delete, result.GetAttributeValue("pwdAccountLockedTime"))
						}
					});
				}
			}
		}

		LdapClient GetLdapServer ()
        {
            return new LdapClient (_ldapConfiguration.LdapHost, _ldapConfiguration.LdapPort);
		}

		LdapClient GetLdapServer (bool isAdmin)
		{
			if (isAdmin)
				return GetLdapServer (_ldapConfiguration.AdminDn, _ldapConfiguration.AdminPassword);
			return GetLdapServer ("", "");
		}

		LdapClient GetLdapServer (string userDN, string password)
		{
			LdapClient client = new LdapClient (_ldapConfiguration.LdapHost, _ldapConfiguration.LdapPort);
			client.Bind (userDN, password, BindAuthentication.Simple);
			return client;
		}

		LdapEntry SearchForSingle (LdapClient client, string searchDN)
		{
			return SearchForSingle (client, searchDN, _ldapConfiguration.BaseDn);
		}

		LdapEntry SearchForSingle (LdapClient client, string searchDN, string baseSearchDN)
		{
			var searchResponse = client.Send<SearchResponse> (new SearchRequest (client.NextMessageId) {
				Attributes = new string [] { "*", "+" },
				BaseObject = baseSearchDN,
				SearchFilter = searchDN,
				Scope = SearchScope.Subtree
			});
			if (searchResponse.HasResults)
				return searchResponse.Results [0];
			return null;
		}

		List<Organisation> GetOrganisationsForUser (LdapClient ldapClient, string username)
		{
			List<Organisation> organisations = new List<Organisation> ();
			LdapEntry userEntry = SearchForSingle (ldapClient, "(uid=" + username + ")");
			if (userEntry != null)
				foreach (var strId in userEntry.GetAttributeValues ("o")) {
					Guid guidOrgId = Guid.Empty;
					if (Guid.TryParse (strId, out guidOrgId))
						organisations.Add (GetOrganisation (guidOrgId));
				}
			return organisations;
		}

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentNullException(nameof(newPassword));

            bool result = false;

            string userDN = GetUsernameDN(username);

            string bindDN = userDN;
            string bindPw = oldPassword;
            Console.WriteLine(username + " " + oldPassword + " " + newPassword);
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                string baseDN = _ldapConfiguration.BaseDn;
                bindDN = string.Format(_ldapConfiguration.AdminDn, baseDN);
                bindPw = _ldapConfiguration.AdminPassword;
            }

            using (LdapClient ldapClient = GetLdapConnection(bindDN, bindPw))
            {
                var modifyRequest = new ModifyRequest(ldapClient.NextMessageId)
                {
                    EntryName = userDN,
                    Attributes = new List<ModifyAttribute> {
                        new ModifyAttribute("userpassword", ModificationType.Replace, newPassword)
                    }
                };
                var response = ldapClient.Send<ModifyResponse>(modifyRequest);
                if (response.ResultCode == 0)
                    return true;

                // error otherwise
                throw new AuthenticationException(response.ErrorMessage) { ErrorCode = response.ResultCode, User = username };
            }
        }

        LdapClient GetLdapConnection()
        {
            string baseDN = _ldapConfiguration.BaseDn;
            string adminUserDN = string.Format(_ldapConfiguration.AdminDn, baseDN);
            string adminUserPassword = _ldapConfiguration.AdminPassword;

            return GetLdapConnection(adminUserDN, adminUserPassword);
        }

        LdapClient GetLdapConnection(string ldapUser, string password)
        {
            LdapClient ldapClient = new LdapClient(_ldapConfiguration.LdapHost, _ldapConfiguration.LdapPort);
            ldapClient.Bind(ldapUser, password, BindAuthentication.Simple);

            return ldapClient;
        }

        public string GetUsernameDN(string username)
        {
            return string.Format(_ldapConfiguration.OpenLdapUserDNFromUsername, username, _ldapConfiguration.UserDN);
        }
    }
}

