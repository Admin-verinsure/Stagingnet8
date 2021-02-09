using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class ApolloPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public ApolloPIUWModule()
        {
            Name = "Apollo_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "dishonestyoptionpremium", "1millimit1kexcesspremium", "1millimit5kexcesspremium", "2millimit1kexcesspremium", "2millimit5kexcesspremium",
                "3millimit1kexcesspremium", "3millimit5kexcesspremium", "5millimit1kexcesspremium", "5millimit5kexcesspremium", "1millimit1kexcesspremiuminv", "1millimit5kexcesspremiuminv", 
                "2millimit1kexcesspremiuminv", "2millimit5kexcesspremiuminv", "3millimit1kexcesspremiuminv", "3millimit5kexcesspremiuminv", "5millimit1kexcesspremiuminv", "5millimit5kexcesspremiuminv");

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
            decimal decOther = 0M;
            decimal decInv = 0M;
            bool bolworkoutsidenz = false;

            string strProfessionalBusiness = "General Insurance Brokers, Life Agents, Investment Advisers, Financial Planning and Mortgage Broking, Consultants and Advisers in the sale of any financial product including referrals to other financial product providers.";

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
                    if (uISActivity.AnzsciCode == "CUS0050") //Fire & General Domestic
                    {
                        decFGD = uISActivity.Percentage;
                        FGDfeeincomelastyear = feeincomelastyear * uISActivity.Percentage / 100;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0051") //Fire & General Commercial
                    {
                        decFGC = uISActivity.Percentage;
                        FGCfeeincomelastyear = feeincomelastyear * uISActivity.Percentage / 100;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0056") //Preparation of Financial Plans and/or monitoring of Financial Portfolios
                    {
                        decFP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0057") //Investment Products (eg Unit Trusts, Bonds)
                    {
                        decIP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0058") //Sharebroking 
                    {
                        decSB = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0060") //Direct Property Investment 
                    {
                        decDPI = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0061") //Other 
                    {
                        decOther = uISActivity.Percentage;
                    }

                }

                decInv = decFP + decIP + decSB + decDPI;
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
                    }
                }
            }

            string strtier = "Apollo Standard";
            if (agreement.ClientInformationSheet.Programme.Tier == "Apollo Prime") 
            {
                strtier = "Apollo Prime";
            } else if (agreement.ClientInformationSheet.Programme.Tier == "Mortgage Express")
            {
                strtier = "Mortgage Express";
            }

            ClientAgreementEndorsement cAEPIAP = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Apollo Prime");

            if (cAEPIAP != null)
            {
                cAEPIAP.DateDeleted = DateTime.UtcNow;
                cAEPIAP.DeletedBy = underwritingUser;
            }

            if (strtier == "Apollo Prime")
            {
                if (cAEPIAP != null)
                {
                    cAEPIAP.DateDeleted = null;
                    cAEPIAP.DeletedBy = null;
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

            decimal decDishonestyOptionPremium = 0m;
            bool dishonestyoptionselected = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasDishonestyOptions").First().Value == "1")
            {
                decDishonestyOptionPremium = rates["dishonestyoptionpremium"];
                dishonestyoptionselected = true;
            }

            bool trustservices = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "OTViewModel.HasInvolvedTrusteeOptions").First().Value == "1")
            {
                trustservices = true;
            }

            bool lessthan10percentinvestmentadvice = false;
            if (((decFP + decIP) < 10 && (decFP + decIP) > 0 && decSB == 0) || decDPI > 0)
            {
                lessthan10percentinvestmentadvice = true;
            }

            int TermExcess1k = 1000;
            int TermExcess5k = 5000;

            int TermLimit1mil1kExcess = 1000000;
            decimal TermPremium1mil1kExcess = 0M;
            decimal TermBrokerage1mil1kExcess = 0M;
            TermPremium1mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit1mil1kExcess, TermExcess1k, decInv, decDishonestyOptionPremium, strtier);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium1mil1kExcess = TermPremium1mil1kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage1mil1kExcess = TermPremium1mil1kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil1kExcess, TermExcess1k);
            term1millimit1kexcesspremiumoption.TermLimit = TermLimit1mil1kExcess;
            term1millimit1kexcesspremiumoption.Premium = TermPremium1mil1kExcess;
            term1millimit1kexcesspremiumoption.BasePremium = TermPremium1mil1kExcess;
            term1millimit1kexcesspremiumoption.Excess = TermExcess1k;
            term1millimit1kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term1millimit1kexcesspremiumoption.Brokerage = TermBrokerage1mil1kExcess;
            term1millimit1kexcesspremiumoption.DateDeleted = null;
            term1millimit1kexcesspremiumoption.DeletedBy = null;

            int TermLimit1mil5kExcess = 1000000;
            decimal TermPremium1mil5kExcess = 0M;
            decimal TermBrokerage1mil5kExcess = 0M;
            TermPremium1mil5kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit1mil5kExcess, TermExcess5k, decInv, decDishonestyOptionPremium, strtier);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium1mil5kExcess = TermPremium1mil5kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage1mil5kExcess = TermPremium1mil5kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil5kExcess, TermExcess5k);
            term1millimit5kexcesspremiumoption.TermLimit = TermLimit1mil5kExcess;
            term1millimit5kexcesspremiumoption.Premium = TermPremium1mil5kExcess;
            term1millimit5kexcesspremiumoption.BasePremium = TermPremium1mil5kExcess;
            term1millimit5kexcesspremiumoption.Excess = TermExcess5k;
            term1millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term1millimit5kexcesspremiumoption.Brokerage = TermBrokerage1mil5kExcess;
            term1millimit5kexcesspremiumoption.DateDeleted = null;
            term1millimit5kexcesspremiumoption.DeletedBy = null;

            int TermLimit2mil1kExcess = 2000000;
            decimal TermPremium2mil1kExcess = 0M;
            decimal TermBrokerage2mil1kExcess = 0M;
            TermPremium2mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit2mil1kExcess, TermExcess1k, decInv, decDishonestyOptionPremium, strtier);
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

            int TermLimit2mil5kExcess = 2000000;
            decimal TermPremium2mil5kExcess = 0M;
            decimal TermBrokerage2mil5kExcess = 0M;
            TermPremium2mil5kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit2mil5kExcess, TermExcess5k, decInv, decDishonestyOptionPremium, strtier);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium2mil5kExcess = TermPremium2mil5kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage2mil5kExcess = TermPremium2mil5kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil5kExcess, TermExcess5k);
            term2millimit5kexcesspremiumoption.TermLimit = TermLimit2mil5kExcess;
            term2millimit5kexcesspremiumoption.Premium = TermPremium2mil5kExcess;
            term2millimit5kexcesspremiumoption.BasePremium = TermPremium2mil5kExcess;
            term2millimit5kexcesspremiumoption.Excess = TermExcess5k;
            term2millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term2millimit5kexcesspremiumoption.Brokerage = TermBrokerage2mil5kExcess;
            term2millimit5kexcesspremiumoption.DateDeleted = null;
            term2millimit5kexcesspremiumoption.DeletedBy = null;

            int TermLimit3mil1kExcess = 3000000;
            decimal TermPremium3mil1kExcess = 0M;
            decimal TermBrokerage3mil1kExcess = 0M;
            TermPremium3mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit3mil1kExcess, TermExcess1k, decInv, decDishonestyOptionPremium, strtier);
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

            int TermLimit3mil5kExcess = 3000000;
            decimal TermPremium3mil5kExcess = 0M;
            decimal TermBrokerage3mil5kExcess = 0M;
            TermPremium3mil5kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit3mil5kExcess, TermExcess5k, decInv, decDishonestyOptionPremium, strtier);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium3mil5kExcess = TermPremium3mil5kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage3mil5kExcess = TermPremium3mil5kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term3millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil5kExcess, TermExcess5k);
            term3millimit5kexcesspremiumoption.TermLimit = TermLimit3mil5kExcess;
            term3millimit5kexcesspremiumoption.Premium = TermPremium3mil5kExcess;
            term3millimit5kexcesspremiumoption.BasePremium = TermPremium3mil5kExcess;
            term3millimit5kexcesspremiumoption.Excess = TermExcess5k;
            term3millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term3millimit5kexcesspremiumoption.Brokerage = TermBrokerage3mil5kExcess;
            term3millimit5kexcesspremiumoption.DateDeleted = null;
            term3millimit5kexcesspremiumoption.DeletedBy = null;

            int TermLimit5mil1kExcess = 5000000;
            decimal TermPremium5mil1kExcess = 0M;
            decimal TermBrokerage5mil1kExcess = 0M;
            TermPremium5mil1kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit5mil1kExcess, TermExcess1k, decInv, decDishonestyOptionPremium, strtier);
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

            int TermLimit5mil5kExcess = 5000000;
            decimal TermPremium5mil5kExcess = 0M;
            decimal TermBrokerage5mil5kExcess = 0M;
            TermPremium5mil5kExcess = GetPremiumForAdvisors(rates, intnumberofadvisors, TermLimit5mil5kExcess, TermExcess5k, decInv, decDishonestyOptionPremium, strtier);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium5mil5kExcess = TermPremium5mil5kExcess / coverperiodindays * agreementperiodindays;
            TermBrokerage5mil5kExcess = TermPremium5mil5kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil5kExcess, TermExcess5k);
            term5millimit5kexcesspremiumoption.TermLimit = TermLimit5mil5kExcess;
            term5millimit5kexcesspremiumoption.Premium = TermPremium5mil5kExcess;
            term5millimit5kexcesspremiumoption.BasePremium = TermPremium5mil5kExcess;
            term5millimit5kexcesspremiumoption.Excess = TermExcess5k;
            term5millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimit5kexcesspremiumoption.Brokerage = TermBrokerage5mil5kExcess;
            term5millimit5kexcesspremiumoption.DateDeleted = null;
            term5millimit5kexcesspremiumoption.DeletedBy = null;


            //Change policy premium claculation
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
                        term1millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium1mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term1millimit1kexcesspremiumoption.Excess == term.Excess)
                        {
                            term1millimit1kexcesspremiumoption.Bound = true;
                        }
                        if (term1millimit1kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term1millimit1kexcesspremiumoption.PremiumDiffer = 0;
                        }
                        term1millimit5kexcesspremiumoption.PremiumDiffer = (TermPremium1mil5kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimit5kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimit5kexcesspremiumoption.TermLimit == term.TermLimit && term1millimit5kexcesspremiumoption.Excess == term.Excess)
                        {
                            term1millimit5kexcesspremiumoption.Bound = true;
                        }
                        if (term1millimit5kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term1millimit5kexcesspremiumoption.PremiumDiffer = 0;
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
                        term2millimit5kexcesspremiumoption.PremiumDiffer = (TermPremium2mil5kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimit5kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimit5kexcesspremiumoption.TermLimit == term.TermLimit && term2millimit5kexcesspremiumoption.Excess == term.Excess)
                        {
                            term2millimit5kexcesspremiumoption.Bound = true;
                        }
                        if (term2millimit5kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term2millimit5kexcesspremiumoption.PremiumDiffer = 0;
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
                        term3millimit5kexcesspremiumoption.PremiumDiffer = (TermPremium3mil5kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term3millimit5kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimit5kexcesspremiumoption.TermLimit == term.TermLimit && term3millimit5kexcesspremiumoption.Excess == term.Excess)
                        {
                            term3millimit5kexcesspremiumoption.Bound = true;
                        }
                        if (term3millimit5kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term3millimit5kexcesspremiumoption.PremiumDiffer = 0;
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
                        term5millimit5kexcesspremiumoption.PremiumDiffer = (TermPremium5mil5kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimit5kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimit5kexcesspremiumoption.TermLimit == term.TermLimit && term5millimit5kexcesspremiumoption.Excess == term.Excess)
                        {
                            term5millimit5kexcesspremiumoption.Bound = true;
                        }
                        if (term5millimit5kexcesspremiumoption.PremiumDiffer < 0)
                        {
                            term5millimit5kexcesspremiumoption.PremiumDiffer = 0;
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
            //F&G Commercial activity over 35%
            uwrffgcactivitiesover35percent(underwritingUser, agreement, decFGC);
            //F&G income over $30,000
            uwrffgincomeover30k(underwritingUser, agreement, FGfeeincomelastyear);
            //Dishonesty Cover selected
            uwrfdishonestycoverselected(underwritingUser, agreement, dishonestyoptionselected);
            //Trust Services
            uwrftrustservices(underwritingUser, agreement, trustservices);
            //Less than 10% Investment Advice
            uwrflessthan10percentinvestmentadvice(underwritingUser, agreement, lessthan10percentinvestmentadvice);
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

            string retrodate = "Policy Inception";
            agreement.TerritoryLimit = "Worldwide";
            agreement.Jurisdiction = "Australia and New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Apollo PI UW created/modified";
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


        decimal GetPremiumForAdvisors(IDictionary<string, decimal> rates, int intnumberofadvisors, int limitoption, int excessoption, decimal decInv, decimal decDishonestyOptionPremium, string strtier)
        {
            decimal premiumoption = 0M;
            decimal totalpremiumoption = 0M;
            decimal tierpremium = 0M;

            if (strtier == "Mortgage Express")
            {
                tierpremium = 100;
            }

            if (decInv > 0)
            {
                switch (limitoption)
                {
                    case 1000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["1millimit1kexcesspremiuminv"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["1millimit5kexcesspremiuminv"];
                            }
                            break;
                        }
                    case 2000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["2millimit1kexcesspremiuminv"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["2millimit5kexcesspremiuminv"];
                            }
                            break;
                        }
                    case 3000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["3millimit1kexcesspremiuminv"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["3millimit5kexcesspremiuminv"];
                            }
                            break;
                        }
                    case 5000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["5millimit1kexcesspremiuminv"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["5millimit5kexcesspremiuminv"];
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            } else
            {
                switch (limitoption)
                {
                    case 1000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["1millimit1kexcesspremium"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["1millimit5kexcesspremium"];
                            }
                            break;
                        }
                    case 2000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["2millimit1kexcesspremium"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["2millimit5kexcesspremium"];
                            }
                            break;
                        }
                    case 3000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["3millimit1kexcesspremium"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["3millimit5kexcesspremium"];
                            }
                            break;
                        }
                    case 5000000:
                        {
                            if (excessoption == 1000)
                            {
                                premiumoption = rates["5millimit1kexcesspremium"];
                            }
                            else if (excessoption == 5000)
                            {
                                premiumoption = rates["5millimit5kexcesspremium"];
                            }
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }

            totalpremiumoption = (premiumoption + decDishonestyOptionPremium + tierpremium) * intnumberofadvisors;

            return totalpremiumoption;
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

        void uwrfsbactivitiesover10percent(User underwritingUser, ClientAgreement agreement, decimal decSB)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsbactivitiesover10percent" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsbactivitiesover10percent").OrderNumber));
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

        void uwrffgcactivitiesover35percent(User underwritingUser, ClientAgreement agreement, decimal decFGC)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgcactivitiesover35percent" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgcactivitiesover35percent") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgcactivitiesover35percent").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgcactivitiesover35percent").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgcactivitiesover35percent").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgcactivitiesover35percent").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgcactivitiesover35percent" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decFGC > 35)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgcactivitiesover35percent" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }

        void uwrffgincomeover30k(User underwritingUser, ClientAgreement agreement, decimal FGfeeincomelastyear)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover30k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover30k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover30k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover30k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover30k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgincomeover30k").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover30k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (FGfeeincomelastyear > 30000)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgincomeover30k" && cref.DateDeleted == null).Status = "Pending";
                    }
                }
            }
        }

        void uwrfdishonestycoverselected(User underwritingUser, ClientAgreement agreement, bool dishonestyoptionselected)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdishonestycoverselected" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdishonestycoverselected") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdishonestycoverselected").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdishonestycoverselected").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdishonestycoverselected").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdishonestycoverselected").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdishonestycoverselected" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (dishonestyoptionselected)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdishonestycoverselected" && cref.DateDeleted == null).Status = "Pending";
                    }
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrustservices").OrderNumber));
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
            }
        }

        void uwrflessthan10percentinvestmentadvice(User underwritingUser, ClientAgreement agreement, bool lessthan10percentinvestmentadvice)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrflessthan10percentinvestmentadvice" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrflessthan10percentinvestmentadvice") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrflessthan10percentinvestmentadvice").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrflessthan10percentinvestmentadvice").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrflessthan10percentinvestmentadvice").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrflessthan10percentinvestmentadvice").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrflessthan10percentinvestmentadvice" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (lessthan10percentinvestmentadvice)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrflessthan10percentinvestmentadvice" && cref.DateDeleted == null).Status = "Pending";
                    }
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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").OrderNumber));
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
            }
        }



    }
}
