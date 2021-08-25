using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZPICLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZPICLUWModule()
        {
            Name = "NZPI_CL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "CL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm clterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "CL" && ct.DateDeleted == null))
                {
                    clterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "clsocialengineeringextpremium", "cl250klimitincomeunder1milpremium", "cl250klimitincome1milto5milpremium",
                "cl500klimitincomeunder1milpremium", "cl500klimitincome1milto5milpremium");

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

            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddYears(-1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            decimal feeincome = 0;
            decimal extpremium = 0m;

            string strProfessionalBusiness = "Planning / urban design, resource management, local government advice, transport planning, Environmental policy advice, heritage planning, planning commissioner, market research, land management investigations, disputes resolution, master planning, urban design workshops, training, university lecturing.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                }
            }

            string strretrodate = "";
            if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            {
                foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
                {
                    if (preRenewOrRefData.DataType == "preterm")
                    {
                        if (!string.IsNullOrEmpty(preRenewOrRefData.CLRetro))
                        {
                            strretrodate = preRenewOrRefData.CLRetro;
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "CL")
                    {
                        if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
                        {
                            ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
                        }
                    }
                }
            }


            ClientAgreementEndorsement cAECLExt = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Social Engineering Fraud Extension");
            ClientAgreementEndorsement cAECLDRB = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Data Recovery and Business Interruption Exclusion (DRB)");
            ClientAgreementEndorsement cAECLUPM = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Unencrypted Portable Media Exclusion (UPM)");

            if (cAECLExt != null)
            {
                cAECLExt.DateDeleted = DateTime.UtcNow;
                cAECLExt.DeletedBy = underwritingUser;
            }
            if (cAECLDRB != null)
            {
                cAECLDRB.DateDeleted = DateTime.UtcNow;
                cAECLDRB.DeletedBy = underwritingUser;
            }
            if (cAECLUPM != null)
            {
                cAECLUPM.DateDeleted = DateTime.UtcNow;
                cAECLUPM.DeletedBy = underwritingUser;
            }

            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
            {
                extpremium = rates["clsocialengineeringextpremium"];

                if (cAECLExt != null)
                {
                    cAECLExt.DateDeleted = null;
                    cAECLExt.DeletedBy = null;
                }
            }
            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasAccessControlOptions").First().Value == "2")
            {
                if (cAECLUPM != null)
                {
                    cAECLUPM.DateDeleted = null;
                    cAECLUPM.DeletedBy = null;
                }
            }
            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasBackupOptions").First().Value == "2")
            {
                if (cAECLDRB != null)
                {
                    cAECLDRB.DateDeleted = null;
                    cAECLDRB.DeletedBy = null;
                }
            }

            int TermExcess = 0;
            TermExcess = 2500;

            int TermLimit250k = 250000;
            decimal TermPremium250k = 0m;
            decimal TermBrokerage250k = 0m;
            TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k) + extpremium;
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium250k = TermPremium250k / coverperiodindays * agreementperiodindays;
            TermBrokerage250k = TermPremium250k * agreement.Brokerage / 100;

            ClientAgreementTerm termcl250klimitoption = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit250k, TermExcess);
            termcl250klimitoption.TermLimit = TermLimit250k;
            termcl250klimitoption.Premium = TermPremium250k;
            termcl250klimitoption.BasePremium = TermPremium250k;
            termcl250klimitoption.Excess = TermExcess;
            termcl250klimitoption.BrokerageRate = agreement.Brokerage;
            termcl250klimitoption.Brokerage = TermBrokerage250k;
            termcl250klimitoption.DateDeleted = null;
            termcl250klimitoption.DeletedBy = null;

            int TermLimit500k = 500000;
            decimal TermPremium500k = 0m;
            decimal TermBrokerage500k = 0m;
            TermPremium500k = GetPremiumFor(rates, feeincome, TermLimit500k) + extpremium;
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium500k = TermPremium500k / coverperiodindays * agreementperiodindays;
            TermBrokerage500k = TermPremium500k * agreement.Brokerage / 100;

            ClientAgreementTerm termcl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit500k, TermExcess);
            termcl500klimitoption.TermLimit = TermLimit500k;
            termcl500klimitoption.Premium = TermPremium500k;
            termcl500klimitoption.BasePremium = TermPremium500k;
            termcl500klimitoption.Excess = TermExcess;
            termcl500klimitoption.BrokerageRate = agreement.Brokerage;
            termcl500klimitoption.Brokerage = TermBrokerage500k;
            termcl500klimitoption.DateDeleted = null;
            termcl500klimitoption.DeletedBy = null;

            //Change policy premium claculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termcl250klimitoption.PremiumDiffer = (TermPremium250k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termcl250klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termcl250klimitoption.TermLimit == term.TermLimit && termcl250klimitoption.Excess == term.Excess)
                        {
                            termcl250klimitoption.Bound = true;
                        }
                        if (termcl250klimitoption.PremiumDiffer < 0)
                        {
                            termcl250klimitoption.PremiumDiffer = 0;
                        }
                        termcl500klimitoption.PremiumDiffer = (TermPremium500k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termcl500klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termcl500klimitoption.TermLimit == term.TermLimit && termcl500klimitoption.Excess == term.Excess)
                        {
                            termcl500klimitoption.Bound = true;
                        }
                        if (termcl500klimitoption.PremiumDiffer < 0)
                        {
                            termcl500klimitoption.PremiumDiffer = 0;
                        }
                    }

                }
            }

            //Referral points per agreement
            //Not a renewal of an existing policy
            uwrfnotrenewalcl(underwritingUser, agreement);
            //Cyber Issue
            uwrclissue(underwritingUser, agreement, feeincome);


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
            agreement.TerritoryLimit = "Worldwide";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZPI CL UW created/modified";
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

                //Inception date rule apply to the renewal client only for grace period of 1 month
                if (string.IsNullOrEmpty(informationSheet.Programme.ClientProgrammeMembershipNumber))
                {
                    if (DateTime.UtcNow > product.DefaultInceptionDate)
                    {
                        inceptionDate = DateTime.UtcNow;
                    }
                }
                else if (!string.IsNullOrEmpty(informationSheet.Programme.ClientProgrammeMembershipNumber))
                {
                    if (DateTime.UtcNow >= product.DefaultInceptionDate.AddMonths(1))
                    {
                        inceptionDate = DateTime.UtcNow;
                    }
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

        decimal GetPremiumFor(IDictionary<string, decimal> rates, decimal feeincome, int limitoption)
        {
            decimal premiumoption = 0M;

            switch (limitoption)
            {
                case 250000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["cl250klimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["cl250klimitincome1milto5milpremium"];
                        }
                        break;
                    }
                case 500000:
                    {
                        if (feeincome >= 0 && feeincome < 1000000)
                        {
                            premiumoption = rates["cl500klimitincomeunder1milpremium"];
                        }
                        else if (feeincome >= 1000000 && feeincome < 5000000)
                        {
                            premiumoption = rates["cl500klimitincome1milto5milpremium"];
                        }
                        break;
                    }

                default:
                    {
                        throw new Exception(string.Format("Can not calculate premium for CL"));
                    }
            }

            return premiumoption;
        }

        void uwrfnotrenewalcl(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasExistingPolicyOptions").First().Value == "2")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).Status = "";
                    }
                }
            }
        }

        void uwrclissue(User underwritingUser, ClientAgreement agreement, decimal feeincome)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrclissue").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (feeincome >= 5000000 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasLocationOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasActivityOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasConfidencialOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasBreachesOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasKnowledgeOptions").First().Value == "2")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).Status = "";
                    }
                }
            }
        }


    }
}

