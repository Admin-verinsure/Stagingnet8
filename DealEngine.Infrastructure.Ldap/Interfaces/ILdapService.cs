using Bismuth.Ldap;
using System;
using DealEngine.Domain.Entities;

namespace DealEngine.Infrastructure.Ldap.Interfaces
{
	public interface ILdapService
	{
		User GetUser (string username);
		User GetUser (Guid userId);
		User GetUserByEmail (string email);
		User GetUserByEmailforupload(string email);
		void Validate (string username, string password, out int resultCode, out string resultMessage);
		Organisation GetOrganisation (string organisationName);
		Organisation GetOrganisation (Guid organisationId);
		void SetPasswordPolicyFor (User user, string passwordPolicyName);
		void Create (User user);
        void CreateWithPassword(User user, string password);
        void Create (Organisation organisation);
		void Update (User user);
		void Update (Organisation organisation);
		void GlobalBan (User user);
		void RemoveGlobalBan (User user);
        bool ChangePassword(string username, string oldPassword, string newPassword);
        string GetUsernameDN(string username);
    }
}

