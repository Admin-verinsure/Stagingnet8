using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZACSDOUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZACSDOUWModule()
        {
            Name = "NZACS_DO";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "do250klimitincomeunder500kpremium", "do500klimitincomeunder500kpremium", "do1millimitincomeunder500kpremium",
                "do250klimitincome500kto750kpremium", "do500klimitincome500kto750kpremium", "do1millimitincome500kto750kpremium",
                "do250klimitincome750kto1milpremium", "do500klimitincome750kto1milpremium", "do1millimitincome750kto1milpremium",
                "do250klimitincome1milto2milpremium", "do500klimitincome1milto2milpremium", "do1millimitincome1milto2milpremium",
                "do250klimitincome2milto5milpremium", "do500klimitincome2milto5milpremium", "do1millimitincome2milto5milpremium",
                "do250klimitincome5milto10milpremium", "do500klimitincome5milto10milpremium", "do1millimitincome5milto10milpremium");

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

            int TermLimit250k = 250000;
            decimal TermPremium250k = 0m;
            decimal TermBrokerage250k = 0m;
            int TermLimit500k = 500000;
            decimal TermPremium500k = 0m;
            decimal TermBrokerage500k = 0m;
            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0m;
            decimal TermBrokerage1mil = 0m;

            int TermExcess = 0;
            decimal feeincome = 0;
            //Calculation
            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (uISTerritory.Location == "NZ") //NZ income only
                    {
                        feeincome = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "totalRevenue").First().Value) * uISTerritory.Percentage / 100;
                    }
                }
            }

            //Return terms based on the limit options

            TermExcess = 1000;

            TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            ClientAgreementTerm termsl250klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit250k, TermExcess);
            termsl250klimitoption.TermLimit = TermLimit250k;
            termsl250klimitoption.Premium = TermPremium250k;
            termsl250klimitoption.Excess = TermExcess;
            termsl250klimitoption.BrokerageRate = agreement.Brokerage;
            termsl250klimitoption.Brokerage = TermBrokerage250k;
            termsl250klimitoption.DateDeleted = null;
            termsl250klimitoption.DeletedBy = null;

            TermPremium500k = GetPremiumFor(rates, feeincome, TermLimit500k);
            ClientAgreementTerm termsl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit500k, TermExcess);
            termsl500klimitoption.TermLimit = TermLimit500k;
            termsl500klimitoption.Premium = TermPremium500k;
            termsl500klimitoption.Excess = TermExcess;
            termsl500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl500klimitoption.Brokerage = TermBrokerage500k;
            termsl500klimitoption.DateDeleted = null;
            termsl500klimitoption.DeletedBy = null;

            TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm termsl1millimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit1mil, TermExcess);
            termsl1millimitoption.TermLimit = TermLimit1mil;
            termsl1millimitoption.Premium = TermPremium1mil;
            termsl1millimitoption.Excess = TermExcess;
            termsl1millimitoption.BrokerageRate = agreement.Brokerage;
            termsl1millimitoption.Brokerage = TermBrokerage1mil;
            termsl1millimitoption.DateDeleted = null;
            termsl1millimitoption.DeletedBy = null;


            ////Referral points per agreement
            ////Not a renewal of an existing policy
            //uwrfnotrenewaldo(underwritingUser, agreement);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }


            string auditLogDetail = "NZACS DO UW created/modified";
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

        decimal GetPremiumFor(IDictionary<string, decimal> rates, decimal feeincome, int limitoption)
        {
            decimal premiumoption = 0M;

            switch (limitoption)
            {
                case 250000:
                    {
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["do250klimitincomeunder500kpremium"];
                        }
                        else if (feeincome > 500000 && feeincome <= 750000)
                        {
                            premiumoption = rates["do250klimitincome500kto750kpremium"];
                        }
                        else if (feeincome > 750000 && feeincome <= 1000000)
                        {
                            premiumoption = rates["do250klimitincome750kto1milpremium"];
                        }
                        else if (feeincome > 1000000 && feeincome <= 2000000)
                        {
                            premiumoption = rates["do250klimitincome1milto2milpremium"];
                        }
                        else if (feeincome > 2000000 && feeincome <= 5000000)
                        {
                            premiumoption = rates["do250klimitincome2milto5milpremium"];
                        }
                        else if (feeincome > 5000000 && feeincome <= 10000000)
                        {
                            premiumoption = rates["do250klimitincome5milto10milpremium"];
                        }
                        break;
                    }
                case 500000:
                    {
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["do500klimitincomeunder500kpremium"];
                        }
                        else if (feeincome > 500000 && feeincome <= 750000)
                        {
                            premiumoption = rates["do500klimitincome500kto750kpremium"];
                        }
                        else if (feeincome > 750000 && feeincome <= 1000000)
                        {
                            premiumoption = rates["do500klimitincome750kto1milpremium"];
                        }
                        else if (feeincome > 1000000 && feeincome <= 2000000)
                        {
                            premiumoption = rates["do500klimitincome1milto2milpremium"];
                        }
                        else if (feeincome > 2000000 && feeincome <= 5000000)
                        {
                            premiumoption = rates["do500klimitincome2milto5milpremium"];
                        }
                        else if (feeincome > 5000000 && feeincome <= 10000000)
                        {
                            premiumoption = rates["do500klimitincome5milto10milpremium"];
                        }
                        break;
                    }
                case 1000000:
                    {
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["do1millimitincomeunder500kpremium"];
                        }
                        else if (feeincome > 500000 && feeincome <= 750000)
                        {
                            premiumoption = rates["do1millimitincome500kto750kpremium"];
                        }
                        else if (feeincome > 750000 && feeincome <= 1000000)
                        {
                            premiumoption = rates["do1millimitincome750kto1milpremium"];
                        }
                        else if (feeincome > 1000000 && feeincome <= 2000000)
                        {
                            premiumoption = rates["do1millimitincome1milto2milpremium"];
                        }
                        else if (feeincome > 2000000 && feeincome <= 5000000)
                        {
                            premiumoption = rates["do1millimitincome2milto5milpremium"];
                        }
                        else if (feeincome > 5000000 && feeincome <= 10000000)
                        {
                            premiumoption = rates["do1millimitincome5milto10milpremium"];
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format("Can not calculate premium for DO"));
                    }
            }

            return premiumoption;
        }

        void uwrfnotrenewaldo(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewaldo" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewaldo").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "true")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewaldo" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DirectorsandOfficers3").First().Value == "false")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewaldo" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewaldo" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewaldo" && cref.DateDeleted == null).Status = "";
                    }
                }

                
            }
        }


    }
}