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
    public class ClientAgreementTermService : IClientAgreementTermService
    {
        IMapperSession<ClientAgreementTerm> _clientAgreementTermRepository;
        IMapperSession<ClientAgreement> _clientAgreementRepository;
        IMapperSession<ClientAgreementTermExtension> _clientAgreementTermExtensionRepository;

        public ClientAgreementTermService(IMapperSession<ClientAgreementTerm> clientAgreementTermRepository, IMapperSession<ClientAgreement> clientAgreementRepository, IMapperSession<ClientAgreementTermExtension> clientAgreementTermExtensionRepository)
        {
            _clientAgreementTermRepository = clientAgreementTermRepository;
            _clientAgreementRepository = clientAgreementRepository;
            _clientAgreementTermExtensionRepository = clientAgreementTermExtensionRepository;
        }
        public async Task AddAgreementExtensionTerm(User createdBy, int termLimit, decimal excess, decimal premium,  ClientAgreement clientAgreement)
        {
            if (string.IsNullOrWhiteSpace(termLimit.ToString()))
                throw new ArgumentNullException(nameof(termLimit));
            if (string.IsNullOrWhiteSpace(excess.ToString()))
                throw new ArgumentNullException(nameof(excess));
            if (string.IsNullOrWhiteSpace(premium.ToString()))
                throw new ArgumentNullException(nameof(premium));
            if (clientAgreement == null)
                throw new ArgumentNullException(nameof(clientAgreement));

            ClientAgreementTermExtension clientAgreementExtensionTerm = new ClientAgreementTermExtension(createdBy, termLimit, excess, premium,clientAgreement);
            clientAgreement.ClientAgreementTermExtensions.Add(clientAgreementExtensionTerm);
            await _clientAgreementTermExtensionRepository.AddAsync(clientAgreementExtensionTerm);
            await _clientAgreementRepository.UpdateAsync(clientAgreement);

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
            
		    ClientAgreementTerm clientAgreementTerm = new ClientAgreementTerm(createdBy, termLimit, excess, premium, fSL, brokerageRate, brokerage, clientAgreement, subTermType);
            clientAgreement.ClientAgreementTerms.Add(clientAgreementTerm);
            await _clientAgreementTermRepository.AddAsync(clientAgreementTerm);
            await _clientAgreementRepository.UpdateAsync(clientAgreement);

        }

        
        public async Task<List<ClientAgreementTerm>> GetAllAgreementTermFor(ClientAgreement clientAgreement)
        {
            return await _clientAgreementTermRepository.FindAll().Where(cagt => cagt.ClientAgreement == clientAgreement && 
                                                                              cagt.DateDeleted == null).ToListAsync();            
        }

        public async Task UpdateAgreementTerm(ClientAgreementTerm clientAgreementTerm)
        {
            await _clientAgreementTermRepository.AddAsync(clientAgreementTerm);            
        }

		public async Task DeleteAgreementTerm (User deletedBy, ClientAgreementTerm clientAgreementTerm)
		{
			clientAgreementTerm.Delete (deletedBy);
			await UpdateAgreementTerm (clientAgreementTerm);
		}

        public async Task<List<ClientAgreementTerm>> GetListAgreementTermFor(ClientAgreement clientAgreement)
        {
            return await _clientAgreementTermRepository.FindAll().Where(cagt => cagt.ClientAgreement == clientAgreement &&
                                                                              cagt.DateDeleted == null).ToListAsync();            
        }

        public async Task<List<ClientAgreementTerm>> GetAllClientAgreementTerm()
        {
            return await _clientAgreementTermRepository.FindAll().ToListAsync();
        }

        public async Task<ClientAgreementTerm> GetAgreementById(string clientAgreementId)
        {
            return await _clientAgreementTermRepository.GetByIdAsync(Guid.Parse(clientAgreementId) );
        }
    }
}
