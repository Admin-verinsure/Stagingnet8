using System;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClientAgreementService
    {
        Task CreateClientAgreement(User createdBy, string insuredName, DateTime inceptionDate, DateTime expiryDate, decimal brokerage, decimal brokerFee, ClientInformationSheet clientInformationSheet);

        Task<ClientAgreement> GetAgreement(Guid clientAgreementId);

        Task<ClientAgreement> AcceptAgreement (ClientAgreement agreement, User acceptingUser);
        //ClientAgreement AcceptAgreement (ClientAgreement agreement, User acceptingUser);
        Task<ClientAgreement> GetAgreementbyReferenceNum(string reference);
        Task UpdateClientAgreement(ClientAgreement clientAgreement);
        Task<ClientAgreementTermExtension> GetAgreementExtension(Guid clientAgreementTermExtensionId);
        
    }
}
