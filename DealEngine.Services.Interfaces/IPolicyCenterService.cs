using DealEngine.Domain.Entities;
using System.Threading.Tasks;

namespace DealEngine.Services.Interfaces
{
    public interface IPolicyCenterService
    {
        Task<bool> GetAccount(Organisation organisation);
    }
}
