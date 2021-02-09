using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class ApolloDOUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public ApolloDOUWModule()
        {
            Name = "Apollo_DO";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "maximumassetsize", "do1millimitpremium");

            //Create default referral points based on the clientagreementrules
            if (agreement.ClientAgreementReferrals.Count == 0)
            {
                foreach (var clientagreementreferralrule in agreement.ClientAgreementRules.Where(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null))
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, clientagreementreferralrule.Name, clientagreementreferralrule.Description, "", clientagreementreferralrule.Value, clientagreementreferralrule.OrderNumber));
            }
            else
            {
                foreach (var clientagreementreferral in agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null))
                    clientagreementreferral.Status = "";
            }

            int agreementperiodindays = 0;
            agreementperiodindays = (agreement.ExpiryDate - agreement.InceptionDate).Days;

            agreement.QuoteDate = DateTime.UtcNow;

            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddYears(-1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            string strretrodate = "";
            if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            {
                foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
                {
                    if (preRenewOrRefData.DataType == "preterm")
                    {
                        if (!string.IsNullOrEmpty(preRenewOrRefData.DORetro))
                        {
                            strretrodate = preRenewOrRefData.DORetro;
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "DO")
                    {
                        if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
                        {
                            ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
                        }
                    }
                }
            }

            string strProfessionalBusiness = "General Insurance Brokers, Life Agents, Investment Advisers, Financial Planning and Mortgage Broking, Consultants and Advisers in the sale of any financial product including referrals to other financial product providers.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            
            int intCompanyAge = 0;
            decimal decDOAssets = 0m;
            decimal decDOLiabs = 0m;


            if (agreement.ClientInformationSheet.Owner.DateofIncorportation.Value != null)
            {
                intCompanyAge = DateTime.Now.Subtract(agreement.ClientInformationSheet.Owner.DateofIncorportation.Value).Days;
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetTotal").Any())
            {
                decDOAssets = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetTotal").First().Value);
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.DebtTotal").Any())
            {
                decDOLiabs = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.DebtTotal").First().Value);
            }


            ClientAgreementEndorsement cAEDOInsExcl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Insolvency Exclusion");
            if (cAEDOInsExcl != null)
            {
                cAEDOInsExcl.DateDeleted = DateTime.UtcNow;
                cAEDOInsExcl.DeletedBy = underwritingUser;
            }
            if ((decDOAssets < decDOLiabs) || (intCompanyAge < 730))
            {
                if (cAEDOInsExcl != null)
                {
                    cAEDOInsExcl.DateDeleted = null;
                    cAEDOInsExcl.DeletedBy = null;
                }
            }

            int TermLimit500k = 500000;
            decimal TermPremium500k = 0m;
            decimal TermBrokerage500k = 0m;
            int TermExcess500k = 0;

            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium500k = TermPremium500k / coverperiodindays * agreementperiodindays;

            TermBrokerage500k = TermPremium500k * agreement.Brokerage / 100;

            ClientAgreementTerm termdo500klimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit500k, TermExcess500k);
            termdo500klimitoption.TermLimit = TermLimit500k;
            termdo500klimitoption.Premium = TermPremium500k;
            termdo500klimitoption.BasePremium = TermPremium500k;
            termdo500klimitoption.Excess = TermExcess500k;
            termdo500klimitoption.BrokerageRate = agreement.Brokerage;
            termdo500klimitoption.Brokerage = TermBrokerage500k;
            termdo500klimitoption.DateDeleted = null;
            termdo500klimitoption.DeletedBy = null;


            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0m;
            decimal TermBrokerage1mil = 0m;
            int TermExcess1mil = 0;
            TermPremium1mil = rates["do1millimitpremium"];

            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium1mil = TermPremium1mil / coverperiodindays * agreementperiodindays;

            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

            ClientAgreementTerm termdo1millimitoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit1mil, TermExcess1mil);
            termdo1millimitoption.TermLimit = TermLimit1mil;
            termdo1millimitoption.Premium = TermPremium1mil;
            termdo1millimitoption.BasePremium = TermPremium1mil;
            termdo1millimitoption.Excess = TermExcess1mil;
            termdo1millimitoption.BrokerageRate = agreement.Brokerage;
            termdo1millimitoption.Brokerage = TermBrokerage1mil;
            termdo1millimitoption.DateDeleted = null;
            termdo1millimitoption.DeletedBy = null;

            //Change policy premium claculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "DO"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termdo500klimitoption.PremiumDiffer = (TermPremium500k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termdo500klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termdo500klimitoption.TermLimit == term.TermLimit && termdo500klimitoption.Excess == term.Excess)
                        {
                            termdo500klimitoption.Bound = true;
                        }
                        if (termdo500klimitoption.PremiumDiffer < 0)
                        {
                            termdo500klimitoption.PremiumDiffer = 0;
                        }
                        termdo1millimitoption.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termdo1millimitoption.PremiumPre = PreviousBoundPremium;
                        if (termdo1millimitoption.TermLimit == term.TermLimit && termdo1millimitoption.Excess == term.Excess)
                        {
                            termdo1millimitoption.Bound = true;
                        }
                        if (termdo1millimitoption.PremiumDiffer < 0)
                        {
                            termdo1millimitoption.PremiumDiffer = 0;
                        }

                    }

                }
            }


            //Referral points per agreement
            //Asset Size
            uwrfassetsize(underwritingUser, agreement, rates);
            //D&O Issues
            uwrdoissue(underwritingUser, agreement);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Policy Inception";
            agreement.TerritoryLimit = "Worldwide";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Apollo DO UW created/modified";
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

                //Inception date rule (turned on after implementing change, any remaining policy and new policy will use submission date as inception date)
                if (DateTime.UtcNow > product.DefaultInceptionDate)
                {
                    inceptionDate = DateTime.UtcNow;
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

        void uwrfassetsize(User underwritingUser, ClientAgreement agreement, IDictionary<string, decimal> rates)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfassetsize" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfassetsize" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetTotal").First().Value) > rates["maximumassetsize"])
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfassetsize" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }

        void uwrdoissue(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasClaimOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasCircumstanceOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDeclinedOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasReceivershipOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasObligationOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasInvestigationOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasProcecutionOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasCriminalOptions").First().Value == "1")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }


    }
}