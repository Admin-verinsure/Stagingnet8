using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZFSGDOUWModule2021 : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZFSGDOUWModule2021()
        {
            Name = "NZFSG_DO_2021";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "dotermlimit", "dotermexcess", "dotermpremiumclass1", "dotermpremiumclass1noemp", "maximumassetsize");

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
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.DORetro))
            //            {
            //                strretrodate = preRenewOrRefData.DORetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "DO")
            //        {
            //            if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
            //            {
            //                ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
            //                agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
            //            }
            //        }
            //    }
            //}

            //renewal data (retro date and endorsements)
            string strretrodate = "";
            if (agreement.ClientInformationSheet.IsRenewawl && agreement.ClientInformationSheet.RenewFromInformationSheet != null)
            {
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "DO"));

                if (renewFromAgreement != null)
                {
                    strretrodate = renewFromAgreement.RetroactiveDate;

                    foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
                    {

                        if (renewendorsement.DateDeleted == null && renewendorsement.Name != "Insolvency Exclusion")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }

            }

            //Programme specific term

            //Default Professional Business, Retroactive Date, TerritoryLimit, Jurisdiction, AuditLog Detail
            string strProfessionalBusiness = "";
            string retrodate = "";
            string strTerritoryLimit = "";
            string strJurisdiction = "";
            string auditLogDetail = "";

            if (agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo ML Programme")
            {
                strProfessionalBusiness = "Financial Advice Provider – in the provision of Life & Health Insurance, Investment Advice, Mortgage Broking and Fire & General Broking.";
                retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
                strTerritoryLimit = "Worldwide";
                strJurisdiction = "New Zealand";
                auditLogDetail = "Apollo DO UW created/modified";
            }
            else if (agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG Programme" ||
               agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG ML Programme")
            {
                strProfessionalBusiness = "Financial Advice Provider – in the provision of Life & Health Insurance, Mortgage Broking and Fire & General Broking.";
                retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
                strTerritoryLimit = "Worldwide";
                strJurisdiction = "New Zealand";
                auditLogDetail = "NZFSG DO UW created/modified";

            }


            decimal decDOTotalAssets = 0m;
            decimal decDOTotalLiabilities = 0m;
            decimal decDOCurrentAssets = 0m;
            decimal decDOCurrentLiabilities = 0m;
            decimal decDOAftertaxProfitOrLoss = 0m;

            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetTotal").Any())
            {
                decDOTotalAssets = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetTotal").First().Value);
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.LiabilityTotal").Any())
            {
                decDOTotalLiabilities = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.LiabilityTotal").First().Value);
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetCurrent").Any())
            {
                decDOCurrentAssets = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AssetCurrent").First().Value);
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.LiabilityCurrent").Any())
            {
                decDOCurrentLiabilities = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.LiabilityCurrent").First().Value);
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AfterTaxNumber").Any())
            {
                decDOAftertaxProfitOrLoss = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.AfterTaxNumber").First().Value);
            }

            ClientAgreementEndorsement cAEDOInsExcl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Insolvency Exclusion");
            if (cAEDOInsExcl != null)
            {
                cAEDOInsExcl.DateDeleted = DateTime.UtcNow;
                cAEDOInsExcl.DeletedBy = underwritingUser;
            }

            if (agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG ML Programme")
            {
                if (decDOTotalAssets <= 0 && decDOTotalLiabilities <= 0 && decDOCurrentAssets <= 0 && decDOCurrentLiabilities <= 0 && decDOAftertaxProfitOrLoss <= 0)
                {
                    if (cAEDOInsExcl != null)
                    {
                        cAEDOInsExcl.DateDeleted = null;
                        cAEDOInsExcl.DeletedBy = null;
                    }
                }
            }
            if (agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo ML Programme")
            {
                int intCompanyAge = 0;
                if (agreement.ClientInformationSheet.Owner.DateofIncorportation.Value != null)
                {
                    intCompanyAge = DateTime.Now.Subtract(agreement.ClientInformationSheet.Owner.DateofIncorportation.Value).Days;
                }

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDebtsOptions").First().Value == "2" || 
                    (intCompanyAge < 1095))
                {
                    if (cAEDOInsExcl != null)
                    {
                        cAEDOInsExcl.DateDeleted = null;
                        cAEDOInsExcl.DeletedBy = null;
                    }
                }
            }



            int TermLimit = 0;
            decimal TermPremium = 0M;
            decimal TermBasePremium = 0M;
            decimal TermBrokerage = 0M;
            decimal TermExcess = 0M;
            TermLimit = Convert.ToInt32(rates["dotermlimit"]);
            TermExcess = rates["dotermexcess"];

            //Check class information to calculate the premium
            bool bolclass2referral = false;
            bool bolclass3referral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "1")
                {
                    TermPremium = rates["dotermpremiumclass1noemp"];
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.HaveAnyEmployeeYN").First().Value == "1" &&
                        Convert.ToInt32(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.TotalEmployees").First().Value) > 1)
                    {
                        TermPremium = rates["dotermpremiumclass1"];
                    }
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "2")
                {
                    bolclass2referral = true;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "3")
                {
                    bolclass3referral = true;
                }
            }

            TermBasePremium = TermPremium;
            TermPremium = TermPremium * agreementperiodindays / coverperiodindays;
            TermBrokerage = TermPremium * agreement.Brokerage / 100;

            ClientAgreementTerm termdotermoption = GetAgreementTerm(underwritingUser, agreement, "DO", TermLimit, TermExcess);
            termdotermoption.TermLimit = TermLimit;
            termdotermoption.Premium = TermPremium;
            termdotermoption.BasePremium = TermBasePremium;
            termdotermoption.Excess = TermExcess;
            termdotermoption.BrokerageRate = agreement.Brokerage;
            termdotermoption.Brokerage = TermBrokerage;
            termdotermoption.DateDeleted = null;
            termdotermoption.DeletedBy = null;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.Where(a => a.DateDeleted == null).
                    FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "DO"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termdotermoption.PremiumDiffer = (TermPremium - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termdotermoption.PremiumPre = PreviousBoundPremium;
                        if (termdotermoption.TermLimit == term.TermLimit && termdotermoption.Excess == term.Excess)
                        {
                            termdotermoption.Bound = true;
                        }
                        if (termdotermoption.PremiumDiffer < 0)
                        {
                            termdotermoption.PremiumDiffer = 0;
                        }
                    }
                }
            }


            //Referral points per agreement
            //D&O Issues
            uwrdoissue(underwritingUser, agreement, decDOTotalAssets, decDOTotalLiabilities, decDOCurrentAssets, decDOCurrentLiabilities, decDOAftertaxProfitOrLoss);
            //Class 2 referral
            //uwrfclass2(underwritingUser, agreement, bolclass2referral);
            //Class 3 referral
            uwrfclass3(underwritingUser, agreement, bolclass3referral);
            //Asset Size
            uwrfassetsize(underwritingUser, agreement, rates);

            //Update agreement status
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
            agreement.ProfessionalBusiness = strProfessionalBusiness;
            agreement.RetroactiveDate = retrodate;
            agreement.TerritoryLimit = strTerritoryLimit;
            agreement.Jurisdiction = strJurisdiction;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            //Create agreement audit log
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

        void uwrdoissue(User underwritingUser, ClientAgreement agreement, decimal decDOTotalAssets, decimal decDOTotalLiabilities, decimal decDOCurrentAssets, decimal decDOCurrentLiabilities, decimal decDOAftertaxProfitOrLoss)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrdoissue").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDebtsOptions").First().Value == "2")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrdoissue" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfclass2(User underwritingUser, ClientAgreement agreement, bool bolclass2referral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass2" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass2").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass2" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolclass2referral) //Class 2 referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass2" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass2" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass2" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfclass3(User underwritingUser, ClientAgreement agreement, bool bolclass3referral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass3" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclass3").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass3" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolclass3referral) //Class 3 referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass3" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass3" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclass3" && cref.DateDeleted == null).Status = "";
                }
            }
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfassetsize").DoNotCheckForRenew));
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

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfassetsize" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfassetsize" && cref.DateDeleted == null).Status = "";
                }
            }
        }

    }
}