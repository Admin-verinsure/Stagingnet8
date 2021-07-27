using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class FANZRunOffUWModule2021 : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public FANZRunOffUWModule2021()
        {
            Name = "FANZ_PIRO_2021";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PIRO" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm piroterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PIRO" && ct.DateDeleted == null))
                {
                    piroterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "pirotermlimit", "pirotermexcess", "pirotermpremium", "piroterm1yearrate", 
                "piroterm2yearrate", "piroterm3yearrate", "piroterm4yearrate", "pirotermminimumpremium");

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


            //Programme specific term

            //Default Professional Business, Retroactive Date, TerritoryLimit, Jurisdiction, AuditLog Detail
            string strProfessionalBusiness = "Financial Advice Provider – in the provision of Life & Health Insurance, Investment Advice, Mortgage Broking, Financial Planning and Fire & General Broking (Please note Fire & General broking cover is restricted to insureds who derive income below -5% of total Turnover or $125,000 whichever is the lesser from this activity).";
            string retrodate = "Unlimited excluding known claims or circumstances";
            string strTerritoryLimit = "New Zealand";
            string strJurisdiction = "New Zealand";
            string auditLogDetail = "FANZ PI Run Off UW created/modified";

            //Update retro date and endorsement based on the pre-renewal data or renewal agreement

            int intrenewalpitermlimit = 0;
            decimal decrenewalpitermexcess = 0M;
            decimal decrenewalpitermpremium = 0M;
            bool bolcustomendorsementrenew = false;
            bool bolnorenewterm = true;
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
                        if (!string.IsNullOrEmpty(preRenewOrRefData.PIBoundLimit))
                        {
                            bolnorenewterm = false;
                            intrenewalpitermlimit = Convert.ToInt32(preRenewOrRefData.PIBoundLimit);
                        }
                        if (!string.IsNullOrEmpty(preRenewOrRefData.PIBoundExcess))
                        {
                            bolnorenewterm = false;
                            decrenewalpitermexcess = Convert.ToDecimal(preRenewOrRefData.PIBoundExcess);
                        }
                        if (!string.IsNullOrEmpty(preRenewOrRefData.PIBoundPremium))
                        {
                            bolnorenewterm = false;
                            decrenewalpitermpremium = Convert.ToDecimal(preRenewOrRefData.PIBoundPremium);
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "PIRO")
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


            //Check year cover required
            int intyearcover = 0;
            bool bolyearcoverreferral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "1")
                {
                    intyearcover = 1;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "2")
                {
                    intyearcover = 2;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "3")
                {
                    intyearcover = 3;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "4")
                {
                    intyearcover = 4;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "5")
                {
                    intyearcover = 5;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "6")
                {
                    intyearcover = 6;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.YearCover").First().Value == "7")
                {
                    intyearcover = 7;
                }
            }
            if (intyearcover != 1)
            {
                bolyearcoverreferral = true;
            }

            decimal TermExcess = 0M;
            int TermLimit = 0;
            decimal TermPremium = 0M;
            decimal TermMinimumPremium = 0M;
            decimal TermBasePremium = 0M;
            decimal TermBrokerage = 0M;
            decimal decyear1premium = 0M;
            decimal decyear2premium = 0M;
            decimal decyear3premium = 0M;
            decimal decyear4premium = 0M;

            TermLimit = Convert.ToInt32(rates["pirotermlimit"]);
            TermExcess = rates["pirotermexcess"];
            TermPremium = rates["pirotermpremium"];
            TermMinimumPremium = rates["pirotermminimumpremium"];

            if (intrenewalpitermlimit > 0)
                TermLimit = intrenewalpitermlimit;
            if (decrenewalpitermexcess > 0)
                TermExcess = decrenewalpitermexcess;
            if (decrenewalpitermpremium > 0)
            {
                decyear1premium = decrenewalpitermpremium * rates["piroterm1yearrate"] / 100;
                decyear2premium = decyear1premium * rates["piroterm2yearrate"] / 100;
                decyear3premium = decyear2premium * rates["piroterm3yearrate"] / 100;
                decyear4premium = decyear3premium * rates["piroterm4yearrate"] / 100;

                if (intyearcover == 1)
                {
                    TermPremium = decyear1premium;
                    TermPremium = (TermPremium > TermMinimumPremium) ? TermPremium : TermMinimumPremium;
                } 
                //else if (intyearcover == 2)
                //{
                //    TermPremium = decyear1premium + decyear2premium;
                //}
                //else if (intyearcover == 3)
                //{
                //    TermPremium = decyear1premium + decyear2premium + decyear3premium;
                //}
                //else if (intyearcover == 4)
                //{
                //    TermPremium = decyear1premium + decyear2premium + decyear3premium + decyear4premium;
                //}
            }

            TermBasePremium = TermPremium;
            TermPremium = TermPremium * agreementperiodindays / coverperiodindays;
            TermBrokerage = TermPremium * agreement.Brokerage / 100;

            ClientAgreementTerm termlimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIRO", TermLimit, TermExcess);
            termlimitpremiumoption.TermLimit = TermLimit;
            termlimitpremiumoption.Premium = TermPremium;
            termlimitpremiumoption.BasePremium = TermBasePremium;
            termlimitpremiumoption.Excess = TermExcess;
            termlimitpremiumoption.BrokerageRate = agreement.Brokerage;
            termlimitpremiumoption.Brokerage = TermBrokerage;
            termlimitpremiumoption.DateDeleted = null;
            termlimitpremiumoption.DeletedBy = null;


            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PIRO"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termlimitpremiumoption.PremiumDiffer = (TermBasePremium - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termlimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (termlimitpremiumoption.TermLimit == term.TermLimit && termlimitpremiumoption.Excess == term.Excess)
                        {
                            termlimitpremiumoption.Bound = true;
                        }
                        if (termlimitpremiumoption.PremiumDiffer < 0)
                        {
                            termlimitpremiumoption.PremiumDiffer = 0;
                        }
                    }

                }
            }


            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //Requires cover over 1 year
            uwrfyearcoverover1year(underwritingUser, agreement, bolyearcoverreferral);
            //No PI renew term
            uwrfnopirenewterm(underwritingUser, agreement, bolnorenewterm);

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
                    if (agreement.ClientInformationSheet.ClaimNotifications.Where(acscn => acscn.DateDeleted == null && !acscn.Removed && (acscn.ClaimStatus == "Settled" || acscn.ClaimStatus == "Precautionary notification only" || acscn.ClaimStatus == "Part Settled")).Count() > 0)
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

        void uwrfyearcoverover1year(User underwritingUser, ClientAgreement agreement, bool bolyearcoverreferral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearcoverover1year" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearcoverover1year").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearcoverover1year" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolyearcoverreferral)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearcoverover1year" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearcoverover1year" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearcoverover1year" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfnopirenewterm(User underwritingUser, ClientAgreement agreement, bool bolnorenewterm)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnopirenewterm" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnopirenewterm").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnopirenewterm" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolnorenewterm)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnopirenewterm" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnopirenewterm" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnopirenewterm" && cref.DateDeleted == null).Status = "";
                }
            }
        }


    }
}



