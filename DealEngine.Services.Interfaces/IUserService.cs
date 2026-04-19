using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUser (string username);
        Task<User> GetUserById (Guid userId);
        Task<User> GetUserByEmail (string email);
        Task<User> GetUserByUserName(string userName);
        Task<User> GetUserByOktaUID(string uid);
        Task<List<User>> GetAllUsers ();
        Task ApplicationCreateUser(User user);
        Task Create(User user);
        Task Update(User user);
        Task<List<User>> GetLockedUsers();
        Task<User> GetUserPrimaryOrganisationOrEmail(Organisation org);
        Task<List<User>> GetAllUserByOrganisation(Organisation org);
        Task<List<User>> GetBrokerUsers();
        Task<User> PostCreateUser(User jsonUser, User currentUser, IFormCollection form,Organisation org);
        Task<User> GetApplicationUserByEmail(string email);
        Task<User> GetMarshUser(string okta_uid);
        Task<User> GetUserByFirstName(string firstName);
        Task<IList<User>> GetUsersByLastName(string lastName);
        Task<bool> CreateUserPrimaryOrgOrgTypeAndLDAP(User user, string userType);
        Task<IList<User>> GetUsersByPrimaryOrganisationId(Guid organisationId);
        Task<List<string>> GetAllUserforOrganisation(Organisation org);
        Task<List<User>> GetUsersByOrganisation(Organisation org);
        Task RemoveAllUsersFromOrganisation(Guid organisationId);
    }
}
