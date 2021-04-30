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
        Task<List<User>> GetAllUsers ();
        Task ApplicationCreateUser(User user);
        Task Create(User user);
        Task Update(User user);
        Task<List<User>> GetLockedUsers();
        Task<User> GetUserPrimaryOrganisationOrEmail(Organisation org);
        Task<List<User>> GetAllUserByOrganisation(Organisation org);
        Task<List<User>> GetBrokerUsers();
        Task<User> PostCreateUser(User jsonUser, User currentUser, IFormCollection form);
        Task<User> GetApplicationUserByEmail(string email);

    }
}
