using System;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClaimNotificationService
    {
        Task<ClaimNotification> GetClaimNotificationById(Guid claimId);
       // Task UpdateClaimNotification(ClaimNotification claim);

    }
}
