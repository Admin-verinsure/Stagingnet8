using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;

namespace DealEngine.Services.Impl
{
    public class ChangeProcessService : IChangeProcessService
    {
        IMapperSession<ChangeReason> _changeReasonRepository;

        public ChangeProcessService(
            IMapperSession<ChangeReason> changeReasonRepository
            )
        {
            _changeReasonRepository = changeReasonRepository;
        }

        public async Task<ChangeReason> Add(ChangeReason changeReason)
        {
            await _changeReasonRepository.AddAsync(changeReason);
            return changeReason;
        }
    }
}
