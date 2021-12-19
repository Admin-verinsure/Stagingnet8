using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using NHibernate.Linq;
using System;

namespace DealEngine.Services.Impl
{
    public class ClaimNotificationService : IClaimNotificationService
    {
        IMapperSession<ClaimNotification> _claimNotificationRepository;

        public ClaimNotificationService(IMapperSession<ClaimNotification> claimNotificationRepository)
        {
            _claimNotificationRepository = claimNotificationRepository;
        }

        public async Task<ClaimNotification> GetClaimNotificationById(Guid claimId)
        {
            return await _claimNotificationRepository.GetByIdAsync(claimId);
        }
        //np code
        public async Task UpdateClaimNotification(ClaimNotification claimNotification)
        {
            await _claimNotificationRepository.UpdateAsync(claimNotification);
        }

    }
}

