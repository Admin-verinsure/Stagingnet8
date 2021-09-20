using System;
using System.Threading.Tasks;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;


namespace DealEngine.Services.Impl
{
    public class ClientAgreementService : IClientAgreementService
    {
        IMapperSession<ClientAgreement> _clientAgreementRepository;
        IMapperSession<ClientInformationSheet> _clientInformationSheetRepository;
        IMapperSession<ClientAgreementTermExtension> _clientAgreementExtensionRepository;

        public ClientAgreementService(IMapperSession<ClientAgreement> clientAgreementRepository, IMapperSession<ClientInformationSheet> clientInformationSheetRepository)
        {
            _clientAgreementRepository = clientAgreementRepository;
            _clientInformationSheetRepository = clientInformationSheetRepository;
        }

        public async Task CreateClientAgreement(User createdBy, string insuredName, DateTime inceptionDate, DateTime expiryDate, decimal brokerage, decimal brokerFee, ClientInformationSheet clientInformationSheet)
        {
            if (string.IsNullOrWhiteSpace(insuredName))
                throw new ArgumentNullException(nameof(insuredName));
            if (clientInformationSheet == null)
                throw new ArgumentNullException(nameof(clientInformationSheet));

            ClientAgreement clientAgreement = new ClientAgreement (createdBy, insuredName, inceptionDate, expiryDate, brokerage, brokerFee, clientInformationSheet, null, clientInformationSheet.ReferenceId);
            clientInformationSheet.ClientAgreement = clientAgreement;
            await _clientInformationSheetRepository.UpdateAsync(clientInformationSheet);
            await _clientAgreementRepository.AddAsync(clientAgreement);
        }

		public async Task<ClientAgreement> GetAgreement (Guid clientAgreementId)
		{
			return await _clientAgreementRepository.GetByIdAsync(clientAgreementId);
		}

        public async Task<ClientAgreement> GetAgreementbyReferenceNum(string reference)
        {
            return _clientAgreementRepository.FindAll().FirstOrDefault(cp => cp.ReferenceId == reference);
        }

        public async Task<ClientAgreement> AcceptAgreement (ClientAgreement agreement, User acceptingUser)
		{
			if (agreement == null)
				throw new ArgumentNullException (nameof (agreement));
			if (acceptingUser == null)
				throw new ArgumentNullException (nameof (acceptingUser));

		    agreement.ClientInformationSheet.Status = "Submitted";
			agreement.ClientInformationSheet.SubmitDate = DateTime.UtcNow;
			agreement.ClientInformationSheet.SubmittedBy = acceptingUser;
			
            await _clientAgreementRepository.UpdateAsync(agreement);
            return agreement;
		}

        public async Task UpdateClientAgreement(ClientAgreement clientAgreement)
        {
            await _clientAgreementRepository.UpdateAsync(clientAgreement);
        }

        public async Task<ClientAgreementTermExtension> GetAgreementExtension(Guid clientAgreementTermExtensionId)
        {
            return await _clientAgreementExtensionRepository.GetByIdAsync(clientAgreementTermExtensionId);
        }
    }
}
