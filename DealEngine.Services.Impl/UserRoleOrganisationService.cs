using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DealEngine.Services.Impl
{
    public class UserRoleOrganisationService : IUserRoleOrganisationService
    {
        private readonly IMapperSession<UserRoleOrganisation> _userRoleOrgRepository;
        private readonly ILogger<UserRoleOrganisationService> _logger;
        private readonly IUserService _userService;
        private readonly IOrganisationService _organisationService;       

        public UserRoleOrganisationService(
            IMapperSession<UserRoleOrganisation> userRoleOrgRepository,
            ILogger<UserRoleOrganisationService> logger,
            IUserService userService,
            IOrganisationService organisationService
            )
        {
            _userRoleOrgRepository = userRoleOrgRepository;
            _logger = logger;
            _userService = userService;
            _organisationService = organisationService;
        }

        public async Task<UserRoleOrganisation> AddUserRoleOrganisationAsync(UserRoleOrganisation userRoleOrganisation)
        {
            try
            {
                await _userRoleOrgRepository.AddAsync(userRoleOrganisation);
                return userRoleOrganisation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding or updating UserRoleOrganisation");
                throw;
            }
        }
    }
}
