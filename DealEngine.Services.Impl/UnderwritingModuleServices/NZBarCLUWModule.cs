using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZBarCLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZBarCLUWModule()
        {
            Name = "NZBar_CL";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "cl250klimitpremium", "cl500klimitpremium", "cl1millimitpremium", "clsocialengineeringextpremium");

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

            //For 1st year set up
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

            //For renewal
            //string strretrodate = "";
            //if (agreement.ClientInformationSheet.IsRenewawl && agreement.ClientInformationSheet.RenewFromInformationSheet != null)
            //{
            //    var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));

            //    if (renewFromAgreement != null)
            //    {
            //        strretrodate = renewFromAgreement.RetroactiveDate;

            //        foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
            //        {

            //            if (renewendorsement.DateDeleted == null  &&
            //                renewendorsement.Name != "Data Recovery and Business Interruption Exclusion (DRB)" && renewendorsement.Name != "Unencrypted Portable Media Exclusion (UPM)"
            //                 && renewendorsement.Name != "Social Engineering Fraud Extension")
            //            {
            //                ClientAgreementEndorsement newclientendorsement =
            //                    new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
            //                agreement.ClientAgreementEndorsements.Add(newclientendorsement);
            //            }
            //        }
            //    }
            //}


            decimal extpremium = 0m;

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

            foreach (ClientAgreementTermExtension cltermextension in agreement.ClientAgreementTermExtensions.Where(ctex => ctex.DateDeleted == null))
            {
                cltermextension.Delete(underwritingUser);
            }

            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" && 
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
            {
                extpremium = rates["clsocialengineeringextpremium"];

                //if (cAECLExt != null)
                //{
                //    cAECLExt.DateDeleted = null;
                //    cAECLExt.DeletedBy = null;
                //}

                //add Social Engineering Fraud Extension
                decimal decSEFPremium = extpremium;
                decimal decSEFBasePremium = extpremium;
                decSEFPremium = decSEFPremium / coverperiodindays * agreementperiodindays;
                
                ClientAgreementTermExtension termSEFextension = GetAgreementExtensionTerm(underwritingUser, agreement, 100000, 2500, decSEFPremium, "Social Engineering Fraud Extension");
                termSEFextension.ExtentionName = "Social Engineering Fraud Extension";
                termSEFextension.HideLimitExcess = false;
                termSEFextension.Premium = decSEFPremium;
                termSEFextension.BasePremium = decSEFBasePremium;
                termSEFextension.DateDeleted = null;
                termSEFextension.DeletedBy = null;

                //Change policy premium claculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
                    var PreviousAgreementExtension = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                    foreach (var termExtension in PreviousAgreementExtension.ClientAgreementTermExtensions)
                    {
                        if (termExtension.Bound)
                        {
                            var PreviousBoundPremiumExtension = termExtension.Premium;
                            if (termExtension.BasePremium > 0 && PreviousAgreementExtension.ClientInformationSheet.IsChange)
                            {
                                PreviousBoundPremiumExtension = termExtension.BasePremium;
                            }
                            termSEFextension.PremiumDiffer = (decSEFPremium - PreviousBoundPremiumExtension) * coverperiodindaysforchange / agreementperiodindays;
                            termSEFextension.PremiumPre = PreviousBoundPremiumExtension;
                            if (termSEFextension.TermLimit == termExtension.TermLimit && termSEFextension.Excess == termExtension.Excess && termSEFextension.ExtentionName == termExtension.ExtentionName)
                            {
                                termSEFextension.Bound = true;
                            }
                            if (termSEFextension.PremiumDiffer < 0)
                            {
                                termSEFextension.PremiumDiffer = 0;
                            }

                        }

                    }
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


            int TermLimit250k = 250000;
            decimal TermPremium250k = 0M;
            decimal TermBasePremium250k = 0M;
            decimal TermBrokerage250k = 0M;
            decimal TermExcess250k = 2500;
            TermPremium250k = rates["cl250klimitpremium"];
            TermBasePremium250k = TermPremium250k;
            TermPremium250k = TermPremium250k * agreementperiodindays / coverperiodindays;
            TermBrokerage250k = TermPremium250k * agreement.Brokerage / 100;

            ClientAgreementTerm termiltermoption250k = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit250k, TermExcess250k);
            termiltermoption250k.TermLimit = TermLimit250k;
            termiltermoption250k.Premium = TermPremium250k;
            termiltermoption250k.BasePremium = TermBasePremium250k;
            termiltermoption250k.Excess = TermExcess250k;
            termiltermoption250k.BrokerageRate = agreement.Brokerage;
            termiltermoption250k.Brokerage = TermBrokerage250k;
            termiltermoption250k.DateDeleted = null;
            termiltermoption250k.DeletedBy = null;

            int TermLimit500k = 500000;
            decimal TermPremium500k = 0M;
            decimal TermBasePremium500k = 0M;
            decimal TermBrokerage500k = 0M;
            decimal TermExcess500k = 2500;
            TermPremium500k = rates["cl500klimitpremium"];
            TermBasePremium500k = TermPremium500k;
            TermPremium500k = TermPremium500k * agreementperiodindays / coverperiodindays;
            TermBrokerage500k = TermPremium500k * agreement.Brokerage / 100;

            ClientAgreementTerm termiltermoption500k = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit500k, TermExcess500k);
            termiltermoption500k.TermLimit = TermLimit500k;
            termiltermoption500k.Premium = TermPremium500k;
            termiltermoption500k.BasePremium = TermBasePremium500k;
            termiltermoption500k.Excess = TermExcess500k;
            termiltermoption500k.BrokerageRate = agreement.Brokerage;
            termiltermoption500k.Brokerage = TermBrokerage500k;
            termiltermoption500k.DateDeleted = null;
            termiltermoption500k.DeletedBy = null;

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0M;
            decimal TermBasePremium1mil = 0M;
            decimal TermBrokerage1mil = 0M;
            decimal TermExcess1mil = 2500;
            TermPremium1mil = rates["cl1millimitpremium"];
            TermBasePremium1mil = TermPremium1mil;
            TermPremium1mil = TermPremium1mil * agreementperiodindays / coverperiodindays;
            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

            ClientAgreementTerm termiltermoption1mil = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit1mil, TermExcess1mil);
            termiltermoption1mil.TermLimit = TermLimit1mil;
            termiltermoption1mil.Premium = TermPremium1mil;
            termiltermoption1mil.BasePremium = TermBasePremium1mil;
            termiltermoption1mil.Excess = TermExcess1mil;
            termiltermoption1mil.BrokerageRate = agreement.Brokerage;
            termiltermoption1mil.Brokerage = TermBrokerage1mil;
            termiltermoption1mil.DateDeleted = null;
            termiltermoption1mil.DeletedBy = null;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.Where(a => a.DateDeleted == null).
                    FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termiltermoption250k.PremiumDiffer = (TermPremium250k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termiltermoption250k.PremiumPre = PreviousBoundPremium;
                        if (termiltermoption250k.TermLimit == term.TermLimit && termiltermoption250k.Excess == term.Excess)
                        {
                            termiltermoption250k.Bound = true;
                        }
                        if (termiltermoption250k.PremiumDiffer < 0)
                        {
                            termiltermoption250k.PremiumDiffer = 0;
                        }
                        termiltermoption500k.PremiumDiffer = (TermPremium500k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termiltermoption500k.PremiumPre = PreviousBoundPremium;
                        if (termiltermoption500k.TermLimit == term.TermLimit && termiltermoption500k.Excess == term.Excess)
                        {
                            termiltermoption500k.Bound = true;
                        }
                        if (termiltermoption500k.PremiumDiffer < 0)
                        {
                            termiltermoption500k.PremiumDiffer = 0;
                        }
                        termiltermoption1mil.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termiltermoption1mil.PremiumPre = PreviousBoundPremium;
                        if (termiltermoption1mil.TermLimit == term.TermLimit && termiltermoption1mil.Excess == term.Excess)
                        {
                            termiltermoption1mil.Bound = true;
                        }
                        if (termiltermoption1mil.PremiumDiffer < 0)
                        {
                            termiltermoption1mil.PremiumDiffer = 0;
                        }
                    }
                }
            }

            //Referral points per agreement
            ////Not a renewal of an existing policy
            //uwrfnotrenewalcl(underwritingUser, agreement);
            //Cyber Issue
            uwrclissue(underwritingUser, agreement);


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            agreement.ProfessionalBusiness = "Barristers and other occupations agreed by NZI";
            string retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
            agreement.TerritoryLimit = "Worldwide excluding USA and Canada";
            agreement.Jurisdiction = "Worldwide excluding USA and Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZBar CL UW created/modified";
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
                    int newalgraceperiodindays = 0;
                    newalgraceperiodindays = programme.BaseProgramme.NewGracePriodInDays;
                    if (DateTime.UtcNow > product.DefaultInceptionDate.AddDays(newalgraceperiodindays))
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

        ClientAgreementTermExtension GetAgreementExtensionTerm(User CurrentUser, ClientAgreement agreement, int limitoption, decimal excessoption, decimal premiumoption, string extensionName)
        {
            ClientAgreementTermExtension extensionTerm = agreement.ClientAgreementTermExtensions.FirstOrDefault(tex => tex.DateDeleted != null && tex.ExtentionName == extensionName);

            if (extensionTerm == null)
            {
                extensionTerm = new ClientAgreementTermExtension(CurrentUser, limitoption, excessoption, premiumoption, agreement);
                agreement.ClientAgreementTermExtensions.Add(extensionTerm);
            }

            return extensionTerm;
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

        void uwrclissue(User underwritingUser, ClientAgreement agreement)
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
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasLocationOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasActivityOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasConfidencialOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasBreachesOptions").First().Value == "2" ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasKnowledgeOptions").First().Value == "2")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrclissue" && cref.DateDeleted == null).Status = "Pending";
                        }
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
