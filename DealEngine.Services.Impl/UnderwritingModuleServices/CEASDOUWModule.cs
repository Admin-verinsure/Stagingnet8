using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class CEASDOUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public CEASDOUWModule()
        {
            Name = "CEAS_DO";
        }

        public bool Underwrite(User CurrentUser, ClientInformationSheet informationSheet)
        {
            throw new NotImplementedException();
        }

        public bool Underwrite(User underwritingUser, ClientInformationSheet informationSheet, Product product, string reference)
        {
            ClientAgreement agreement = GetClientAgreement(underwritingUser, informationSheet, informationSheet.Programme, product, reference);
            Guid id = agreement.Id;

            if (agreement.ClientAgreementRules.Count == 0)
                foreach (var rule in product.Rules.Where(r => !string.IsNullOrWhiteSpace(r.Name)))
                    agreement.ClientAgreementRules.Add(new ClientAgreementRule(underwritingUser, rule, agreement));

            if (agreement.ClientAgreementEndorsements.Count == 0)
                foreach (var endorsement in product.Endorsements.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
                    agreement.ClientAgreementEndorsements.Add(new ClientAgreementEndorsement(underwritingUser, endorsement, agreement));

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "DO" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm doterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "DO" && ct.DateDeleted == null))
                {
                    doterm.Delete(underwritingUser);
                }
            }

            //IDictionary<string, decimal> rates = BuildRulesTable(agreement, "do250klimitincomeunder500kpremium", "do500klimitincomeunder500kpremium", "do1millimitincomeunder500kpremium",
            //    "do250klimitincome500kto750kpremium", "do500klimitincome500kto750kpremium", "do1millimitincome500kto750kpremium",
            //    "do250klimitincome750kto1milpremium", "do500klimitincome750kto1milpremium", "do1millimitincome750kto1milpremium",
            //    "do250klimitincome1milto2milpremium", "do500klimitincome1milto2milpremium", "do1millimitincome1milto2milpremium",
            //    "do250klimitincome2milto5milpremium", "do500klimitincome2milto5milpremium", "do1millimitincome2milto5milpremium",
            //    "do250klimitincome5milto10milpremium", "do500klimitincome5milto10milpremium", "do1millimitincome5milto10milpremium");

            //Create default referral points based on the clientagreementrules
            if (agreement.ClientAgreementReferrals.Count == 0)
            {
                foreach (var clientagreementreferralrule in agreement.ClientAgreementRules.Where(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null))
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, clientagreementreferralrule.Name, clientagreementreferralrule.Description, "", clientagreementreferralrule.Value, clientagreementreferralrule.OrderNumber, clientagreementreferralrule.DoNotCheckForRenew));
            }
            else
            {
                foreach (var clientagreementreferral in agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null))
                    clientagreementreferral.Status = "";
            }

            int agreementperiodindays = 0;
            agreementperiodindays = (agreement.ExpiryDate - agreement.InceptionDate).Days;

            agreement.QuoteDate = DateTime.UtcNow;

            int TermLimit300k = 300000;
            decimal TermPremiumDEFAULT = 0m;
            decimal TermBrokerageDEFAULT = 0m;

            int TermExcess = 0;
            decimal feeincome = 0;
            TermExcess = 5000;

            //TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl300klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit300k, TermExcess);
            termsl300klimitoption.TermLimit = TermLimit300k;
            termsl300klimitoption.Premium = TermPremiumDEFAULT;
            termsl300klimitoption.Excess = TermExcess;
            termsl300klimitoption.BrokerageRate = agreement.Brokerage;
            termsl300klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl300klimitoption.DateDeleted = null;
            termsl300klimitoption.DeletedBy = null;

            int TermLimit500k = 500000;
            int TermExcess7k = 7500;

            //TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit500k, TermExcess);
            termsl500klimitoption.TermLimit = TermLimit500k;
            termsl500klimitoption.Premium = TermPremiumDEFAULT;
            termsl500klimitoption.Excess = TermExcess7k;
            termsl500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl500klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl500klimitoption.DateDeleted = null;
            termsl500klimitoption.DeletedBy = null;

            int TermLimit1000k = 1000000;
            int TermExcess10k = 10000;

            //TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl1000klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit1000k, TermExcess);
            termsl1000klimitoption.TermLimit = TermLimit1000k;
            termsl1000klimitoption.Premium = TermPremiumDEFAULT;
            termsl1000klimitoption.Excess = TermExcess10k;
            termsl1000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl1000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl1000klimitoption.DateDeleted = null;
            termsl1000klimitoption.DeletedBy = null;

            int TermLimit2000k = 2000000;
            int TermExcess15k = 15000;

            //TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl2000klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit2000k, TermExcess);
            termsl2000klimitoption.TermLimit = TermLimit2000k;
            termsl2000klimitoption.Premium = TermPremiumDEFAULT;
            termsl2000klimitoption.Excess = TermExcess15k;
            termsl2000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl2000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl2000klimitoption.DateDeleted = null;
            termsl2000klimitoption.DeletedBy = null;

            int TermLimit3000k = 3000000;
            int TermExcess20k = 20000;

            //TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl3000klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit3000k, TermExcess);
            termsl3000klimitoption.TermLimit = TermLimit3000k;
            termsl3000klimitoption.Premium = TermPremiumDEFAULT;
            termsl3000klimitoption.Excess = TermExcess20k;
            termsl3000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl3000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl3000klimitoption.DateDeleted = null;
            termsl3000klimitoption.DeletedBy = null;
            ////Referral points per agreement


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }


            string auditLogDetail = "CEAS DO UW created/modified";
            AuditLog auditLog = new AuditLog(underwritingUser, informationSheet, agreement, auditLogDetail);
            agreement.ClientAgreementAuditLogs.Add(auditLog);

            return true;

        }

        ClientAgreement GetClientAgreement(User currentUser, ClientInformationSheet informationSheet, ClientProgramme programme, Product product, string reference)
        {
            ClientAgreement clientAgreement = programme.Agreements.FirstOrDefault(a => a.Product != null && a.Product.Id == product.Id);
            ClientAgreement previousClientAgreement = null;
            if (clientAgreement == null)
            {
                DateTime inceptionDate = (product.DefaultInceptionDate > DateTime.MinValue) ? product.DefaultInceptionDate : DateTime.UtcNow;
                DateTime expiryDate = (product.DefaultExpiryDate > DateTime.MinValue) ? product.DefaultExpiryDate : DateTime.UtcNow.AddYears(1);

                if (informationSheet.IsChange) //change agreement to keep the original inception date and expiry date
                {
                    if (informationSheet.PreviousInformationSheet != null)
                    {
                        previousClientAgreement = informationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(prea => prea.Product != null && prea.Product.Id == product.Id);
                        if (previousClientAgreement != null)
                        {
                            inceptionDate = previousClientAgreement.InceptionDate;
                            expiryDate = previousClientAgreement.ExpiryDate;
                        }
                    }
                }
                clientAgreement = new ClientAgreement(currentUser, informationSheet.Owner.Name, inceptionDate, expiryDate, product.DefaultBrokerage, product.DefaultBrokerFee, informationSheet, product, reference);
                if (product.IsMasterProduct)
                {
                    clientAgreement.MasterAgreement = true;
                }
                else
                {
                    clientAgreement.MasterAgreement = false;
                }
                clientAgreement.PreviousAgreement = previousClientAgreement;
                programme.Agreements.Add(clientAgreement);
                clientAgreement.Status = "Quoted";
            }
            else
            {
                clientAgreement.DeletedBy = null;
                clientAgreement.DateDeleted = null;
            }
            return clientAgreement;
        }

        ClientAgreementTerm GetAgreementTerm(User CurrentUser, ClientAgreement agreement, string subTerm, int limitoption, decimal excessoption)
        {
            ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == subTerm && t.DateDeleted != null && t.TermLimit == limitoption && t.Excess == excessoption);

            if (term == null)
            {
                term = new ClientAgreementTerm(CurrentUser, 0, 0m, 0m, 0m, 0m, 0m, agreement, subTerm);
                agreement.ClientAgreementTerms.Add(term);
            }

            return term;
        }

        IDictionary<string, decimal> BuildRulesTable(ClientAgreement agreement, params string[] names)
        {
            var dict = new Dictionary<string, decimal>();

            foreach (string name in names)
                dict[name] = Convert.ToDecimal(agreement.ClientAgreementRules.FirstOrDefault(r => r.Name == name).Value);

            return dict;
        }

        


    }
}