using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClientAgreementRuleService
    {
        Task AddClientAgreementRule(User createdBy, Rule rule, string name, string description, Product product, string value, int orderNumber, string ruleCategory, string ruleRoleType, bool isPublic, ClientAgreement clientAgreement, bool doNotCheckForRenew);

        Task AddClientAgreementRule(User createdBy, Rule rule, ClientAgreement clientAgreement);

        Task<List<ClientAgreementRule>> GetAllClientAgreementRuleFor(ClientAgreement clientAgreement);

        Task<ClientAgreementRule> GetClientAgreementRuleBy(Guid clientAgreementRuleId);
    }
}
