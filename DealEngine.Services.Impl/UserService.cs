using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Ldap.Interfaces;
using DealEngine.Services.Interfaces;
using NHibernate.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Novell.Directory.Ldap;
using NHibernate;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DealEngine.Services.Impl
{
	public class UserService : IUserService
	{
		IMapperSession<User> _userRepository;
		ILdapService _ldapService;
		ILegacyLdapService _legacyLdapService;
		IMapperSession<Organisation> _organisationRepository;
		ILogger<UserService> _logger;
		IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;


		public UserService(
			IMapper mapper,
			IMapperSession<User> userRepository,
			ILdapService ldapService,
			ILegacyLdapService legacyLdapService,
			IMapperSession<Organisation> organisationRepository,
			ILogger<UserService> logger,
            IUnitOfWork unitOfWork
            )
		{
			_mapper = mapper;
			_userRepository = userRepository;
			_ldapService = ldapService;
			_legacyLdapService = legacyLdapService;
			_organisationRepository = organisationRepository;
			_logger = logger;
            _unitOfWork = unitOfWork;
        }

		public async Task<User> GetUser(string username)
		{
			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.UserName == username);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
			// have a repo user? Return them
			if (user != null)
				return user;
			user = _ldapService.GetUser(username);
			// have a ldap user but no repo? Update NHibernate & return them
			if (user != null)
			{
				// postgres is case sensitive, while Ldap is case insensitive. So a valid lowercase username will fail the 1st condition, but get here and reimport the user, which isn't what we want to have happen
				// in this case, we'll get the ldap user, and only if the uppercase'd ldap username doesn't exist in postgres, we'll add the user.
				var localUser = _userRepository.FindAll().FirstOrDefault(u => u.UserName == user.UserName);
				if (localUser == null)
					await Update(user);
				return user;
			}
			//user = _legacyLdapService.GetLegacyUser(username);
			// have a legacy ldap user only? Create them in Ldap & NHibernate & return them
			if (user != null)
			{
				await Create(user);
				return user;
			}
			// no user at all? Throw exception
			throw new Exception("User with username '" + username + "' does not exist in the system");
		}

		public async Task<User> GetMarshUser(string OktaUID)
		{

			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.OktaUID == OktaUID);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
			return user;
			//// have a repo user? Return them
			//if (user != null)
			//	return user;
			//user = _ldapService.GetUser(username);
			//// have a ldap user but no repo? Update NHibernate & return them
			//if (user != null)
			//{
			//	// postgres is case sensitive, while Ldap is case insensitive. So a valid lowercase username will fail the 1st condition, but get here and reimport the user, which isn't what we want to have happen
			//	// in this case, we'll get the ldap user, and only if the uppercase'd ldap username doesn't exist in postgres, we'll add the user.
			//	var localUser = _userRepository.FindAll().FirstOrDefault(u => u.UserName == user.UserName);
			//	if (localUser == null)
			//		await Update(user);
			//	return user;
			//}
			////user = _legacyLdapService.GetLegacyUser(username);
			//// have a legacy ldap user only? Create them in Ldap & NHibernate & return them
			//if (user != null)
			//{
			//	await Create(user);
			//	return user;
			//}
			//// no user at all? Throw exception
			//throw new Exception("User with username '" + username + "' does not exist in the system");


		}

		public async Task<User> GetUserById(Guid userId)
		{
			User user = await _userRepository.GetByIdAsync(userId);
			// have a repo user? Return them
			if (user != null)
				return user;
			user = _ldapService.GetUser(userId);
			// have a ldap user but no repo? Update NHibernate & return them
			if (user != null)
			{
				await Update(user);
				return user;
			}
			//user = _legacyLdapService.GetLegacyUser(userId);
			// have a legacy ldap user only? Create them in Ldap & NHibernate & return them
			if (user != null)
			{
				await Create(user);
				return user;
			}
			throw new Exception("User with Id '" + userId + "' does not exist in the system");
		}

		public async Task<User> GetUserByEmail(string email)
		{
			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.Email == email);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			// have a repo user? Return them
			if (user != null)
				return user;
			user = _ldapService.GetUserByEmail(email);
			// have a ldap user but no repo? Update NHibernate & return them
			if (user != null)
			{
				await Update(user);
				return user;
			}
			//user = _legacyLdapService.GetLegacyUserByEmail (email);
			// have a legacy ldap user only? Create them in Ldap & NHibernate & return them
			if (user != null)
			{
				await Create(user);
			}
			return user;
		}
		public async Task<User> GetUserByFirstName(string firstName)
		{
			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.FirstName == firstName);

			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
			return user;
		}
		public async Task<IList<User>> GetUsersByLastName(string lastName)
		{
			try
			{
				var users = await _userRepository.FindAll().Where(u => u.LastName == lastName).ToListAsync();
				return users;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}
		public async Task<User> GetUserByUserName(string userName)
		{
			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.UserName == userName);

			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			// have a repo user? Return them
			if (user != null)
				return user;
			user = _ldapService.GetUser(userName);
			// have a ldap user but no repo? Update NHibernate & return them
			if (user != null)
			{
				await Update(user);
				return user;
			}
			//user = _legacyLdapService.GetLegacyUserByEmail (email);
			// have a legacy ldap user only? Create them in Ldap & NHibernate & return them
			if (user != null)
			{
				await Create(user);
			}
			return user;
		}

		public async Task<User> GetUserByOktaUID(string uid)
		{
			User user = null;
			try
			{
				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.OktaUID == uid);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return user;
		}

		public async Task<List<User>> GetAllUsers()
		{
			return await _userRepository.FindAll().ToListAsync();
		}


		public async Task ApplicationCreateUser(User user)
		{
			await CreateDefaultUserOrganisation(user);
			await _userRepository.AddAsync(user);
			//await Update(user);
		}

		public async Task Create(User user)
		{
			await CreateDefaultUserOrganisation(user);
			await _userRepository.AddAsync(user);
			try
			{
				_ldapService.Create(user);
			}
			catch (Exception ex)
			{
				await Update(user);
				_logger.LogWarning(ex.Message);
			}
			await Update(user);
		}

		public async Task Update(User user)
		{
			try
			{
				UpdateLDap(user);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex.Message);
			}
			finally
			{
				await _userRepository.UpdateAsync(user);
			}
		}

		public async Task Delete(User user, User authorizingUser)
		{
			user.Delete(authorizingUser, DateTime.UtcNow);
			await _userRepository.UpdateAsync(user);
		}

		public void SetPasswordPolicyFor(User user, string passwordPolicyName)
		{
			_ldapService.SetPasswordPolicyFor(user, passwordPolicyName);
		}

		public async Task IssueLocalBan(User user, User banningUser)
		{
			user.Lock();
			await Update(user);
		}

		public async Task RemoveLocalban(User user, User banningUser)
		{
			user.Unlock();
			await Update(user);
		}

		public bool IsUserLocalBanned(User user)
		{
			return user.Locked;
		}

		public void IssueGlobalBan(User user, User banningUser)
		{
			_ldapService.GlobalBan(user);
		}

		public void RemoveGlobalBan(User user, User banningUser)
		{
			_ldapService.RemoveGlobalBan(user);
		}

		public bool IsUserGlobalBanned(User user)
		{
			throw new NotImplementedException();
		}

		protected async Task CreateDefaultUserOrganisation(User user)
		{
			OrganisationType personalOrganisationType = new OrganisationType(user, "Default");
			Organisation defaultOrganisation = Organisation.CreateDefaultOrganisation(user, user, personalOrganisationType);
			user.Organisations.Add(defaultOrganisation);
		}

		protected async Task CreateBrokerUserOrganisation(User user)
		{
			OrganisationType brokerOrganisationType = new OrganisationType(user, "Broker");
			Organisation brokerOrganisation = Organisation.CreateBrokerOrganisation(user, user, brokerOrganisationType);
            brokerOrganisation.IsBroker = true;
			user.Organisations.Add(brokerOrganisation);
		}

		protected async Task CreateInsurerUserOrganisation(User user)
		{
			OrganisationType insurerOrganisationType = new OrganisationType(user, "Insurer");
			Organisation insurerOrganisation = Organisation.CreateInsurerOrganisation(user, user, insurerOrganisationType);
            insurerOrganisation.IsInsurer = true;
			user.Organisations.Add(insurerOrganisation);
		}

        protected async Task CreateAssociationUserOrganisation(User user)
        {
            OrganisationType associationOrganisationType = new OrganisationType(user, "Association");
            Organisation associationOrganisation = Organisation.CreateAssociationOrganisation(user, user, associationOrganisationType);
            associationOrganisation.IsAssociation = true;
            user.Organisations.Add(associationOrganisation);
        }

        public async Task<List<User>> GetLockedUsers()
		{
			return await _userRepository.FindAll().Where(u => u.Locked == true).ToListAsync();
		}

		public async Task<List<User>> GetBrokerUsers()
		{
			var users = await _userRepository.FindAll().Where(u => u.PrimaryOrganisation.IsBroker == true).ToListAsync();
			return users;
		}

		public async Task<User> GetUserPrimaryOrganisationOrEmail(Organisation org)
		{
			var user = await _userRepository.FindAll().OrderByDescending(u => u.DateCreated).FirstOrDefaultAsync(u => u.PrimaryOrganisation == org);

			try
			{
				if (user == null)
				{
					user = await GetUserByEmail(org.Email);
				}

			}
			catch (Exception ex)
			{

			}
			return user;

		}
		public async Task<List<User>> GetAllUserByOrganisation(Organisation org)
		{
			return _userRepository.FindAll().Where(u => u.PrimaryOrganisation == org).OrderBy(u => u.FullName).ToList();
		}

		private void UpdateLDap(User user)
		{
			_ldapService.Update(user);
		}

		public async Task<User> PostCreateUser(User jsonUser, User currentUser, IFormCollection form, Organisation orgselected)
		{
			var createUser = await GetUserByEmail(form["User.Email"]);
			if (createUser != null)
			{

				//createUser = new User(currentUser, Guid.NewGuid(), form);
				createUser = _mapper.Map(jsonUser, createUser);
				//var PrimaryOrganisation = await _organisationRepository.GetByIdAsync(OrganisationId);
				createUser.SetPrimaryOrganisation(orgselected);
				await Update(createUser);

			}
			else
			{
				createUser = new User(currentUser, Guid.NewGuid(), form);
				createUser = _mapper.Map(jsonUser, createUser);
				if (orgselected != null)
				{
					createUser.SetPrimaryOrganisation(orgselected);

				}
				await Create(createUser);
			}
			return createUser;
		}


		public async Task<User> GetApplicationUserByEmail(string email)
		{

			User user = null;
			try
			{
				//  user = await _userRepository.FindAll().FirstOrDefaultAsync(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

				user = await _userRepository.FindAll().FirstOrDefaultAsync(u => u.Email == email);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
			return user;
		}

		public async Task<bool> CreateUserPrimaryOrgOrgTypeAndLDAP(User user, string userType)
        {
			try
			{
                _logger.LogInformation($"Starting user creation for {user.Email} with userType {userType}.");

                // Set up user and organisation for DB
                SetUpUserAndOrganisation(user, userType);
                _logger.LogDebug($"User and primary organisation setup completed for {user.Email}.");

                // Ensure Correct PrimaryOrganisation is set
                var primaryOrg = user.Organisations
                    .FirstOrDefault(o => o.OrganisationType != null && o.OrganisationType.Name == userType);
                user.PrimaryOrganisation = primaryOrg;

                // Create LDAP User
                string ldapPassword = user.Password;
				user.Password = "modWG9qrVnKM7UW"; // Placeholder password for DB

				CreateLdapUser(user, ldapPassword);
                _logger.LogDebug($"LDAP user created for {user.Email}.");

				CreateLdapOrganisation(primaryOrg);
                _logger.LogDebug($"LDAP organisation created for {primaryOrg.Name}.");

				// Create User in the repository
				await _userRepository.AddAsync(user);
                _logger.LogInformation($"User creation completed successfully for {user.Email}.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"User creation failed: {ex.Message}");
				//throw new Exception($"User creation failed: {ex.Message}");
				throw;
            }
        }

        private bool CreateLdapUser(User user, string ldapPassword)
        {
            try
            {
                _ldapService.CreateWithPassword(user, ldapPassword);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"LDAP User creation failed: {ex.Message}.");
                throw;
            }
        }

        private bool CreateLdapOrganisation(Organisation organisation)
        {
            try
            {
                _ldapService.CreateNoFakeTimeout(organisation);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"LDAP Organisation creation failed: {ex.Message}.");
                throw;
            }
        }

        public async Task<IList<User>> GetUsersByPrimaryOrganisationId(Guid organisationId)
        {
            return await _userRepository
                .FindAll()
                .Where(u =>
                    u.PrimaryOrganisation != null &&
                    u.PrimaryOrganisation.Id == organisationId)
                .ToListAsync();
        }
        private void SetUpUserAndOrganisation(User user, string userType)
        {
            if (userType == "Broker")
            {
                CreateBrokerUserOrganisation(user);
            }
            else if (userType == "Insurer")
            {
                CreateInsurerUserOrganisation(user);
            }
            else if (userType == "Association")
            {
                CreateAssociationUserOrganisation(user);
            }
            else
            {
                CreateDefaultUserOrganisation(user);
            }
        }
    }
}