using System;
using System.Configuration;
using System.DirectoryServices;
using System.Globalization;


namespace DealEngine.Infrastructure.BaseLdap.Providers
{
    public class OpenLdapMembershipProvider //: MembershipProvider
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

		protected TimeSpan ClientTimeout {
			get;
			set;
		}

		public OpenLdapMembershipProvider ()
		{
			ClientTimeout = new TimeSpan (0, 0, 10);
		}

        //#region overridden Properties

        //public override string ApplicationName
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }

        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override bool EnablePasswordReset
        //{
        //    get
        //    {
        //        // We are using password reset requests, so this of course will be true
        //        return true;
        //    }
        //}

        //public override bool EnablePasswordRetrieval
        //{
        //    get
        //    {
        //        // OpenLdap salts and hashes the passwords, so we can't retrieve them
        //        return false;
        //    }
        //}

        //public override int MaxInvalidPasswordAttempts
        //{
        //    get
        //    {
        //        DirectoryEntry entry = GetPasswordPolicy();
        //        if (entry != null)
        //        {
        //            object value = entry.Properties["pwdMaxFailure"].Value;
        //            if (value != null)
        //                return Convert.ToInt32(value);
        //        }
        //        return 0;
        //    }
        //}

        //public override int MinRequiredNonAlphanumericCharacters
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override int MinRequiredPasswordLength
        //{
        //    get
        //    {
        //        DirectoryEntry entry = GetPasswordPolicy();
        //        if (entry != null)
        //        {
        //            object value = entry.Properties["pwdMinLength"].Value;
        //            if (value != null)
        //                return Convert.ToInt32(value);
        //        }
        //        return 6;
        //    }
        //}

        //public override int PasswordAttemptWindow
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override MembershipPasswordFormat PasswordFormat
        //{
        //    get
        //    {
        //        return MembershipPasswordFormat.Hashed;
        //    }
        //}

        //public override string PasswordStrengthRegularExpression
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override bool RequiresQuestionAndAnswer
        //{
        //    get
        //    {
        //        // OpenLdap currently doesn't have a module for this, and we're using reset requests anyway
        //        return false;
        //    }
        //}

        //public override bool RequiresUniqueEmail
        //{
        //    get
        //    {
        //        // We use password reset requests, so we'll always need unique email addresses
        //        return true;
        //    }
        //}

        //#endregion

  //      #region overridden Methods

  //      public override bool ChangePassword(string username, string oldPassword, string newPassword)
  //      {
  //          string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"];
  //          string baseUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBaseUserDN"], baseDN);

  //          bool result = false;
  //          try
  //          {
  //              string userDN = string.Format(ConfigurationManager.AppSettings["OpenLdapUserDNFromUsername"], username, baseUserDN);

  //              DirectoryEntry connection = (string.IsNullOrWhiteSpace(oldPassword)) ? GetLdapConnection() : GetLdapConnection(userDN, oldPassword);
  //              DirectorySearcher searcher = new DirectorySearcher(connection);

  //              searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
  //              searcher.Filter = "uid=" + username;
  //              searcher.ClientTimeout = ClientTimeout;

  //              searcher.PropertiesToLoad.AddRange(new string[3] { "userpassword", "+", "*" });

  //              SearchResult findResult = searcher.FindOne();

  //              DirectoryEntry entry = findResult.GetDirectoryEntry();
  //              if (entry != null)
  //              {
  //                  entry.Properties["userpassword"].Value = newPassword;
  //                  entry.CommitChanges();
  //                  result = true;
  //              }
  //          }
  //          catch (Novell.Directory.Ldap.LdapException ex)
  //          {
  //              throw new Exception(ex.LdapErrorMessage);
  //          }
  //          catch (Exception ex)
  //          {
  //              throw new Exception(string.Format("Unable to change the password for {0}.", username), ex);
  //          }

  //          return result;
  //      }

  //      public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
  //      {
  //          throw new NotSupportedException("OpenLdap does not support password recovery questions and answers.");
  //      }

  //      public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
  //      {
  //          string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"];
  //          string baseUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBaseUserDN"], baseDN);

  //          Guid userId;

  //          // validate parameters
  //          if (string.IsNullOrWhiteSpace(username))
  //          {
  //              status = MembershipCreateStatus.InvalidUserName;
  //              return null;
  //          }
  //          if (string.IsNullOrWhiteSpace(password))
  //          {
  //              status = MembershipCreateStatus.InvalidPassword;
  //              return null;
  //          }
  //          if (string.IsNullOrWhiteSpace(email))
  //          {
  //              status = MembershipCreateStatus.InvalidEmail;
  //              return null;
  //          }
  //          if (providerUserKey == null || Guid.TryParse(providerUserKey.ToString(), out userId))
  //          {
  //              status = MembershipCreateStatus.InvalidProviderUserKey;
  //              return null;
  //          }

  //          // attempt to create the user
  //          try
  //          {
  //              DirectoryEntry connection = GetLdapConnection();
  //              DirectoryEntry entry = connection.Children.Add("uid=" + username, "user");

  //              entry.Properties["employeeNumber"].Value = userId;
  //              entry.Properties["mail"].Value = email;
  //              entry.Properties["uid"].Value = username;
  //              entry.Properties["userpassword"].Value = password;

  //              entry.CommitChanges();

  //              status = MembershipCreateStatus.Success;
  //          }
  //          catch (Exception ex)
  //          {
  //              status = MembershipCreateStatus.ProviderError;
  //              throw new MembershipCreateUserException("Unable to create OpenLdap user.", ex);
  //          }

  //          return GetUser(username, false);
  //      }

  //      public override bool DeleteUser(string username, bool deleteAllRelatedData)
  //      {
  //          throw new NotSupportedException("OpenLdap does not support user deletion.");
  //      }

  //      public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override int GetNumberOfUsersOnline()
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override string GetPassword(string username, string answer)
  //      {
  //          throw new NotSupportedException("OpenLdap does not support retrieval of unhashed passwords.");
  //      }

  //      public override MembershipUser GetUser(string username, bool userIsOnline)
  //      {
  //          if (string.IsNullOrWhiteSpace(username))
  //              throw new ArgumentException("Parameter 'attribute' cannot be empty or null");

  //          return SearchForUser("uid", username);
  //      }

  //      public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
  //      {
  //          if (providerUserKey == null)
  //              throw new ArgumentNullException("providerUserKey");
  //          if (providerUserKey.GetType() != typeof(Guid))
  //              throw new ArgumentException("Value cannot be an empty Guid.", "providerUserKey");

  //          return SearchForUser("entryuuid", providerUserKey);
  //      }

  //      public override string GetUserNameByEmail(string email)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override string ResetPassword(string username, string answer)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override bool UnlockUser(string userName)
  //      {
  //          throw new NotImplementedException();
  //      }

  //      public override void UpdateUser(MembershipUser user)
  //      {
  //          string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"];
  //          string baseUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBaseUserDN"], baseDN);

  //          if (user == null)
  //              throw new ArgumentNullException("user");

  //          try
  //          {
  //              DirectoryEntry connection = GetLdapConnection();
  //              DirectorySearcher searcher = new DirectorySearcher(connection);

  //              searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
  //              searcher.Filter = "uid=" + user.UserName;
  //              searcher.ClientTimeout = ClientTimeout;

  //              searcher.PropertiesToLoad.AddRange(new string[2] { "+", "*" });

  //              SearchResult findResult = searcher.FindOne();

  //              DirectoryEntry entry = findResult.GetDirectoryEntry();
  //              if (entry != null)
  //              {
  //                  //					entry.Properties["cn"].Value = "";
  //                  entry.Properties["description"].Value = user.Comment;
  //                  entry.Properties["mail"].Value = user.Email;
  //                  //					entry.Properties["givenname"].Value = "";
  //                  //					entry.Properties["sn"].Value = "";
  //                  entry.Properties["uid"].Value = user.UserName;
  //                  //					entry.Properties["userpassword"].Value = "";
  //                  //					entry.Properties["userpassword"].Value = "";
  //                  //					entry.Properties["userpassword"].Value = "";
  //                  entry.CommitChanges();
  //              }
  //          }
  //          catch (Exception ex)
  //          {
  //              throw new Exception(string.Format("Unable to update user details for {0}.", user.UserName), ex);
  //          }
  //      }

  //      public override bool ValidateUser(string username, string password)
  //      {
  //          // Find the person in the directory to determine their distinct name
  //          try
  //          {
  //              DirectoryEntry root = new DirectoryEntry(ServerAddress + "/" + UserDN, null, null, AuthenticationTypes.None);
  //              DirectorySearcher searcher = new DirectorySearcher(root);

  //              searcher.SearchScope = SearchScope.Subtree;
  //              searcher.Filter = "uid=" + username;
  //              searcher.ClientTimeout = ClientTimeout;

  //              SearchResult findResult = searcher.FindOne();

  //              string distinctName = string.Empty;

  //              // Inverse the ou order found in LDAP to build distinct name
  //              if (findResult.Properties["ou"] != null)
  //              {
  //                  distinctName = "uid=" + username;

  //                  for (int i = findResult.Properties["ou"].Count - 1; i >= 0; i--)
  //                  {
  //                      distinctName += ",ou=" + findResult.Properties["ou"][i];
  //                  }

  //                  distinctName += ",";
  //                  distinctName += UserDN;
  //              }
  //              else
  //              {
  //                  distinctName = string.Format(ConfigurationManager.AppSettings["OpenLdapUserDNFromUsername"], username, UserDN);
  //              }

  //              // Find the person as Employee
  //              DirectoryEntry root2 = new DirectoryEntry(ServerAddress + "/" + UserDN, distinctName, password, AuthenticationTypes.ServerBind);

  //              DirectorySearcher searcher2 = new DirectorySearcher(root2);
  //              searcher2.SearchScope = SearchScope.Subtree;
  //              searcher2.Filter = "uid=" + username;

  //              try
  //              {
  //                  SearchResult resultEmployee = searcher2.FindOne();
  //                  if (resultEmployee.Properties["uid"].Count == 1) { return true; } else { return false; }
  //              }
  //              catch (Exception ex)
  //              {
  //                  throw new Exception(string.Format("ValidateUser : {0} : uid {1} Found; Credentials failed", ex.Source, username), ex);
  //                  // Implement Logging Service here
  //                  //EventLog eventLog = new EventLog();    
  //                  //ThreadStart starter = delegate { eventLog.WriteEntry(this.ToString(), String.Format("ValidateUser : {0} : uid {1} Found; Credentials failed", ex.Source, username), System.Diagnostics.EventLogEntryType.Warning); };
  //                  //new Thread(starter).Start();
  //                  //return false;
  //              }
  //          }
  //          catch (NullReferenceException ex)
  //          {
  //              throw new Exception(string.Format("ValidateUser : {0} : uid {1} NOT found", ex.Source, username), ex);
  //          }
  //          catch (Exception ex)
  //          {
  //              throw new Exception(string.Format("ValidateUser : {0} : uid {1}", ex.Source, username), ex);
  //              //                if (ex.Message == "Object reference not set to an instance of an object.")
  //              //                {
  //              //                    // Implement Logging Service here
  //              //                    //EventLog eventLog = new EventLog();
  //              //                    //ThreadStart starter = delegate { eventLog.WriteEntry(this.ToString(), String.Format("ValidateUser : {0} : uid {1} NOT found", ex.Source, username), System.Diagnostics.EventLogEntryType.Warning); };
  //              //                    //new Thread(starter).Start();
  //              //                }
  //              //                else
  //              //                {
  //              //                    // Implement Logging Service here
  //              //                    //EventLog eventLog = new EventLog();
  //              //                    //ThreadStart starter = delegate { eventLog.WriteEntry(this.ToString(), String.Format("ValidateUser : {0} : {1}", ex.Source, ex.Message), System.Diagnostics.EventLogEntryType.Error); };
  //              //                    //new Thread(starter).Start();
  //              //                }
  //              //                return false;
  //          }
  //      }

  //      #endregion

  //      MembershipUser SearchForUser(string attribute, object value)
		//{
		//	string serverPort = ConfigurationManager.AppSettings["OpenLdapPort"];
		//	string serverAddress = "LDAP://" + ConfigurationManager.AppSettings["OpenLdapServer"] + ":" + serverPort;
		//	string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"];
		//	string baseUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBaseUserDN"], baseDN);

		//	MembershipUser user = null;

		//	try
		//	{
		//		DirectoryEntry root = new DirectoryEntry(serverAddress + "/" + baseUserDN, null, null, AuthenticationTypes.None);
		//		DirectorySearcher searcher = new DirectorySearcher(root);

		//		searcher.SearchScope = System.DirectoryServices.SearchScope.Subtree;
		//		searcher.Filter = string.Format("{0}={1}", attribute, value.ToString());
		//		searcher.ClientTimeout = ClientTimeout;

		//		searcher.PropertiesToLoad.AddRange(new string[3] { "userpassword", "+", "*"});

		//		SearchResult findResult = searcher.FindOne();

		//		DirectoryEntry entry = findResult.GetDirectoryEntry();
		//		if (entry != null)
		//		{
		//			foreach (DictionaryEntry de in findResult.Properties)
		//			{
		//				System.Collections.Generic.List<string> l = new System.Collections.Generic.List<string>();
		//				foreach (object o in ((ResultPropertyValueCollection)de.Value))
		//					l.Add(o.ToString());

		//				string s = string.Join(", ", l);
		//				//Console.WriteLine(de.Key + ": " + s);
		//			}

		//			// inline if statements
		//			DateTime lastLockoutTime = (findResult.Properties["pwdaccountlockedtime"] != null) ?
		//				GetUTCTime(findResult.Properties["pwdaccountlockedtime"][0].ToString()) : DateTime.MinValue;
		//			DateTime passwordChangeTime = (findResult.Properties["pwdchangedtime"] != null) ?
		//				GetUTCTime(findResult.Properties["pwdchangedtime"][0].ToString()) : DateTime.MinValue;

		//			user = new MembershipUser(
		//				this.GetType().Name,
		//				findResult.Properties["uid"][0].ToString(),
		//				new Guid(findResult.Properties["employeeNumber"][0].ToString()),
		//				entry.Properties["mail"].Value.ToString(),
		//				"",
		//				"",
		//				true,
		//				lastLockoutTime != DateTime.MinValue,
		//				GetUTCTime(findResult.Properties["createtimestamp"][0].ToString()),
		//				DateTime.MinValue,
		//				DateTime.MinValue,	// think entrycsn might contain this value, need to test more
		//				passwordChangeTime,
		//				lastLockoutTime
		//			);
		//		}
		//		return user;
		//	}
		//	catch (Exception ex)
		//	{
		//		throw new Exception (string.Format("Unable to find User with {0}={1}", attribute, value), ex);
		//	}

		//}

		DirectoryEntry GetLdapConnection()
		{
			//string baseDN = ConfigurationManager.AppSettings["OpenLdapBaseDN"];
			//string adminUserDN = string.Format(ConfigurationManager.AppSettings["OpenLdapBindDN"], baseDN);
			string adminUserDN = string.Format(ConfigurationManager.AppSettings["ApacheLdapBindDN"]);
			string adminUserPassword = ConfigurationManager.AppSettings["ApacheLdapBindPW"]; // OpenLdapBindPW

			return GetLdapConnection (adminUserDN, adminUserPassword);
		}

		DirectoryEntry GetLdapConnection(string ldapUser, string password)
		{
			string serverPort = ConfigurationManager.AppSettings["ApacheLdapPort"]; // OpenLdapPort
			string serverAddress = "LDAP://" + ConfigurationManager.AppSettings["ApacheLdapServer"] + ":" + serverPort; // OpenLdapServer
			string baseDN = ConfigurationManager.AppSettings["ApacheLdapBaseDN"]; // OpenLdapBaseDN

			return new DirectoryEntry(serverAddress + "/" + baseDN, ldapUser, password, AuthenticationTypes.Secure);
		}

		DateTime GetUTCTime(string time)
		{
			return GetUTCTime (time, "yyyyMMddHHmmssK");
		}

		DateTime GetUTCTime(string time, string format)
		{
			return DateTime.ParseExact (time, format, null, DateTimeStyles.AssumeUniversal).ToUniversalTime ();
		}

		DirectoryEntry GetPasswordPolicy()
		{
			DirectoryEntry connection = GetLdapConnection ();
			DirectorySearcher searcher = new DirectorySearcher(connection);
			searcher.Filter = "ou=policies,cn=default";
			searcher.PropertiesToLoad.Add("pwdMaxFailure");
			searcher.ClientTimeout = ClientTimeout;

			SearchResult findResult = searcher.FindOne();
			if (findResult != null) {
				return findResult.GetDirectoryEntry ();
			}
			return null;
		}
    }
}
