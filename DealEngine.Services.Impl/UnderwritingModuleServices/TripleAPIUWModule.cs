using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class TripleAPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public TripleAPIUWModule()
        {
            Name = "TripleA_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "piindividualpremium", "1millimitfappremium2advisor", "2millimitfappremium2advisor", "5millimitfappremium2advisor", 
                "1millimitfappremium3advisor", "2millimitfappremium3advisor", "5millimitfappremium3advisor", "1millimitfappremium4advisor", "2millimitfappremium4advisor", "5millimitfappremium4advisor", 
                "1millimitfappremium5advisor", "2millimitfappremium5advisor", "5millimitfappremium5advisor", "1millimitfappremium6advisor", "2millimitfappremium6advisor", "5millimitfappremium6advisor", 
                "1millimitfappremium7advisor", "2millimitfappremium7advisor", "5millimitfappremium7advisor", "1millimitfappremium8advisor", "2millimitfappremium8advisor", "5millimitfappremium8advisor", 
                "1millimitfappremium9advisor", "2millimitfappremium9advisor", "5millimitfappremium9advisor", "1millimitfappremium10advisor", "2millimitfappremium10advisor", "5millimitfappremium10advisor", 
                "1millimitfappremium11advisor", "2millimitfappremium11advisor", "5millimitfappremium11advisor");

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

            int fapagreementperiodindays = 0;
            fapagreementperiodindays = (agreement.ExpiryDate - Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value)).Days;

            decimal feeincome = 0;
            decimal decFGD = 0M;
            decimal decFGC = 0M;
            decimal decFG = 0M;
            decimal decOther = 0M;

            string strProfessionalBusiness = "Life and general advisers of any insurance or assurance company and/or intermediaries, agents or consultants in the sale or negotiation of any financial product or the provision of any financial advice including mortgage advice and financial services educational workshops.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0)
                {
                    feeincome = agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                }

                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUS0030") //Domestic Fire & General
                    {
                        decFGD = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0031") //Commercial Fire & General
                    {
                        decFGC = uISActivity.Percentage;
                    }
                    else if(uISActivity.AnzsciCode == "CUS0039") //Other 
                    {
                        decOther = uISActivity.Percentage;
                    }

                }

                decFG = decFGD + decFGC;
            }

            int intnumberofadvisors = 0;
            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                //foreach (var uisorg in agreement.ClientInformationSheet.Organisation.Where(o=>o.InsuranceAttributes.Any(i=>i.Name == "Advisor" || i.Name == "Nominated Representative")))
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    if (!uisorg.Removed)
                    {
                        var principleadvisorunit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => (u.Name == "Advisor" || u.Name == "Nominated Representative") && u.DateDeleted == null);

                        if (principleadvisorunit != null)
                        {
                            if (uisorg.DateDeleted == null && !uisorg.Removed)
                            {
                                intnumberofadvisors += 1;
                            }
                        }
                    }
                }
            }

            bool subuisreferred = false;
            if (agreement.ClientInformationSheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null).Count() > 0)
            {
                foreach (var subuis in agreement.ClientInformationSheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null))
                {
                    if ((subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasDamageLossOptions").First().Value == "1" ||
                        subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasWithdrawnOptions").First().Value == "1" ||
                        subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasRefusedOptions").First().Value == "1" ||
                        subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasStatutoryOffenceOptions").First().Value == "1" ||
                        subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasLiquidationOptions").First().Value == "1" ||
                        subuis.ClaimNotifications.Where(subacscn => subacscn.DateDeleted == null && (subacscn.ClaimStatus == "Settled" || subacscn.ClaimStatus == "Precautionary notification only" || subacscn.ClaimStatus == "Part Settled")).Count() > 0) &&
                        !subuisreferred)
                    {
                        subuisreferred = true;
                    }
                }
            }

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

            decimal decOtherInvetmentPerc = 0m;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "IPViewModel.HasClientFundsOptions").First().Value == "1")
            {
                var sss = agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "IPViewModel.OtherFunds");

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "IPViewModel.OtherFunds").Any())
                {
                    decOtherInvetmentPerc = Convert.ToDecimal(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "IPViewModel.OtherFunds").First().Value);
                }
            }


            int TermExcess = 0;
            TermExcess = 1000;

            decimal TermPremiumAdvisors = 0M;
            TermPremiumAdvisors = GetPremiumForAdvisors(rates, intnumberofadvisors, agreementperiodindays, coverperiodindays);

            if (intnumberofadvisors > 1)
            {
                int TermLimit1mil = 1000000;
                decimal TermPremium1mil = 0M;
                decimal TermPremium1milFAP = 0M;
                decimal TermBrokerage1mil = 0M;

                TermPremium1milFAP = GetPremiumForFAP(rates, feeincome, TermLimit1mil, intnumberofadvisors, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermPremium1mil = TermPremiumAdvisors + TermPremium1milFAP;
                TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

                ClientAgreementTerm term1millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess);
                term1millimitpremiumoption.TermLimit = TermLimit1mil;
                term1millimitpremiumoption.Premium = TermPremium1mil;
                term1millimitpremiumoption.BasePremium = TermPremium1mil;
                term1millimitpremiumoption.FAPPremium = TermPremium1milFAP;
                term1millimitpremiumoption.Excess = TermExcess;
                term1millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term1millimitpremiumoption.Brokerage = TermBrokerage1mil;
                term1millimitpremiumoption.DateDeleted = null;
                term1millimitpremiumoption.DeletedBy = null;


                int TermLimit2mil = 2000000;
                decimal TermPremium2mil = 0M;
                decimal TermPremium2milFAP = 0M;
                decimal TermBrokerage2mil = 0M;

                TermPremium2milFAP = GetPremiumForFAP(rates, feeincome, TermLimit2mil, intnumberofadvisors, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermPremium2mil = TermPremiumAdvisors + TermPremium2milFAP;
                TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;

                ClientAgreementTerm term2millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess);
                term2millimitpremiumoption.TermLimit = TermLimit2mil;
                term2millimitpremiumoption.Premium = TermPremium2mil;
                term2millimitpremiumoption.BasePremium = TermPremium2mil;
                term2millimitpremiumoption.FAPPremium = TermPremium2milFAP;
                term2millimitpremiumoption.Excess = TermExcess;
                term2millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term2millimitpremiumoption.Brokerage = TermBrokerage2mil;
                term2millimitpremiumoption.DateDeleted = null;
                term2millimitpremiumoption.DeletedBy = null;

                //Change policy premium calculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
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
                            term1millimitpremiumoption.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term1millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term1millimitpremiumoption.TermLimit == term.TermLimit && term1millimitpremiumoption.Excess == term.Excess)
                            {
                                term1millimitpremiumoption.Bound = true;
                            }
                            if (term1millimitpremiumoption.PremiumDiffer < 0)
                            {
                                term1millimitpremiumoption.PremiumDiffer = 0;
                            }
                            term2millimitpremiumoption.PremiumDiffer = (TermPremium2mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term2millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term2millimitpremiumoption.TermLimit == term.TermLimit && term2millimitpremiumoption.Excess == term.Excess)
                            {
                                term2millimitpremiumoption.Bound = true;
                            }
                            if (term2millimitpremiumoption.PremiumDiffer < 0)
                            {
                                term2millimitpremiumoption.PremiumDiffer = 0;
                            }
                        }

                    }
                }

            }
           
            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = 0M;
            decimal TermPremium5milFAP = 0M;
            decimal TermBrokerage5mil = 0M;

            TermPremium5milFAP = GetPremiumForFAP(rates, feeincome, TermLimit5mil, intnumberofadvisors, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
            TermPremium5mil = TermPremiumAdvisors + TermPremium5milFAP;
            TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess);
            term5millimitpremiumoption.TermLimit = TermLimit5mil;
            term5millimitpremiumoption.Premium = TermPremium5mil;
            term5millimitpremiumoption.BasePremium = TermPremium5mil;
            term5millimitpremiumoption.FAPPremium = TermPremium5milFAP;
            term5millimitpremiumoption.Excess = TermExcess;
            term5millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremiumoption.Brokerage = TermBrokerage5mil;
            term5millimitpremiumoption.DateDeleted = null;
            term5millimitpremiumoption.DeletedBy = null;


            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
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
                        term5millimitpremiumoption.PremiumDiffer = (TermPremium5mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimitpremiumoption.TermLimit == term.TermLimit && term5millimitpremiumoption.Excess == term.Excess)
                        {
                            term5millimitpremiumoption.Bound = true;
                        }
                        if (term5millimitpremiumoption.PremiumDiffer < 0)
                        {
                            term5millimitpremiumoption.PremiumDiffer = 0;
                        }
                    }

                }
            }


            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //F&G over 50%
            uwrffgactivitiesover50percent(underwritingUser, agreement, decFG);
            //Other Business Activities
            uwrfotheractivities(underwritingUser, agreement, decOther);
            //Other Investment Activities
            uwrfotherinvestmentactivity(underwritingUser, agreement, decOtherInvetmentPerc);
            //Advisor Claims / Insurance History
            uwrfadvisorpriorinsurance(underwritingUser, agreement, subuisreferred);
            //Custom Endorsement renew
            uwrfcustomendorsementrenew(underwritingUser, agreement, bolcustomendorsementrenew);
            //Advisor number over 11
            uwrfadvisornumberover11(underwritingUser, agreement, intnumberofadvisors);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Unlimited excluding known claims or circumstances";
            agreement.TerritoryLimit = "Australia and New Zealand";
            agreement.Jurisdiction = "Australia and New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "TripleA PI UW created/modified";
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

                if (DateTime.UtcNow > product.DefaultInceptionDate.AddMonths(1))
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


        decimal GetPremiumForAdvisors(IDictionary<string, decimal> rates, int intnumberofadvisors, int agreementperiodindays, int coverperiodindays)
        {
            decimal indadvisorpremium = 0M;
            decimal advisorpremiumoption = 0M;

            indadvisorpremium = rates["piindividualpremium"] * agreementperiodindays / coverperiodindays;

            if (intnumberofadvisors > 1)
            {
                indadvisorpremium *= intnumberofadvisors;

            }
            advisorpremiumoption = indadvisorpremium;
            return advisorpremiumoption;
        }

        decimal GetPremiumForFAP(IDictionary<string, decimal> rates, decimal feeincome, int limitoption, int intnumberofadvisors, int agreementperiodindays, int coverperiodindays, int fapagreementperiodindays)
        {
            decimal premiumoption = 0M;

            if (intnumberofadvisors > 1)
            {
                if (intnumberofadvisors == 2)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium2advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium2advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium2advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 3)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium3advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium3advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium3advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 4)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium4advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium4advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium4advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 5)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium5advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium5advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium5advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 6)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium6advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium6advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium6advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 7)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium7advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium7advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium7advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 8)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium8advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium8advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium8advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 9)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium9advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium9advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium9advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 10)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium10advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium10advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium10advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                if (intnumberofadvisors == 11)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                premiumoption = rates["1millimitfappremium11advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremium11advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremium11advisor"] * fapagreementperiodindays / coverperiodindays;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }

            }

            return premiumoption;
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").OrderNumber));
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
            }
        }


        void uwrffgactivitiesover50percent(User underwritingUser, ClientAgreement agreement, decimal decFG)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivitiesover50percent" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivitiesover50percent") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivitiesover50percent").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivitiesover50percent").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivitiesover50percent").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivitiesover50percent").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivitiesover50percent" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decFG > 50)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivitiesover50percent" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }

        void uwrfotheractivities(User underwritingUser, ClientAgreement agreement, decimal decOther)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decOther > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }

        void uwrfotherinvestmentactivity(User underwritingUser, ClientAgreement agreement, decimal decOtherInvetmentPerc)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherinvestmentactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherinvestmentactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decOtherInvetmentPerc > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherinvestmentactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }


        void uwrfadvisorpriorinsurance(User underwritingUser, ClientAgreement agreement, bool subuisreferred)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorpriorinsurance" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorpriorinsurance" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (subuisreferred)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorpriorinsurance" && cref.DateDeleted == null).Status = "Pending";
                    }
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew").OrderNumber));
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
            }
        }

        void uwrfadvisornumberover11(User underwritingUser, ClientAgreement agreement, int intnumberofadvisors)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover11" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover11") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover11").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover11").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover11").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover11").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover11" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (intnumberofadvisors > 11)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover11" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }


    }
}
