using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClientAgreementTermService
    {

        Task AddAgreementTerm(User createdBy, int termLimit, decimal excess, decimal premium, decimal fSL, decimal brokerageRate, decimal brokerage, ClientAgreement clientAgreement, string subTermType);

        Task<List<ClientAgreementTerm>> GetAllAgreementTermFor(ClientAgreement clientAgreement);

        Task<List<ClientAgreementTerm>> GetListAgreementTermFor(ClientAgreement clientAgreement);

        Task UpdateAgreementTerm(ClientAgreementTerm clientAgreementTerm);

        Task DeleteAgreementTerm(User deletedBy, ClientAgreementTerm clientAgreementTerm);
        Task<List<ClientAgreementTerm>> GetAllClientAgreementTerm();
        Task<ClientAgreementTerm> GetAgreementById(string clientAgreementId);
        Task AddAgreementExtensionTerm(User createdBy, int termLimit, decimal excess, decimal premium, ClientAgreement clientAgreement);

    }
}
