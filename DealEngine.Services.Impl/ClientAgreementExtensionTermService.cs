using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;


namespace DealEngine.Services.Impl
{
    public class ClientAgreementExtensionTermService : IClientAgreementExtensionTermService
    {
        IMapperSession<ClientAgreementTermExtension> _clientAgreementTermExtentionRepository;
        IMapperSession<ClientAgreement> _clientAgreementRepository;

        public ClientAgreementExtensionTermService(IMapperSession<ClientAgreementTermExtension> clientAgreementExtensionTermRepository, IMapperSession<ClientAgreement> clientAgreementRepository)
        {
            _clientAgreementTermExtentionRepository = clientAgreementExtensionTermRepository;
            _clientAgreementRepository = clientAgreementRepository;
        }

        public async Task AddAgreementTerm(User createdBy, int termLimit, decimal excess, decimal premium, decimal fSL, decimal brokerageRate, decimal brokerage, ClientAgreement clientAgreement, string subTermType)
        {
            if (string.IsNullOrWhiteSpace(termLimit.ToString()))
                throw new ArgumentNullException(nameof(termLimit));
            if (string.IsNullOrWhiteSpace(excess.ToString()))
                throw new ArgumentNullException(nameof(excess));
            if (string.IsNullOrWhiteSpace(premium.ToString()))
                throw new ArgumentNullException(nameof(premium));
            if (string.IsNullOrWhiteSpace(fSL.ToString()))
                throw new ArgumentNullException(nameof(fSL));
            if (string.IsNullOrWhiteSpace(brokerageRate.ToString()))
                throw new ArgumentNullException(nameof(brokerageRate));
            if (string.IsNullOrWhiteSpace(brokerage.ToString()))
                throw new ArgumentNullException(nameof(brokerage));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            ClientAgreementTermExtension clientAgreementTermExtension = new ClientAgreementTermExtension(createdBy, termLimit, excess, premium,  clientAgreement);
            clientAgreement.ClientAgreementTermExtensions.Add(clientAgreementTermExtension);
            await _clientAgreementTermExtentionRepository.AddAsync(clientAgreementTermExtension);
            await _clientAgreementRepository.UpdateAsync(clientAgreement);

        }

        
        public async Task<List<ClientAgreementTermExtension>> GetAllAgreementTermFor(ClientAgreement clientAgreement)
        {
            return await _clientAgreementTermExtentionRepository.FindAll().Where(cagt => cagt.ClientAgreement == clientAgreement && 
                                                                              cagt.DateDeleted == null).ToListAsync();            
        }

        public async Task UpdateAgreementExtensionTerm(ClientAgreementTermExtension clientAgreementTermExtension)
        {
            await _clientAgreementTermExtentionRepository.AddAsync(clientAgreementTermExtension);            
        }

		public async Task DeleteAgreementTerm (User deletedBy, ClientAgreementTermExtension clientAgreementTermExtension)
		{
            clientAgreementTermExtension.Delete (deletedBy);
			await UpdateAgreementExtensionTerm(clientAgreementTermExtension);
		}

        public async Task<List<ClientAgreementTermExtension>> GetListAgreementExtensionTermFor(ClientAgreement clientAgreement)
        {
            return await _clientAgreementTermExtentionRepository.FindAll().Where(cagt => cagt.ClientAgreement == clientAgreement &&
                                                                              cagt.DateDeleted == null).ToListAsync();            
        }

        public async Task<List<ClientAgreementTermExtension>> GetAllClientAgreementExtensionTerm()
        {
            return await _clientAgreementTermExtentionRepository.FindAll().ToListAsync();
        }

        public async Task<ClientAgreementTermExtension> GetAgreementExtentionById(Guid clientAgreementExtentionId)
        {
            return await _clientAgreementTermExtentionRepository.GetByIdAsync(clientAgreementExtentionId);
        }
    }
}
