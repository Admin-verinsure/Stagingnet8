using Bismuth.Ldap;
using Bismuth.Ldap.Requests;
using Bismuth.Ldap.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Configuration;

using DealEngine.Domain.Exceptions;

namespace DealEngine.Infrastructure.Ldap.Providers
{
	public class BismuthLdapMembershipProvider //: MembershipProvider
	{
		protected string ServerAddress {
			get {
				return ConfigurationManager.AppSettings ["OpenLdapServer"];
			}
		}

		protected int ServerPort {
			get {
				return Convert.ToInt32 (ConfigurationManager.AppSettings ["OpenLdapPort"]);
			}
		}

		protected string UserDN {
			get {
				string baseDN = ConfigurationManager.AppSettings ["OpenLdapBaseDN"];
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseUserDN"], baseDN);
			}
		}
		
		//#region overridden Properties

		//public override string ApplicationName {
		//	get {
		//		throw new NotImplementedException ();
		//	}

		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override bool EnablePasswordReset {
		//	get {
		//		// We are using password reset requests, so this of course will be true
		//		return true;
		//	}
		//}

		//public override bool EnablePasswordRetrieval {
		//	get {
		//		// OpenLdap salts and hashes the passwords, so we can't retrieve them
		//		return false;
		//	}
		//}

		//public override int MaxInvalidPasswordAttempts {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override int MinRequiredNonAlphanumericCharacters {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override int MinRequiredPasswordLength {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override int PasswordAttemptWindow {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override MembershipPasswordFormat PasswordFormat {
		//	get {
		//		return MembershipPasswordFormat.Hashed;
		//	}
		//}

		//public override string PasswordStrengthRegularExpression {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//public override bool RequiresQuestionAndAnswer {
		//	get {
		//		// OpenLdap currently doesn't have a module for this, and we're using reset requests anyway
		//		return false;
		//	}
		//}

		//public override bool RequiresUniqueEmail {
		//	get {
		//		// We use password reset requests, so we'll always need unique email addresses
		//		return true;
		//	}
		//}

		//#endregion

		//#region overridden Methods

		//public override bool ChangePassword (string username, string oldPassword, string newPassword)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof (username));
		//	if (string.IsNullOrWhiteSpace (newPassword))
		//		throw new ArgumentNullException (nameof (newPassword));

		//	bool result = false;

		//	string userDN = GetUsernameDN (username);

		//	string bindDN = userDN;
		//	string bindPw = oldPassword;
		//	Console.WriteLine (username + " " + oldPassword + " " + newPassword);
		//	if (string.IsNullOrWhiteSpace (oldPassword)) {
		//		string baseDN = ConfigurationManager.AppSettings ["OpenLdapBaseDN"];
		//		bindDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapBindDN"], baseDN);
		//		bindPw = ConfigurationManager.AppSettings ["OpenLdapBindPW"];
		//	}

		//	using (LdapClient ldapClient = GetLdapConnection (bindDN, bindPw)) {
		//		var modifyRequest = new ModifyRequest (ldapClient.NextMessageId) {
		//			EntryName = userDN,
		//			Attributes = new List<ModifyAttribute> {
		//				new ModifyAttribute("userpassword", ModificationType.Replace, newPassword)
		//			}
		//		};
		//		var response = ldapClient.Send <ModifyResponse> (modifyRequest);
		//		if (response.ResultCode == 0)
		//			return true;

		//		// error otherwise
		//		throw new AuthenticationException (response.ErrorMessage) { ErrorCode = response.ResultCode, User = username };
		//	}
		//}

		//public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support password recovery questions and answers");
		//}

		//public override MembershipUser CreateUser (string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		//{
		//	Guid userId;
		//	status = MembershipCreateStatus.ProviderError;

		//	// validate parameters
		//	if (string.IsNullOrWhiteSpace (username)) {
		//		status = MembershipCreateStatus.InvalidUserName;
		//		return null;
		//	}
		//	if (string.IsNullOrWhiteSpace (password)) {
		//		status = MembershipCreateStatus.InvalidPassword;
		//		return null;
		//	}
		//	if (string.IsNullOrWhiteSpace (email)) {
		//		status = MembershipCreateStatus.InvalidEmail;
		//		return null;
		//	}
		//	if (providerUserKey == null || Guid.TryParse (providerUserKey.ToString (), out userId)) {
		//		status = MembershipCreateStatus.InvalidProviderUserKey;
		//		return null;
		//	}

		//	string userDN = GetUsernameDN (username);

		//	using (LdapClient ldapClient = GetLdapConnection ()) {
		//		var addRequest = new AddRequest (ldapClient.NextMessageId) {
		//			EntryName = userDN,
		//			Attributes = new List<ObjectAttribute> {
		//				new ObjectAttribute ("employeeNumber", userId.ToString()),
		//				new ObjectAttribute ("mail", email),
		//				new ObjectAttribute ("uid", username),
		//				new ObjectAttribute ("userpassword", password)
		//			}
		//		};
		//		var response = ldapClient.Send <AddResponse> (addRequest);
		//		if (response.ResultCode == 0)
		//			status = MembershipCreateStatus.Success;
		//	}
		//	return GetUser (username, false);
		//}

		//public override bool DeleteUser (string username, bool deleteAllRelatedData)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support user deletion");
		//}

		//public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override MembershipUserCollection FindUsersByName (string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override int GetNumberOfUsersOnline ()
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string GetPassword (string username, string answer)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support retrieval of unhashed passwords");
		//}

		//public override MembershipUser GetUser (string username, bool userIsOnline)
		//{
		//	MembershipUser user = null;
		//	using (LdapClient ldapClient = GetLdapConnection ()) {
		//		var searchRequest = new SearchRequest (ldapClient.NextMessageId) {
		//			BaseObject = UserDN,
		//			Scope = SearchScope.SingleLevel,
		//			SearchFilter = "(uid=" + username + ")",
		//			Attributes = new string [] { "+", "*" },
		//		};
		//		SearchResponse searchResponse = ldapClient.Send <SearchResponse> (searchRequest);
		//		if (searchResponse.Results.Count > 0) {
		//			SearchResult result = searchResponse.Results [0];
		//			MembershipUserBuilder userBuilder = new MembershipUserBuilder (GetType ().Name);
		//			userBuilder.Username (username);
		//			userBuilder.Email (result.Attributes ["mail"].Value);
		//			userBuilder.ProviderUserKey (new Guid (result.Attributes ["employeeNumber"].Value));
		//			userBuilder.DateCreated (GetUTCTime (result.Attributes ["createTimestamp"].Value));
		//			userBuilder.LastPasswordChangeDate (GetUTCTimeOrDefault(result.GetAttributeValue("pwdChangedTime")));
		//			userBuilder.LastLockoutDate (GetUTCTimeOrDefault (result.GetAttributeValue ("pwdAccountLockedTime")));

		//			user = userBuilder.GetUser ();
		//		}
		//	}
		//	return user;
		//}

		//public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string GetUserNameByEmail (string email)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string ResetPassword (string username, string answer)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool UnlockUser (string userName)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override void UpdateUser (MembershipUser user)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool ValidateUser (string username, string password)
		//{
		//	if (string.IsNullOrWhiteSpace (username))
		//		throw new ArgumentNullException (nameof (username));
		//	if (string.IsNullOrWhiteSpace (password))
		//		throw new ArgumentNullException (nameof (password));

		//	string userDN = GetUsernameDN (username);

		//	try {
		//		using (LdapClient ldapClient = GetLdapConnection (userDN, password)) {
		//			ldapClient.Unbind ();
		//		}
		//	}
		//	catch (Exception ex) {
		//		// bind error
		//		throw new AuthenticationException ("Unable to authenticate user") { ErrorCode = 49, User = username };
		//	}

		//	return true;
		//}

		//#endregion


		LdapClient GetLdapConnection ()
		{
			string baseDN = ConfigurationManager.AppSettings ["OpenLdapBaseDN"];
			string adminUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBindDN"]);//, baseDN);
			string adminUserPassword = ConfigurationManager.AppSettings ["OpenLdapBindPW"];

			return GetLdapConnection (adminUserDN, adminUserPassword);
		}

		LdapClient GetLdapConnection (string ldapUser, string password)
		{
			LdapClient ldapClient = new LdapClient (ServerAddress, ServerPort);
			ldapClient.Bind (ldapUser, password, BindAuthentication.Simple);

			return ldapClient;
		}

		DateTime GetUTCTime (string time)
		{
			return GetUTCTime (time, "yyyyMMddHHmmssK");
		}

		DateTime GetUTCTime (string time, string format)
		{
			return DateTime.ParseExact (time, format, null, DateTimeStyles.AssumeUniversal).ToUniversalTime ();
		}

		DateTime GetUTCTimeOrDefault (string dateTime)
		{
			if (!string.IsNullOrWhiteSpace (dateTime))
				return GetUTCTime (dateTime);
			return DateTime.MinValue;
		}

		DateTime GetUTCTimeOrDefault (ObjectAttribute attribute)
		{
			if (attribute != null)
				return GetUTCTimeOrDefault (attribute.Value);
			return DateTime.MinValue;
		}

		string GetUsernameDN (string username)
		{
			return string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], username, UserDN);
		}
	}
}

