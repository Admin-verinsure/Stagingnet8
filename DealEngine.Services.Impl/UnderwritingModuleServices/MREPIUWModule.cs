using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class MREPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public MREPIUWModule()
        {
            Name = "MRE_PI";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PI" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm piterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PI" && ct.DateDeleted == null))
                {
                    piterm.Delete(underwritingUser);
                }
            }

            //IDictionary<string, decimal> rates = BuildRulesTable(agreement, "pi10millimit10kexcesspremium");

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

            bool bolmultichanges = false;
            int coverperiodindaysforchange = 0;
            bool bolinvalidchangeeffectivedate = false;
            bool bolinvalidchangeeffectivedatesubmission = false;
            //coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;
            int intchangePriodInDaysFromInception = 0;
            intchangePriodInDaysFromInception = agreement.ClientInformationSheet.Programme.BaseProgramme.ChangePriodInDaysFromInception;
            int intchangePriodInDaysToExpiry = 0;
            intchangePriodInDaysToExpiry = agreement.ClientInformationSheet.Programme.BaseProgramme.ChangePriodInDaysToExpiry * -1;
            int intchangePriodInDaysToSubmission = 7 * -1;
            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ChangePriodInDaysToSubmission > 7)
            {
                intchangePriodInDaysToSubmission = agreement.ClientInformationSheet.Programme.BaseProgramme.ChangePriodInDaysToSubmission * -1;
            }
            if (agreement.ClientInformationSheet.IsChange)
            {
                if (agreement.ClientInformationSheet.Programme.ChangeReason != null)
                {
                    if (agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate > DateTime.MinValue)
                    {
                        coverperiodindaysforchange = (agreement.ExpiryDate - agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate).Days;
                        if (agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate < agreement.InceptionDate.AddDays(intchangePriodInDaysFromInception) ||
                            agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate > agreement.ExpiryDate.AddDays(intchangePriodInDaysToExpiry))
                        {
                            bolinvalidchangeeffectivedate = true;
                        }
                        if (agreement.ClientInformationSheet.SubmitDate > DateTime.MinValue)
                        {
                            if (agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate < agreement.ClientInformationSheet.SubmitDate.AddDays(intchangePriodInDaysToSubmission))
                            {
                                bolinvalidchangeeffectivedatesubmission = true;
                            }
                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Programme.ChangeReason.EffectiveDate < DateTime.UtcNow.AddDays(intchangePriodInDaysToSubmission))
                            {
                                bolinvalidchangeeffectivedatesubmission = true;
                            }
                        }


                    }
                }
            }

            //For 1st year set up
            bool bolcustomendorsementrenew = false;
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
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "PI")
                    {
                        if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
                        {
                            bolcustomendorsementrenew = true;
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
            //    var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PI"));

            //    if (renewFromAgreement != null)
            //    {
            //        strretrodate = renewFromAgreement.RetroactiveDate;

            //        foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
            //        {

            //            if (renewendorsement.DateDeleted == null)
            //            {
            //                ClientAgreementEndorsement newclientendorsement =
            //                    new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
            //                agreement.ClientAgreementEndorsements.Add(newclientendorsement);
            //            }
            //        }
            //    }


            //}

            //Endorsement

            if (agreement.ClientAgreementEndorsements != null)
            {
                foreach (var endorsement in agreement.ClientAgreementEndorsements.Where(endo => endo.DateDeleted == null))
                {
                    if (endorsement.Name == "Deemed Subsidiary Endorsement" || endorsement.Name == "Run Off Endorsement")
                    {
                        endorsement.DateDeleted = DateTime.UtcNow;
                        endorsement.DeletedBy = underwritingUser;
                    }
                }
            }

            string SubsidiaryOrg = "";
            string RunOffOrg = "";
            string RunOffEff = "";
            string SubsidiaryEndorsementTxt = "";
            string RunOffEndorsementTxt = "";
            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    if (!uisorg.Removed && uisorg.DateDeleted == null)
                    {
                        var unit = (SubsidiaryUnit)uisorg.OrganisationalUnits.FirstOrDefault(o => o.Name == "Subsidiary Company organisation");
                        if (unit != null)
                        {
                            if (string.IsNullOrEmpty(SubsidiaryOrg))
                            {
                                SubsidiaryOrg += uisorg.Name;
                            }
                            else
                            {
                                SubsidiaryOrg += ", " + uisorg.Name;
                            }

                            SubsidiaryEndorsementTxt = "Subsidiary means any entity of which the Policyholder has Control either directly or indirectly through one or more other entities on or before the inception date of this policy.In addition, the following named entity or entities are also deemed to be a Subsidiary for the purposes of this policy: " + SubsidiaryOrg + ". <br/ > All other terms, conditions and exclusions remain unchanged.";
                            ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, "Deemed Subsidiary Endorsement", "Exclusion", product, SubsidiaryEndorsementTxt, 110, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
                        }
                        var unit2 = (RealEstateRunOffUnit)uisorg.OrganisationalUnits.FirstOrDefault(o => o.Name == "Run Off");
                        if (unit2 != null)
                        {
                            if (string.IsNullOrEmpty(RunOffOrg))
                            {
                                RunOffOrg += uisorg.Name;
                            }
                            else
                            {
                                RunOffOrg += ", " + uisorg.Name;
                            }
                            if (string.IsNullOrEmpty(RunOffEff))
                            {
                                RunOffEff += unit2.EffectiveDate;
                            }
                            else
                            {
                                RunOffEff += ", " + unit2.EffectiveDate;
                            }
                            RunOffEndorsementTxt = "It is hereby declared and agreed that the policy shall not indemnify the " + RunOffOrg + " against any Claim or loss arising from any civil liability committed or alleged to have been committed on the part of the Insured in or about the conduct of any Professional Business after " + RunOffEff + ". <br/ >All other terms, conditions and exclusions remain unchanged.";
                            ClientAgreementEndorsement clientAgreementEndorsement1 = new ClientAgreementEndorsement(underwritingUser, "Run Off Endorsement", "Exclusion", product, RunOffEndorsementTxt, 111, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement1);
                        }
                    }
                }
            }


            //Calculate premium option

            int TermLimit = 0;
            decimal TermPremium = 0M;
            decimal TermBasePremium = 0M;
            decimal TermBrokerage = 0M;
            decimal TermExcess = 0;

            TermPremium = TermPremium * agreementperiodindays / coverperiodindays;
            TermBrokerage = TermPremium * agreement.Brokerage / 100;

            ClientAgreementTerm termpitermoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit, TermExcess);
            termpitermoption.TermLimit = TermLimit;
            termpitermoption.Premium = TermPremium;
            termpitermoption.BasePremium = TermBasePremium;
            termpitermoption.Excess = TermExcess;
            termpitermoption.BrokerageRate = agreement.Brokerage;
            termpitermoption.Brokerage = TermBrokerage;
            termpitermoption.DateDeleted = null;
            termpitermoption.DeletedBy = null;

            ////add Costs & Expenses extension
            //foreach (ClientAgreementTermExtension pitermextension in agreement.ClientAgreementTermExtensions.Where(ctex => ctex.DateDeleted == null))
            //{
            //    pitermextension.Delete(underwritingUser);
            //}
            //ClientAgreementTermExtension termPICEextension = GetAgreementExtensionTerm(underwritingUser, agreement, 500000, 0M, 0M, "Professional Indemnity – Costs & Expenses");
            //termPICEextension.ExtentionName = "Professional Indemnity – Costs & Expenses";
            //termPICEextension.HideLimitExcess = false;
            //termPICEextension.Premium = 0M;
            //termPICEextension.BasePremium = 0M;
            //termPICEextension.DateDeleted = null;
            //termPICEextension.DeletedBy = null;
            //termPICEextension.Bound = true;


            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                //set admin fee $0
                agreement.BrokerFee = 0M;

                if (agreement.ClientInformationSheet.PreviousInformationSheet.IsChange)
                {
                    bolmultichanges = true;
                }

                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PI"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termpitermoption.PremiumDiffer = (TermPremium - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpitermoption.PremiumPre = PreviousBoundPremium;
                        if (termpitermoption.TermLimit == term.TermLimit && termpitermoption.Excess == term.Excess)
                        {
                            termpitermoption.Bound = true;
                        }
                    }

                }

                strretrodate = PreviousAgreement.RetroactiveDate;

                foreach (var renewendorsement in PreviousAgreement.ClientAgreementEndorsements)
                {

                    if (renewendorsement.DateDeleted == null && renewendorsement.Name != "Deemed Subsidiary Endorsement" && renewendorsement.Name != "Run Off Endorsement")
                    {
                        ClientAgreementEndorsement newclientendorsement =
                            new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                        agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                    }
                }
            }

            //Referral points per agreement
            ////Claims / Insurance History
            //uwrfpriorinsurance(underwritingUser, agreement);
            ////Custom Endorsement renew
            //uwrfcustomendorsementrenew(underwritingUser, agreement, bolcustomendorsementrenew);

            //if (informationSheet.IsChange) //change agreement referrals
            //{
            //    //Change effective date entered prior the inception date and after the expiry date
            //    uwrfchangeeffectivedate(underwritingUser, agreement, bolinvalidchangeeffectivedate);
            //    //Change effective date entered prior the submission date
            //    uwrfchangeeffectivedatesubmission(underwritingUser, agreement, bolinvalidchangeeffectivedatesubmission);
            //    //Multiple changes
            //    uwrfmultichanges(underwritingUser, agreement, bolmultichanges);
            //}

            //Update agreement Status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            //Update agreement Name of Insured
            agreement.InsuredName = informationSheet.Owner.Name;

            //Update agreement Professional Business, Retroactive Date, TerritoryLimit, Jurisdiction
            agreement.ProfessionalBusiness = "Licensed Real Estate Agents, Property Managers, Registered Valuers, Licensed Auctioneers and other activities as disclosed in the proposal form completed on behalf of the firm or as subsequently advised to the Insurers in writing and the payment of any additional Premium required.";
            string retrodate = "Unlimited";
            agreement.TerritoryLimit = "Anywhere in NZ";
            agreement.Jurisdiction = "Anywhere in NZ";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgMultiPolicyMode)
            {
                agreement.ClientInformationSheet.Programme.ClientProgrammeInceptionDate = agreement.InceptionDate;
                agreement.ClientInformationSheet.Programme.ClientProgrammeExpiryDate = agreement.ExpiryDate;
            }

            //Create agreement audit log
            string auditLogDetail = "MRE PI UW created/modified";
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

                ////Inception date rule (turned on after implementing change, any remaining policy and new policy will use submission date as inception date)
                //if (informationSheet.IsRenewawl)
                //{
                //    int renewalgraceperiodindays = 0;
                //    renewalgraceperiodindays = programme.BaseProgramme.RenewGracePriodInDays;
                //    if (DateTime.UtcNow > product.DefaultInceptionDate.AddDays(renewalgraceperiodindays))
                //    {
                //        inceptionDate = DateTime.UtcNow;
                //    }
                //}
                //else
                //{
                //    int newalgraceperiodindays = 0;
                //    newalgraceperiodindays = programme.BaseProgramme.NewGracePriodInDays;
                //    if (DateTime.UtcNow > product.DefaultInceptionDate.AddDays(newalgraceperiodindays))
                //    {
                //        inceptionDate = DateTime.UtcNow;
                //    }
                //}

                //inception date and expiry date from uis data
                if (informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").Any())
                {
                    inceptionDate = Convert.ToDateTime(informationSheet.Answers.Where(sa => sa.ItemName == "GeneralViewModel.PolicyStartDate").First().Value);
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

        //Add the Costs & Expenses extension
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

        void uwrfpriorinsurance(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasDamageLossOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasWithdrawnOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasRefusedOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasStatutoryOffenceOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasLiquidationOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.ClaimNotifications.Where(acscn => acscn.DateDeleted == null && !acscn.Removed && (acscn.ClaimStatus == "Settled" || acscn.ClaimStatus == "Precautionary notification only" || acscn.ClaimStatus == "Part Settled")).Count() > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfcustomendorsementrenew(User underwritingUser, ClientAgreement agreement, bool bolcustomendorsementrenew)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolcustomendorsementrenew) //Custom Endorsement Renew
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfchangeeffectivedate(User underwritingUser, ClientAgreement agreement, bool bolinvalidchangeeffectivedate)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedate" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedate").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedate" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolinvalidchangeeffectivedate) //Change effective date
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedate" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedate" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedate" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfchangeeffectivedatesubmission(User underwritingUser, ClientAgreement agreement, bool bolinvalidchangeeffectivedatesubmission)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedatesubmission" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfchangeeffectivedatesubmission").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedatesubmission" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolinvalidchangeeffectivedatesubmission) //Change effective date
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedatesubmission" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedatesubmission" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfchangeeffectivedatesubmission" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfmultichanges(User underwritingUser, ClientAgreement agreement, bool bolmultichanges)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmultichanges" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmultichanges").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmultichanges" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolmultichanges) //Multiple changes
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmultichanges" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmultichanges" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmultichanges" && cref.DateDeleted == null).Status = "";
                }
            }
        }


    }
}

