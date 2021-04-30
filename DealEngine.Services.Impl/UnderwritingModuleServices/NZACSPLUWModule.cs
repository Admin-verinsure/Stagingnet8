using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZACSPLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZACSPLUWModule()
        {
            Name = "NZACS_PL";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "pl1millimitincomeunder1milpremium", "pl2millimitincomeunder1milpremium", "pl3millimitincomeunder1milpremium", "pl4millimitincomeunder1milpremium", "pl5millimitincomeunder1milpremium",
                "pl1millimitincome1milto3milpremium", "pl2millimitincome1milto3milpremium", "pl3millimitincome1milto3milpremium", "pl4millimitincome1milto3milpremium", "pl5millimitincome1milto3milpremium",
                "pl1millimitincome3milto5milpremium", "pl2millimitincome3milto5milpremium", "pl3millimitincome3milto5milpremium", "pl4millimitincome3milto5milpremium", "pl5millimitincome3milto5milpremium");

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

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0m;
            decimal TermBrokerage1mil = 0m;
            int TermLimit2mil = 2000000;
            decimal TermPremium2mil = 0m;
            decimal TermBrokerage2mil = 0m;
            int TermLimit3mil = 3000000;
            decimal TermPremium3mil = 0m;
            decimal TermBrokerage3mil = 0m;
            int TermLimit4mil = 4000000;
            decimal TermPremium4mil = 0m;
            decimal TermBrokerage4mil = 0m;
            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = 0m;
            decimal TermBrokerage5mil = 0m;

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

            TermExcess = 500;

            TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            ClientAgreementTerm termsl1millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit1mil, TermExcess);
            termsl1millimitoption.TermLimit = TermLimit1mil;
            termsl1millimitoption.Premium = TermPremium1mil;
            termsl1millimitoption.Excess = TermExcess;
            termsl1millimitoption.BrokerageRate = agreement.Brokerage;
            termsl1millimitoption.Brokerage = TermBrokerage1mil;
            termsl1millimitoption.DateDeleted = null;
            termsl1millimitoption.DeletedBy = null;

            TermPremium2mil = GetPremiumFor(rates, feeincome, TermLimit2mil);
            ClientAgreementTerm termsl2millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit2mil, TermExcess);
            termsl2millimitoption.TermLimit = TermLimit2mil;
            termsl2millimitoption.Premium = TermPremium2mil;
            termsl2millimitoption.Excess = TermExcess;
            termsl2millimitoption.BrokerageRate = agreement.Brokerage;
            termsl2millimitoption.Brokerage = TermBrokerage2mil;
            termsl2millimitoption.DateDeleted = null;
            termsl2millimitoption.DeletedBy = null;

            TermPremium3mil = GetPremiumFor(rates, feeincome, TermLimit3mil);
            ClientAgreementTerm termsl3millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit3mil, TermExcess);
            termsl3millimitoption.TermLimit = TermLimit3mil;
            termsl3millimitoption.Premium = TermPremium3mil;
            termsl3millimitoption.Excess = TermExcess;
            termsl3millimitoption.BrokerageRate = agreement.Brokerage;
            termsl3millimitoption.Brokerage = TermBrokerage3mil;
            termsl3millimitoption.DateDeleted = null;
            termsl3millimitoption.DeletedBy = null;

            TermPremium4mil = GetPremiumFor(rates, feeincome, TermLimit4mil);
            ClientAgreementTerm termsl4millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit4mil, TermExcess);
            termsl4millimitoption.TermLimit = TermLimit4mil;
            termsl4millimitoption.Premium = TermPremium4mil;
            termsl4millimitoption.Excess = TermExcess;
            termsl4millimitoption.BrokerageRate = agreement.Brokerage;
            termsl4millimitoption.Brokerage = TermBrokerage4mil;
            termsl4millimitoption.DateDeleted = null;
            termsl4millimitoption.DeletedBy = null;

            TermPremium5mil = GetPremiumFor(rates, feeincome, TermLimit5mil);
            ClientAgreementTerm termsl5millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit5mil, TermExcess);
            termsl5millimitoption.TermLimit = TermLimit5mil;
            termsl5millimitoption.Premium = TermPremium5mil;
            termsl5millimitoption.Excess = TermExcess;
            termsl5millimitoption.BrokerageRate = agreement.Brokerage;
            termsl5millimitoption.Brokerage = TermBrokerage5mil;
            termsl5millimitoption.DateDeleted = null;
            termsl5millimitoption.DeletedBy = null;

            ////Referral points per agreement
            ////High GL Limits
            //uwrfhighgllimits(underwritingUser, agreement);
            ////Not a renewal of an existing policy
            //uwrfnotrenewalpl(underwritingUser, agreement);


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }


            string auditLogDetail = "NZACS PL UW created/modified";
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
                case 1000000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["pl1millimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 3000000)
                        {
                            premiumoption = rates["pl1millimitincome1milto3milpremium"];
                        }
                        else if (feeincome >= 3000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["pl1millimitincome3milto5milpremium"];
                        }
                        break;
                    }
                case 2000000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["pl2millimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 3000000)
                        {
                            premiumoption = rates["pl2millimitincome1milto3milpremium"];
                        }
                        else if (feeincome >= 3000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["pl2millimitincome3milto5milpremium"];
                        }
                        break;
                    }
                case 3000000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["pl3millimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 3000000)
                        {
                            premiumoption = rates["pl3millimitincome1milto3milpremium"];
                        }
                        else if (feeincome >= 3000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["pl3millimitincome3milto5milpremium"];
                        }
                        break;
                    }
                case 4000000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["pl4millimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 3000000)
                        {
                            premiumoption = rates["pl4millimitincome1milto3milpremium"];
                        }
                        else if (feeincome >= 3000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["pl4millimitincome3milto5milpremium"];
                        }
                        break;
                    }
                case 5000000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["pl5millimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 3000000)
                        {
                            premiumoption = rates["pl5millimitincome1milto3milpremium"];
                        }
                        else if (feeincome >= 3000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["pl5millimitincome3milto5milpremium"];
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format("Can not calculate premium for PL"));
                    }
            }

            return premiumoption;
        }

        void uwrfhighgllimits(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighgllimits" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits").OrderNumber, 
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighgllimits").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "true")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighgllimits" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "GeneralLiabilityInsurance2").First().Value == "true")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighgllimits" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighgllimits" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighgllimits" && cref.DateDeleted == null).Status = "";
                    }
                }

                
            }
        }

        void uwrfnotrenewalpl(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalpl" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalpl").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "true")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalpl" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "GeneralLiabilityInsurance3").First().Value == "false")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalpl" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalpl" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalpl" && cref.DateDeleted == null).Status = "";
                    }
                }

                
            }
        }


    }
}
