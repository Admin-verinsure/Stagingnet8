using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class AbbottPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public AbbottPIUWModule()
        {
            Name = "Abbott_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "2millimitpremiuminv", "3millimitpremiuminv", "5millimitpremiuminv", 
                "2millimitpremiummb", "3millimitpremiummb", "5millimitpremiummb", "2millimitpremium", "3millimitpremium", "5millimitpremium");

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

            //Abbott programme is not a full year policy
            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddMonths(-9).AddDays(1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            decimal feeincomelastyear = 0M;
            decimal FGCfeeincomelastyear = 0M;
            decimal FGDfeeincomelastyear = 0M;
            decimal FGfeeincomelastyear = 0M;
            decimal decFGD = 0M;
            decimal decFGC = 0M;
            decimal decFP = 0M;
            decimal decIP = 0M;
            decimal decSB = 0M;
            decimal decDPI = 0M;
            decimal decMB = 0M;
            decimal decOther = 0M;
            decimal decInv = 0M;
            bool bolworkoutsidenz = false;

            string strProfessionalBusiness = "Sales & Promotion of Life, Investment & General Insurance products, Financial planning & Mortgage Brokering Services";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincomelastyear = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                }

                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (!bolworkoutsidenz && uISTerritory.Location != "New Zealand" && uISTerritory.Percentage > 0) //Work outside New Zealand Check
                    {
                        bolworkoutsidenz = true;
                    }
                }

                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUS0065") //Fire & General Domestic
                    {
                        decFGD = uISActivity.Percentage;
                        FGDfeeincomelastyear = feeincomelastyear * uISActivity.Percentage / 100;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0066") //Fire & General Commercial
                    {
                        decFGC = uISActivity.Percentage;
                        FGCfeeincomelastyear = feeincomelastyear * uISActivity.Percentage / 100;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0070") //Preparation of Financial Plans and/or monitoring of Financial Portfolios
                    {
                        decFP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0071") //Investment Products (eg Unit Trusts, Bonds)
                    {
                        decIP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0072") //Sharebroking 
                    {
                        decSB = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0074") //Direct Property Investment 
                    {
                        decDPI = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0067") //Mortgage Broking (residential)
                    {
                        decMB = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0076") //Other 
                    {
                        decOther = uISActivity.Percentage;
                    }

                }

                decInv = decFP + decIP + decSB;
                FGfeeincomelastyear = FGDfeeincomelastyear + FGCfeeincomelastyear;
            }


            int intnumberofadvisors = 0;
            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    var principleadvisorunit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => (u.Name == "Advisor") && u.DateDeleted == null);

                    if (principleadvisorunit != null)
                    {
                        if (uisorg.DateDeleted == null && !uisorg.Removed)
                        {
                            intnumberofadvisors += 1;
                        }
                        if (agreement.ClientInformationSheet.IsChange && uisorg.OrgBeenMoved && uisorg.DateDeleted == null)
                        {
                            intnumberofadvisors -= 1;
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

            bool trustservices = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "OTViewModel.HasInvolvedTrusteeOptions").First().Value == "1")
            {
                trustservices = true;
            }

            int TermExcess1k = 1000;

            int TermLimit2mil1kExcess = 2000000;
            decimal TermPremium2mil1kExcess = 0M;
            decimal TermBrokerage2mil1kExcess = 0M;
            TermPremium2mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit2mil1kExcess, TermExcess1k, decInv, decMB);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium2mil1kExcess = TermPremium2mil1kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage2mil1kExcess = TermPremium2mil1kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil1kExcess, TermExcess1k);
            term2millimit1kexcesspremiumoption.TermLimit = TermLimit2mil1kExcess;
            term2millimit1kexcesspremiumoption.Premium = TermPremium2mil1kExcess;
            term2millimit1kexcesspremiumoption.BasePremium = TermPremium2mil1kExcess;
            term2millimit1kexcesspremiumoption.Excess = TermExcess1k;
            term2millimit1kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term2millimit1kexcesspremiumoption.Brokerage = TermBrokerage2mil1kExcess;
            term2millimit1kexcesspremiumoption.DateDeleted = null;
            term2millimit1kexcesspremiumoption.DeletedBy = null;


            int TermLimit3mil1kExcess = 3000000;
            decimal TermPremium3mil1kExcess = 0M;
            decimal TermBrokerage3mil1kExcess = 0M;
            TermPremium3mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit3mil1kExcess, TermExcess1k, decInv, decMB);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium3mil1kExcess = TermPremium3mil1kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage3mil1kExcess = TermPremium3mil1kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term3millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil1kExcess, TermExcess1k);
            term3millimit1kexcesspremiumoption.TermLimit = TermLimit3mil1kExcess;
            term3millimit1kexcesspremiumoption.Premium = TermPremium3mil1kExcess;
            term3millimit1kexcesspremiumoption.BasePremium = TermPremium3mil1kExcess;
            term3millimit1kexcesspremiumoption.Excess = TermExcess1k;
            term3millimit1kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term3millimit1kexcesspremiumoption.Brokerage = TermBrokerage3mil1kExcess;
            term3millimit1kexcesspremiumoption.DateDeleted = null;
            term3millimit1kexcesspremiumoption.DeletedBy = null;


            int TermLimit5mil1kExcess = 5000000;
            decimal TermPremium5mil1kExcess = 0M;
            decimal TermBrokerage5mil1kExcess = 0M;
            TermPremium5mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit5mil1kExcess, TermExcess1k, decInv, decMB);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium5mil1kExcess = TermPremium5mil1kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage5mil1kExcess = TermPremium5mil1kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil1kExcess, TermExcess1k);
            term5millimit1kexcesspremiumoption.TermLimit = TermLimit5mil1kExcess;
            term5millimit1kexcesspremiumoption.Premium = TermPremium5mil1kExcess;
            term5millimit1kexcesspremiumoption.BasePremium = TermPremium5mil1kExcess;
            term5millimit1kexcesspremiumoption.Excess = TermExcess1k;
            term5millimit1kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimit1kexcesspremiumoption.Brokerage = TermBrokerage5mil1kExcess;
            term5millimit1kexcesspremiumoption.DateDeleted = null;
            term5millimit1kexcesspremiumoption.DeletedBy = null;

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
                        term2millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium2mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term2millimit1kexcesspremiumoption.Excess == term.Excess)
                        {
                            term2millimit1kexcesspremiumoption.Bound = true;
                        }
                        if (term2millimit1kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term2millimit1kexcesspremiumoption.PremiumDiffer = 0;
                        }
                        term3millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium3mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term3millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term3millimit1kexcesspremiumoption.Excess == term.Excess)
                        {
                            term3millimit1kexcesspremiumoption.Bound = true;
                        }
                        if (term3millimit1kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term3millimit1kexcesspremiumoption.PremiumDiffer = 0;
                        }
                        term5millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium5mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term5millimit1kexcesspremiumoption.Excess == term.Excess)
                        {
                            term5millimit1kexcesspremiumoption.Bound = true;
                        }
                        if (term5millimit1kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term5millimit1kexcesspremiumoption.PremiumDiffer = 0;
                        }
                    }

                }
            }


            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //Other Business Activities
            uwrfotheractivities(underwritingUser, agreement, decOther);
            //Sharebroking business activity over 10%
            uwrfsbactivitiesover10percent(underwritingUser, agreement, decSB);
            //Other Investment Activities
            uwrfotherinvestmentactivity(underwritingUser, agreement, decOtherInvetmentPerc);
            //Advisor Claims / Insurance History
            uwrfadvisorpriorinsurance(underwritingUser, agreement, subuisreferred);
            //F&G income over $50,000
            uwrffgincomeover50k(underwritingUser, agreement, FGfeeincomelastyear);
            //Trust Services
            uwrftrustservices(underwritingUser, agreement, trustservices);
            //Operates Outside of NZ
            uwrfoperatesoutsideofnz(underwritingUser, agreement, bolworkoutsidenz);


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Unlimited excluding known claims and circumstances";
            agreement.TerritoryLimit = "Australia and New Zealand";
            agreement.Jurisdiction = "Australia and New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Abbott PI UW created/modified";
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


        decimal GetPremiumForAdvisors(IDictionary<string, decimal> rates, int intnumberofadvisors, int limitoption, int excessoption, decimal decInv, decimal decMB)
        {
            decimal premiumoption = 0M;


            if (decInv > 0)
            {
                switch (limitoption)
                {
                    case 2000000:
                        {
                            premiumoption = rates["2millimitpremiuminv"];
                            break;
                        }
                    case 3000000:
                        {
                            premiumoption = rates["3millimitpremiuminv"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoption = rates["5millimitpremiuminv"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            } else if (decMB >= 60)
            {
                switch (limitoption)
                {
                    case 2000000:
                        {
                            premiumoption = rates["2millimitpremiummb"];
                            break;
                        }
                    case 3000000:
                        {
                            premiumoption = rates["3millimitpremiummb"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoption = rates["5millimitpremiummb"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }
            else
            {
                switch (limitoption)
                {
                    case 2000000:
                        {
                            premiumoption = rates["2millimitpremium"];
                            break;
                        }
                    case 3000000:
                        {
                            premiumoption = rates["3millimitpremium"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoption = rates["5millimitpremium"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }

            premiumoption *= intnumberofadvisors;

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


        void uwrfotheractivities(User underwritingUser, ClientAgreement agreement, decimal decOther)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivities").DoNotCheckForRenew));
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

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivities" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfsbactivitiesover10percent(User underwritingUser, ClientAgreement agreement, decimal decSB)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decSB > 10)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null).Status = "";
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherinvestmentactivity").DoNotCheckForRenew));
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

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherinvestmentactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherinvestmentactivity" && cref.DateDeleted == null).Status = "";
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorpriorinsurance").DoNotCheckForRenew));
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

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorpriorinsurance" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorpriorinsurance" && cref.DateDeleted == null).Status = "";
                }
            }
        }


        void uwrffgincomeover50k(User underwritingUser, ClientAgreement agreement, decimal FGfeeincomelastyear)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover50k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover50k").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover50k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (FGfeeincomelastyear > 50000)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover50k" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover50k" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover50k" && cref.DateDeleted == null).Status = "";
                }
            }
        }


        void uwrftrustservices(User underwritingUser, ClientAgreement agreement, bool trustservices)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrustservices" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrustservices" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (trustservices)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrustservices" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrustservices" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrustservices" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfoperatesoutsideofnz(User underwritingUser, ClientAgreement agreement, bool bolworkoutsidenz)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoutsideofnz" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoutsideofnz" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolworkoutsidenz) //Work outside New Zealand
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoutsideofnz" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoutsideofnz" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoutsideofnz" && cref.DateDeleted == null).Status = "";
                }
            }
        }



    }
}
