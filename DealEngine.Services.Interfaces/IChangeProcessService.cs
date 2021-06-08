using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
	public interface IChangeProcessService
    {
        Task<ChangeReason> Add(ChangeReason changeReason);

    }
}

