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
    public class ClientAgreementRuleService : IClientAgreementRuleService
    {
        IMapperSession<ClientAgreementRule> _clientAgreementRuleRepository;
        IMapperSession<ClientAgreement> _clientAgreementRepository;

        public ClientAgreementRuleService(IMapperSession<ClientAgreementRule> clientAgreementRuleRepository, IMapperSession<ClientAgreement> clientAgreementRepository)
        {
            _clientAgreementRuleRepository = clientAgreementRuleRepository;
            _clientAgreementRepository = clientAgreementRepository;
        }

		public async Task AddClientAgreementRule (User createdBy, Rule rule, string name, string description, Product product, string value, int orderNumber, string ruleCategory, string ruleRoleType, bool isPublic, ClientAgreement clientAgreement, bool doNotCheckForRenew)
		{
			if (string.IsNullOrWhiteSpace (name))
				throw new ArgumentNullException (nameof (name));
			if (string.IsNullOrWhiteSpace (description))
				throw new ArgumentNullException (nameof (description));
			if (product == null)
				throw new ArgumentNullException (nameof (product));
			if (string.IsNullOrWhiteSpace (value))
				throw new ArgumentNullException (nameof (value));
			if (string.IsNullOrWhiteSpace (orderNumber.ToString ()))
				throw new ArgumentNullException (nameof (orderNumber));
			if (clientAgreement == null)
				throw new ArgumentNullException (nameof (clientAgreement));

            ClientAgreementRule clientAgreementRule = new ClientAgreementRule(createdBy, rule, rule.Name, rule.Description, rule.Product, rule.Value, rule.OrderNumber, rule.RuleCategory, rule.RuleRoleType, rule.IsPublic, clientAgreement, rule.DoNotCheckForRenew);
            clientAgreement.ClientAgreementRules.Add(clientAgreementRule);
            await _clientAgreementRuleRepository.AddAsync(clientAgreementRule);
            await _clientAgreementRepository.UpdateAsync(clientAgreement);
        }

        public async Task AddClientAgreementRule(User createdBy, Rule rule, ClientAgreement clientAgreement)
        {
			await AddClientAgreementRule(createdBy, rule, rule.Name, rule.Description, rule.Product, rule.Value, rule.OrderNumber, rule.RuleCategory, rule.RuleRoleType, rule.IsPublic, clientAgreement, rule.DoNotCheckForRenew);
        }


        public async Task<List<ClientAgreementRule>> GetAllClientAgreementRuleFor(ClientAgreement clientAgreement)
        {
            return await _clientAgreementRuleRepository.FindAll().Where(cagt => cagt.ClientAgreement == clientAgreement).ToListAsync();            
        }

        public async Task<ClientAgreementRule> GetClientAgreementRuleBy(Guid clientAgreementRuleId)
        {
            return await _clientAgreementRuleRepository.GetByIdAsync(clientAgreementRuleId);
        }
    }
}
