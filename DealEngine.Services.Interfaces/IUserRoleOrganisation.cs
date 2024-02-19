using DealEngine.Domain.Entities;
using System.Threading.Tasks;

namespace DealEngine.Services.Interfaces
{
    public interface IUserRoleOrganisationService
    {
        Task<UserRoleOrganisation> AddUserRoleOrganisationAsync(UserRoleOrganisation userRoleOrganisation);
    }
}
