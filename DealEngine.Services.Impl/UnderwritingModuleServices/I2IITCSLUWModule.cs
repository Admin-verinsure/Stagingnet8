using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class I2IITCSLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public I2IITCSLUWModule()
        {
            Name = "I2IITC_SL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "SL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm slterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "SL" && ct.DateDeleted == null))
                {
                    slterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "slpremium250k", "slpremium500k");

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

            //inception date rule to use policy start and end date from UIS
            if (!informationSheet.IsChange)
            {
                if (informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").Any())
                {
                    agreement.InceptionDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").First().Value);
                    agreement.ExpiryDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").First().Value).AddYears(1);
                }
                if (informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyEndDate").Any())
                {
                    agreement.ExpiryDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyEndDate").First().Value);
                }
            }

            int agreementperiodindays = 0;
            agreementperiodindays = (agreement.ExpiryDate - agreement.InceptionDate).Days;

            agreement.QuoteDate = DateTime.UtcNow;

            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddYears(-1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;


            //string strProfessionalBusiness = "";

            //agreement.ProfessionalBusiness = strProfessionalBusiness;


            string strretrodate = "";

            if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            {
                foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
                {
                    if (preRenewOrRefData.DataType == "preterm")
                    {
                        if (!string.IsNullOrEmpty(preRenewOrRefData.PIRetro))
                        {
                            strretrodate = preRenewOrRefData.PIRetro;
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "SL")
                    {
                        if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
                        {
                            ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
                        }
                    }
                }
            }

            //set agreement fees
            agreement.PlacementFee = product.DefaultPlacementFee;
            agreement.AdditionalCertFee = product.DefaultAdditionalCertFee;

            int TermExcess = 500;

            int TermLimit250k = 250000;
            decimal TermPremium250k = rates["slpremium250k"];
            decimal TermBrokerage250k = 0M;
            
            TermBrokerage250k = TermPremium250k * agreement.Brokerage / 100;

            ClientAgreementTerm term250klimitexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "SL", TermLimit250k, TermExcess);
            term250klimitexcesspremiumoption.TermLimit = TermLimit250k;
            term250klimitexcesspremiumoption.Premium = TermPremium250k;
            term250klimitexcesspremiumoption.BasePremium = TermPremium250k;
            term250klimitexcesspremiumoption.Excess = TermExcess;
            term250klimitexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term250klimitexcesspremiumoption.Brokerage = TermBrokerage250k;
            term250klimitexcesspremiumoption.DateDeleted = null;
            term250klimitexcesspremiumoption.DeletedBy = null;


            int TermLimit500k = 500000;
            decimal TermPremium500k = rates["slpremium500k"];
            decimal TermBrokerage500k = 0M;
            
            TermBrokerage500k = TermPremium500k * agreement.Brokerage / 100;

            ClientAgreementTerm term500klimitexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "SL", TermLimit500k, TermExcess);
            term500klimitexcesspremiumoption.TermLimit = TermLimit500k;
            term500klimitexcesspremiumoption.Premium = TermPremium500k;
            term500klimitexcesspremiumoption.BasePremium = TermPremium500k;
            term500klimitexcesspremiumoption.Excess = TermExcess;
            term500klimitexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term500klimitexcesspremiumoption.Brokerage = TermBrokerage500k;
            term500klimitexcesspremiumoption.DateDeleted = null;
            term500klimitexcesspremiumoption.DeletedBy = null;

            ////Change policy premium claculation
            //if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            //{
            //    var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PI"));
            //    foreach (var term in PreviousAgreement.ClientAgreementTerms)
            //    {
            //        if (term.Bound)
            //        {
            //            var PreviousBoundPremium = term.Premium;
            //            if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
            //            {
            //                PreviousBoundPremium = term.BasePremium;
            //            }
            //            term1millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium1mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
            //            term1millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
            //            if (term1millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term1millimit1kexcesspremiumoption.Excess == term.Excess)
            //            {
            //                term1millimit1kexcesspremiumoption.Bound = true;
            //            }
            //            if (term1millimit1kexcesspremiumoption.PremiumDiffer < 0)
            //            {
            //                term1millimit1kexcesspremiumoption.PremiumDiffer = 0;
            //            }
            //        }

            //    }
            //}


            //Referral points per agreement


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
            agreement.TerritoryLimit = "New Zealand";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "I2I ITC SL UW created/modified";
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

                //inception date rule to use policy start and end date from UIS
                if (informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").Any())
                {
                    inceptionDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").First().Value);
                    expiryDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").First().Value).AddYears(1);
                }
                if (informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyEndDate").Any())
                {
                    expiryDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyEndDate").First().Value);
                }

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

