using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class PMINZCLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public PMINZCLUWModule()
        {
            Name = "PMINZ_CL";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "cl250klimitincomeunder500k", "cl250klimitincome500kto2andhalfmilpremium", "cl500klimitincomeunder500k", "cl500klimitincome500kto2andhalfmilpremium",
                "cl1millimitincomeunder500k", "cl1millimitincome500kto2andhalfmilpremium", "clsocialengineeringextpremium");

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

            //string strretrodate = "";
            //if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            //{
            //    foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
            //    {
            //        if (preRenewOrRefData.DataType == "preterm")
            //        {
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.CLRetro))
            //            {
            //                strretrodate = preRenewOrRefData.CLRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "CL")
            //        {
            //            if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
            //            {
            //                ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
            //                agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
            //            }
            //        }
            //    }
            //}

            int TermLimit250k = 250000;
            decimal TermPremium250k = 0m;
            decimal TermBrokerage250k = 0m;
            int TermLimit500k = 500000;
            decimal TermPremium500k = 0m;
            decimal TermBrokerage500k = 0m;
            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0m;
            decimal TermBrokerage1mil = 0m;

            decimal extpremium = 0m;
            int TermExcess = 0;
            decimal feeincome = 0M;
            decimal totalfeeincome = 0M;
            int numberoffeeincome = 1;
            //Calculation
            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                totalfeeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0)
                {
                    totalfeeincome += agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                    numberoffeeincome += 1;
                }
                if (agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal > 0)
                {
                    totalfeeincome += agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal;
                    numberoffeeincome += 1;
                }
                feeincome = totalfeeincome / numberoffeeincome;
            }

            //Return terms based on the limit options

            TermExcess = 2500;

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


            string strretrodate = "";
            if (agreement.ClientInformationSheet.IsRenewawl && agreement.ClientInformationSheet.RenewFromInformationSheet != null)
            {
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));

                if (renewFromAgreement != null)
                {
                    strretrodate = renewFromAgreement.RetroactiveDate;

                    foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
                    {

                        if (renewendorsement.DateDeleted == null && renewendorsement.Name != "Social Engineering Fraud Extension" && 
                            renewendorsement.Name != "Data Recovery and Business Interruption Exclusion (DRB)" && renewendorsement.Name != "Unencrypted Portable Media Exclusion (UPM)")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }


            }

            TermPremium250k = GetPremiumFor(rates, feeincome, TermLimit250k);
            TermPremium250k += extpremium;
            ClientAgreementTerm termsl250klimitoption = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit250k, TermExcess);
            termsl250klimitoption.TermLimit = TermLimit250k;
            termsl250klimitoption.Premium = TermPremium250k;
            termsl250klimitoption.Excess = TermExcess;
            termsl250klimitoption.BrokerageRate = agreement.Brokerage;
            termsl250klimitoption.Brokerage = TermBrokerage250k;
            termsl250klimitoption.DateDeleted = null;
            termsl250klimitoption.DeletedBy = null;
            termsl250klimitoption.BasePremium = TermPremium250k;

            TermPremium500k = GetPremiumFor(rates, feeincome, TermLimit500k);
            TermPremium500k += extpremium;
            ClientAgreementTerm termsl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit500k, TermExcess);
            termsl500klimitoption.TermLimit = TermLimit500k;
            termsl500klimitoption.Premium = TermPremium500k;
            termsl500klimitoption.Excess = TermExcess;
            termsl500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl500klimitoption.Brokerage = TermBrokerage500k;
            termsl500klimitoption.DateDeleted = null;
            termsl500klimitoption.DeletedBy = null;
            termsl500klimitoption.BasePremium = TermPremium500k;

            TermPremium1mil = GetPremiumFor(rates, feeincome, TermLimit1mil);
            TermPremium1mil += extpremium;
            ClientAgreementTerm termsl1millimitoption = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit1mil, TermExcess);
            termsl1millimitoption.TermLimit = TermLimit1mil;
            termsl1millimitoption.Premium = TermPremium1mil;
            termsl1millimitoption.Excess = TermExcess;
            termsl1millimitoption.BrokerageRate = agreement.Brokerage;
            termsl1millimitoption.Brokerage = TermBrokerage1mil;
            termsl1millimitoption.DateDeleted = null;
            termsl1millimitoption.DeletedBy = null;
            termsl1millimitoption.BasePremium = TermPremium1mil;

            //Change policy premium calculation
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
                        termsl250klimitoption.PremiumDiffer = (TermPremium250k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsl250klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termsl250klimitoption.TermLimit == term.TermLimit && termsl250klimitoption.Excess == term.Excess)
                        {
                            termsl250klimitoption.Bound = true;
                        }
                        if (termsl250klimitoption.PremiumDiffer < 0)
                        {
                            termsl250klimitoption.PremiumDiffer = 0;
                        }
                        termsl500klimitoption.PremiumDiffer = (TermPremium500k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsl500klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termsl500klimitoption.TermLimit == term.TermLimit && termsl500klimitoption.Excess == term.Excess)
                        {
                            termsl500klimitoption.Bound = true;
                        }
                        if (termsl500klimitoption.PremiumDiffer < 0)
                        {
                            termsl500klimitoption.PremiumDiffer = 0;
                        }
                        termsl1millimitoption.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsl1millimitoption.PremiumPre = PreviousBoundPremium;
                        if (termsl1millimitoption.TermLimit == term.TermLimit && termsl1millimitoption.Excess == term.Excess)
                        {
                            termsl1millimitoption.Bound = true;
                        }
                        if (termsl1millimitoption.PremiumDiffer < 0)
                        {
                            termsl1millimitoption.PremiumDiffer = 0;
                        }
                        
                    }

                }

                if (PreviousAgreement != null)
                {
                    strretrodate = PreviousAgreement.RetroactiveDate;

                    foreach (var changeendorsement in PreviousAgreement.ClientAgreementEndorsements)
                    {

                        if (changeendorsement.DateDeleted == null && changeendorsement.Name != "Social Engineering Fraud Extension" &&
                            changeendorsement.Name != "Data Recovery and Business Interruption Exclusion (DRB)" && changeendorsement.Name != "Unencrypted Portable Media Exclusion (UPM)")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, changeendorsement.Name, changeendorsement.Type, product, changeendorsement.Value, changeendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
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

            string retrodate = "Inception or Date since CL policy first held";
            agreement.TerritoryLimit = "Worldwide excluding USA/Canada";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            string auditLogDetail = "PMINZ CL UW created/modified";
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
                if (informationSheet.IsRenewawl)
                {
                    int renewalgraceperiodindays = 0;
                    renewalgraceperiodindays = programme.BaseProgramme.RenewGracePriodInDays;
                    if (DateTime.UtcNow > product.DefaultInceptionDate.AddDays(renewalgraceperiodindays))
                    {
                        inceptionDate = DateTime.UtcNow;
                    }
                }
                else
                {
                    if (DateTime.UtcNow > product.DefaultInceptionDate)
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
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["cl250klimitincomeunder500k"];
                        }
                        else if (feeincome > 500000 && feeincome <= 2500000)
                        {
                            premiumoption = rates["cl250klimitincome500kto2andhalfmilpremium"];
                        }
                        break;
                    }
                case 500000:
                    {
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["cl500klimitincomeunder500k"];
                        }
                        else if (feeincome > 500000 && feeincome <= 2500000)
                        {
                            premiumoption = rates["cl500klimitincome500kto2andhalfmilpremium"];
                        }
                        break;
                    }
                case 1000000:
                    {
                        if (feeincome >= 0 && feeincome <= 500000)
                        {
                            premiumoption = rates["cl1millimitincomeunder500k"];
                        }
                        else if (feeincome > 500000 && feeincome <= 2500000)
                        {
                            premiumoption = rates["cl1millimitincome500kto2andhalfmilpremium"];
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
                        if (feeincome > 2500000)
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

