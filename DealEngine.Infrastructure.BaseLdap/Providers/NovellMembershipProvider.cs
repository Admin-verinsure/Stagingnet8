using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Novell.Directory.Ldap;

namespace DealEngine.Infrastructure.BaseLdap.Providers
{
	public class NovellMembershipProvider //: MembershipProvider
	{
		protected string ServerAddress {
			get {
				return ConfigurationManager.AppSettings ["OpenLdapServer"]; // ApacheLdapServer
			}
		}

		protected int ServerPort {
			get {
				return Convert.ToInt32 (ConfigurationManager.AppSettings ["OpenLdapPort"]); // ApacheLdapPort
			}
		}

		protected string BaseDN {
			get {
				return ConfigurationManager.AppSettings ["OpenLdapBaseDN"]; // ApacheLdapBaseDN
			}
		}

		protected string BaseUserDN {
			get {
				return string.Format (ConfigurationManager.AppSettings ["OpenLdapBaseUserDN"], BaseDN);
			}
		}

		public NovellMembershipProvider ()
		{
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

		//public override bool ChangePassword (string name, string oldPwd, string newPwd)
		//{
		//	if (string.IsNullOrWhiteSpace (name))
		//		throw new ArgumentNullException (nameof (name));
		//	if (string.IsNullOrWhiteSpace (oldPwd))
		//		throw new ArgumentNullException (nameof (oldPwd));
		//	if (string.IsNullOrWhiteSpace (newPwd))
		//		throw new ArgumentNullException (nameof (newPwd));

		//	string userDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], name, BaseUserDN);

		//	bool result = false;
		//	LdapConnection conn = null;
		//	try {
		//		// change password via reset function
		//		if (string.IsNullOrWhiteSpace (oldPwd))
		//			conn = GetConnection ();
		//		// change password through account panel, so need to authenticate
		//		else
		//			conn = GetConnection (userDN, oldPwd);
		//		if (conn != null) {
		//			LdapEntry entry = conn.Read (userDN);
		//			if (entry != null) {
		//				conn.Modify (userDN, new LdapModification (LdapModification.REPLACE, new LdapAttribute ("userPassword", newPwd)));
		//				entry = conn.Read (userDN);
		//				// if entry exists, has password and password == new password, true
		//				result = (entry != null && entry.getAttribute ("userPassword") != null && entry.getAttribute ("userPassword").StringValue == newPwd);
		//			}
		//		}
		//	}
		//	catch (LdapException ex) {
		//		throw new Exception (ex.LdapErrorMessage);
		//	}
		//	catch (Exception ex) {
		//		throw new Exception (string.Format ("Unable to change the password for {0}.", name), ex);
		//	}
		//	finally {
		//		if (conn != null && conn.Connected)
		//			conn.Disconnect ();
		//	}
		//	return result;
		//}

		//public override bool ChangePasswordQuestionAndAnswer (string name, string password, string newPwdQuestion, string newPwdAnswer)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support password recovery questions and answers");
		//}

		//public override MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		//{
		//	Guid userId;

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

		//	string userDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], username, BaseUserDN);

		//	LdapConnection conn = null;
		//	try {
		//		conn = GetConnection ();

		//		LdapAttributeSet attributes = new LdapAttributeSet ();
		//		attributes.Add (new LdapAttribute ("employeeNumber", userId.ToString()));
		//		attributes.Add (new LdapAttribute ("mail", email));
		//		attributes.Add (new LdapAttribute ("uid", username));
		//		attributes.Add (new LdapAttribute ("userpassword", password));

		//		LdapEntry newUserEntry = new LdapEntry (userDN, attributes);
		//		conn.Add (newUserEntry);

		//		status = MembershipCreateStatus.Success;
		//	}
		//	catch (Exception ex) {
		//		status = MembershipCreateStatus.ProviderError;
		//	}
		//	finally {
		//		if (conn != null && conn.Connected)
		//			conn.Disconnect ();
		//	}

		//	return GetUser (username, false);
		//}

		//public override bool DeleteUser (string name, bool deleteAllRelatedData)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support user deletion");
		//}

		//public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override MembershipUserCollection FindUsersByName (string nameToMatch, int pageIndex, int pageSize, out int totalRecords)
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

		//public override string GetPassword (string name, string answer)
		//{
		//	throw new NotSupportedException ("OpenLdap does not support retrieval of unhashed passwords");
		//}

		//public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override MembershipUser GetUser (string name, bool userIsOnline)
		//{
		//	if (string.IsNullOrWhiteSpace (name))
		//		throw new ArgumentNullException (nameof (name));

		//	string userDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], name, BaseUserDN);

		//	LdapConnection conn = null;
		//	MembershipUser user = null;
		//	try {
		//		conn = GetConnection ();
		//		LdapEntry userEntry = conn.Read (userDN);
		//		if (userEntry != null) {
		//			LdapAttribute lockTimeAttr = userEntry.getAttribute ("pwdAccountLockedTime");
		//			DateTime lastLockoutTime = (lockTimeAttr != null) ? GetUTCTime (lockTimeAttr.StringValue) : DateTime.MinValue;
		//			LdapAttribute passwordChangeTimeAttr = userEntry.getAttribute ("pwdChangedTime");
		//			DateTime lastPasswordChangeTime = (lockTimeAttr != null) ? GetUTCTime (passwordChangeTimeAttr.StringValue) : DateTime.MinValue;

		//			//user = new MembershipUser (
		//			//	GetType ().Name,
		//			//	name,
		//			//	new Guid (userEntry.getAttribute ("employeeNumber").StringValue),
		//			//	userEntry.getAttribute ("mail").StringValue,
		//			//	"",
		//			//	"",
		//			//	true,
		//			//	lastLockoutTime != DateTime.MinValue,
		//			//	GetUTCTime (userEntry.getAttribute ("createtimestamp").StringValue),
		//			//	DateTime.MinValue,
		//			//	DateTime.MinValue,  // think entrycsn might contain this value, need to test more
		//			//	lastPasswordChangeTime,
		//			//	lastLockoutTime
		//			//);
		//			MembershipUserBuilder userBuilder = new MembershipUserBuilder (GetType ().Name);
		//			userBuilder.Username (name);
		//			userBuilder.Email (userEntry.getAttribute ("mail").StringValue);
		//			userBuilder.ProviderUserKey (new Guid (userEntry.getAttribute ("employeeNumber").StringValue));
		//			userBuilder.DateCreated (GetUTCTime (userEntry.getAttribute ("createTimestamp").StringValue));
		//			userBuilder.LastPasswordChangeDate (lastPasswordChangeTime);
		//			userBuilder.LastLockoutDate (lastLockoutTime);

		//			user = userBuilder.GetUser ();
		//		}
		//	}
		//	catch (Exception ex) {
		//		throw new Exception (string.Format ("Unable to get user for {0}.", name), ex);
		//	}
		//	finally {
		//		if (conn != null && conn.Connected)
		//			conn.Disconnect ();
		//	}
		//	return user;
		//}

		//public override string GetUserNameByEmail (string email)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override string ResetPassword (string name, string answer)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override bool UnlockUser (string userName)
		//{
		//	throw new NotImplementedException ();
		//}

		//public override void UpdateUser (MembershipUser user)
		//{
		//	if (user == null)
		//		throw new ArgumentNullException (nameof (user));

		//	string userDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], user.UserName, BaseUserDN);

		//	LdapConnection conn = null;
		//	try {
		//		conn = GetConnection ();
		//		LdapEntry userEntry = conn.Read (userDN);
		//		if (userEntry != null) {
		//			LdapModification [] mods = new LdapModification [] {
		//				new LdapModification (LdapModification.REPLACE, new LdapAttribute ("description", user.Comment)),
		//				new LdapModification (LdapModification.REPLACE, new LdapAttribute ("mail", user.Email)),
		//				new LdapModification (LdapModification.REPLACE, new LdapAttribute ("uid", user.UserName))
		//			};
		//			conn.Modify (userDN, mods.ToArray ());
		//		}
		//	}
		//	catch (Exception ex) {
		//		throw new Exception (string.Format ("Unable to update user details for {0}.", user.UserName), ex);
		//	}
		//	finally {
		//		if (conn != null && conn.Connected)
		//			conn.Disconnect ();
		//	}
		//}

		//public override bool ValidateUser (string name, string password)
		//{
		//	if (string.IsNullOrWhiteSpace (name))
		//		throw new ArgumentNullException (nameof (name));
		//	if (string.IsNullOrWhiteSpace (password))
		//		throw new ArgumentNullException (nameof (password));

		//	string userDN = string.Format (ConfigurationManager.AppSettings ["OpenLdapUserDNFromUsername"], name, BaseUserDN);

		//	bool result = false;
		//	LdapConnection conn = null;
		//	try {
		//		conn = GetConnection (userDN, password);
		//		if (conn != null)
		//			result = true;
		//	}
		//	catch (NullReferenceException ex) {
		//		throw new Exception (string.Format ("ValidateUser : {0} : uid {1} NOT found", ex.Source, name), ex);
		//	}
		//	catch (Exception ex) {
		//		throw new Exception (string.Format ("ValidateUser : {0} : uid {1}", ex.Source, name), ex);
		//	}
		//	finally {
		//		if (conn != null && conn.Connected)
		//			conn.Disconnect ();
		//	}
		//	return result;
		//}

		//#endregion

		LdapConnection GetConnection ()
		{
			string adminUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBindDN"], BaseDN);
			//string adminUserDN = string.Format(ConfigurationManager.AppSettings["ApacheLdapBindDN"]);
			string adminUserPassword = ConfigurationManager.AppSettings ["OpenLdapBindPW"]; // ApacheLdapBindPW

			return GetConnection (adminUserDN, adminUserPassword);
		}

		LdapConnection GetConnection (string bindDN, string bindPassword)
		{
			LdapConnection connection = new LdapConnection ();
			connection.Connect (ServerAddress, ServerPort);

			connection.Bind (bindDN, bindPassword);

			if (connection.Bound)
				return connection;
			throw new Exception ("Unable to connect to ldap");
		}

		DateTime GetUTCTime (string time)
		{
			return GetUTCTime (time, "yyyyMMddHHmmssK");
		}

		DateTime GetUTCTime (string time, string format)
		{
			return DateTime.ParseExact (time, format, null, DateTimeStyles.AssumeUniversal).ToUniversalTime ();
		}
	}
}

