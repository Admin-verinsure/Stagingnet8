using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClientAgreementExtensionTermService
    {

        Task AddAgreementTerm(User createdBy, int termLimit, decimal excess, decimal premium, decimal fSL, decimal brokerageRate, decimal brokerage, ClientAgreement clientAgreement, string subTermType);

        Task<List<ClientAgreementTermExtension>> GetAllAgreementTermFor(ClientAgreement clientAgreement);

        Task<List<ClientAgreementTermExtension>> GetListAgreementExtensionTermFor(ClientAgreement clientAgreement);
        Task UpdateAgreementExtensionTerm(ClientAgreementTermExtension clientAgreementTermExtension);

        Task DeleteAgreementTerm(User deletedBy, ClientAgreementTermExtension clientAgreementTermExtension);
        Task<List<ClientAgreementTermExtension>> GetAllClientAgreementExtensionTerm();
        Task<ClientAgreementTermExtension> GetAgreementById(string clientAgreementId);

    }

}
