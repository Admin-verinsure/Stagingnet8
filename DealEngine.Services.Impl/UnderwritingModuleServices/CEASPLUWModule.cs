using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class CEASPLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public CEASPLUWModule()
        {
            Name = "CEAS_PL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm plterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PL" && ct.DateDeleted == null))
                {
                    plterm.Delete(underwritingUser);
                }
            }

            //IDictionary<string, decimal> rates = BuildRulesTable(agreement, "pl1millimitincomeunder1milpremium", "pl2millimitincomeunder1milpremium", "pl3millimitincomeunder1milpremium", "pl4millimitincomeunder1milpremium", "pl5millimitincomeunder1milpremium",
            //    "pl1millimitincome1milto3milpremium", "pl2millimitincome1milto3milpremium", "pl3millimitincome1milto3milpremium", "pl4millimitincome1milto3milpremium", "pl5millimitincome1milto3milpremium",
            //    "pl1millimitincome3milto5milpremium", "pl2millimitincome3milto5milpremium", "pl3millimitincome3milto5milpremium", "pl4millimitincome3milto5milpremium", "pl5millimitincome3milto5milpremium");

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

            int TermLimit500k = 500000;
            decimal TermPremiumDEFAULT = 0m;
            decimal TermBrokerageDEFAULT = 0m;
            int TermExcess = 500;
            decimal feeincome = 0;


            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms500klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit500k, TermExcess);
            terms500klimitoption.TermLimit = TermLimit500k;
            terms500klimitoption.Premium = TermPremiumDEFAULT;
            terms500klimitoption.Excess = TermExcess;
            terms500klimitoption.BrokerageRate = agreement.Brokerage;
            terms500klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms500klimitoption.DateDeleted = null;
            terms500klimitoption.DeletedBy = null;

            int TermLimit1000k = 1000000;

            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms1000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit1000k, TermExcess);
            terms1000klimitoption.TermLimit = TermLimit1000k;
            terms1000klimitoption.Premium = TermPremiumDEFAULT;
            terms1000klimitoption.Excess = TermExcess;
            terms1000klimitoption.BrokerageRate = agreement.Brokerage;
            terms1000klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms1000klimitoption.DateDeleted = null;
            terms1000klimitoption.DeletedBy = null;

            int TermLimit2000k = 2000000;

            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms2000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit2000k, TermExcess);
            terms2000klimitoption.TermLimit = TermLimit2000k;
            terms2000klimitoption.Premium = TermPremiumDEFAULT;
            terms2000klimitoption.Excess = TermExcess;
            terms2000klimitoption.BrokerageRate = agreement.Brokerage;
            terms2000klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms2000klimitoption.DateDeleted = null;
            terms2000klimitoption.DeletedBy = null;
            
            int TermLimit5000k = 5000000;

            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms5000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit5000k, TermExcess);
            terms5000klimitoption.TermLimit = TermLimit5000k;
            terms5000klimitoption.Premium = TermPremiumDEFAULT;
            terms5000klimitoption.Excess = TermExcess;
            terms5000klimitoption.BrokerageRate = agreement.Brokerage;
            terms5000klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms5000klimitoption.DateDeleted = null;
            terms5000klimitoption.DeletedBy = null;
            
            int TermLimit10000k = 10000000;

            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms10000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit10000k, TermExcess);
            terms10000klimitoption.TermLimit = TermLimit10000k;
            terms10000klimitoption.Premium = TermPremiumDEFAULT;
            terms10000klimitoption.Excess = TermExcess;
            terms10000klimitoption.BrokerageRate = agreement.Brokerage;
            terms10000klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms10000klimitoption.DateDeleted = null;
            terms10000klimitoption.DeletedBy = null;
            
            int TermLimit20000k = 20000000;

            //TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm terms20000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit20000k, TermExcess);
            terms20000klimitoption.TermLimit = TermLimit20000k;
            terms20000klimitoption.Premium = TermPremiumDEFAULT;
            terms20000klimitoption.Excess = TermExcess;
            terms20000klimitoption.BrokerageRate = agreement.Brokerage;
            terms20000klimitoption.Brokerage = TermBrokerageDEFAULT;
            terms20000klimitoption.DateDeleted = null;
            terms20000klimitoption.DeletedBy = null;


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


            string auditLogDetail = "CEAS PL UW created/modified";
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
