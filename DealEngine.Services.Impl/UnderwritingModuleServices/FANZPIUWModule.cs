using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class FANZPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public FANZPIUWModule()
        {
            Name = "FANZ_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "1millimitpremiumratemb", "1millimitminimumpremiummbclass1", "1millimitminimumpremiummbclass2",
                "2millimitpremiumratemb", "2millimitminimumpremiummbclass1", "2millimitminimumpremiummbclass2", "3millimitpremiumratemb", "3millimitminimumpremiummbclass1", "3millimitminimumpremiummbclass2",
                "4millimitpremiumratemb", "4millimitminimumpremiummbclass1", "4millimitminimumpremiummbclass2", "5millimitpremiumratemb", "5millimitminimumpremiummbclass1", "5millimitminimumpremiummbclass2",
                "1millimitpremiumrateslh", "1millimitminimumpremiumslhclass1", "1millimitminimumpremiumslhclass2",
                "2millimitpremiumrateslh", "2millimitminimumpremiumslhclass1", "2millimitminimumpremiumslhclass2", "3millimitpremiumrateslh", "3millimitminimumpremiumslhclass1", "3millimitminimumpremiumslhclass2",
                "4millimitpremiumrateslh", "4millimitminimumpremiumslhclass1", "4millimitminimumpremiumslhclass2", "5millimitpremiumrateslh", "5millimitminimumpremiumslhclass1", "5millimitminimumpremiumslhclass2",
                "1millimitpremiumratelhfg", "1millimitminimumpremiumlhfgclass1", "1millimitminimumpremiumlhfgclass2",
                "2millimitpremiumratelhfg", "2millimitminimumpremiumlhfgclass1", "2millimitminimumpremiumlhfgclass2", "3millimitpremiumratelhfg", "3millimitminimumpremiumlhfgclass1", "3millimitminimumpremiumlhfgclass2",
                "4millimitpremiumratelhfg", "4millimitminimumpremiumlhfgclass1", "4millimitminimumpremiumlhfgclass2", "5millimitpremiumratelhfg", "5millimitminimumpremiumlhfgclass1", "5millimitminimumpremiumlhfgclass2",
                "1millimitpremiumrateinv", "1millimitminimumpremiuminvclass1", "1millimitminimumpremiuminvclass2",
                "2millimitpremiumrateinv", "2millimitminimumpremiuminvclass1", "2millimitminimumpremiuminvclass2", "3millimitpremiumrateinv", "3millimitminimumpremiuminvclass1", "3millimitminimumpremiuminvclass2",
                "4millimitpremiumrateinv", "4millimitminimumpremiuminvclass1", "4millimitminimumpremiuminvclass2", "5millimitpremiumrateinv", "5millimitminimumpremiuminvclass1", "5millimitminimumpremiuminvclass2", 
                "10kexcessdiscountrate", "25kexcessdiscountrate", "10kexcessdiscountrateadvisortruststatus", "25kexcessdiscountrateadvisortruststatus");

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

            decimal feeincomelastyear = 0M;
            decimal feeincomecurrentyear = 0M;
            decimal feeincomenextyear = 0M;
            decimal feeincome = 0M;
            decimal decInv = 0M;
            decimal decFG = 0M;
            decimal decOther = 0M;

            decimal decMBCategoryPercentage = 0M;
            decimal decSLHCategoryPercentage = 0M;
            decimal decLHFGCategoryPercentage = 0M;
            decimal decInvProdCategoryPercentage = 0M;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincomelastyear = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                }
                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0)
                {
                    feeincomecurrentyear = agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                }
                if (agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal > 0)
                {
                    feeincomenextyear = agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal;
                }
            }

            //Programme specific term

            //Default Professional Business, Retroactive Date, TerritoryLimit, Jurisdiction, AuditLog Detail
            string strProfessionalBusiness = "Financial Advice Provider – in the provision of Life & Health Insurance, Investment Advice, Mortgage Broking, Financial Planning and Fire & General Broking ";
            string retrodate = "Unlimited excluding known claims or circumstances";
            string strTerritoryLimit = "New Zealand";
            string strJurisdiction = "New Zealand";
            string auditLogDetail = "FANZ PI UW created/modified";

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUSFANZ01") //Financial Planning (eg Preparation and/or Monitoring of Financial Plans)
                    {
                        if (uISActivity.Percentage > 0)
                            decInvProdCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ02") //Fire & General Commercial (eg Business Insurance)
                    {
                        if (uISActivity.Percentage > 0)
                        {
                            decLHFGCategoryPercentage += uISActivity.Percentage;
                            decFG += uISActivity.Percentage;
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ03") //Fire & General (eg House, Contents, Car, Travel)
                    {
                        if (uISActivity.Percentage > 0)
                        {
                            decLHFGCategoryPercentage += uISActivity.Percentage;
                            decFG += uISActivity.Percentage;
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ04") //Fire & General – Referral Income
                    {
                        if (uISActivity.Percentage > 0)
                        {
                            decLHFGCategoryPercentage += uISActivity.Percentage;
                            decFG += uISActivity.Percentage;
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ05") //Home Equity Release (including reverse mortgages)
                    {
                        if (uISActivity.Percentage > 0)
                            decMBCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ06") //Insurance – Personal Risk (eg Life insurance, Income Protection, Health, In/Expatriate insurance)
                    {
                        if (uISActivity.Percentage > 0)
                            decSLHCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ07") //Investments (eg Advice on Portfolio)
                    {
                        if (uISActivity.Percentage > 0)
                        {
                            decInvProdCategoryPercentage += uISActivity.Percentage;
                            decInv = uISActivity.Percentage;
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ08") //Managed Investment Funds (eg Unit Trusts, PIEs, Advice on portfolios)
                    {
                        if (uISActivity.Percentage > 0)
                            decInvProdCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ09") //Kiwisaver & Retirement Savings (including superannuation)
                    {
                        if (uISActivity.Percentage > 0)
                            decSLHCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ10") //Mortgage Advice (Commercial, Residential & Asset Financing)
                    {
                        if (uISActivity.Percentage > 0)
                            decMBCategoryPercentage += uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSFANZ11") //Other
                    {
                        if (uISActivity.Percentage > 0)
                            decOther = uISActivity.Percentage;
                    }
                }
            }
            if (decLHFGCategoryPercentage > 0)
            {
                decLHFGCategoryPercentage += decSLHCategoryPercentage;
            }
            feeincome = feeincomelastyear;


            //Update retro date and endorsement based on the pre-renewal data or renewal agreement
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

            //Check class information
            int intclasscategory = 0;
            bool bolclass3referral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "1")
                {
                    intclasscategory = 1;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "2")
                {
                    intclasscategory = 2;
                }
                else if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasClassOfLicense").First().Value == "3")
                {
                    intclasscategory = 3;
                    bolclass3referral = true;
                }
            }
            if (intclasscategory == 0)
            {
                bolclass3referral = true;
            }

            //Check Cluster Groups information
            bool bolclustergroupsreferral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPartyMembers").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPartyMembers").First().Value == "1" ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPartyMembers").First().Value == "2" ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPartyMembers").First().Value == "3" ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPartyMembers").First().Value == "4")
                {
                    bolclustergroupsreferral = true;
                }
            }

            //Check Dual Insurance information
            bool boldualinsurancereferral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasOtherPIinsurances").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasOtherPIinsurances").First().Value == "1")
                {
                    boldualinsurancereferral = true;
                }
            }

            //Check Trusted Advisor status information
            bool boladvisortruststatus = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.TrustAdvisorStatus").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.TrustAdvisorStatus").First().Value == "1")
                {
                    boladvisortruststatus = true;
                }
            }

            //Calculate premium option
            int option = 0;
            decimal investmentfeeincome = 0M;
            decimal remainingfeeincome = 0M;

            var values = new List<decimal> { decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage };
            decimal maxactivitypercentage = values.Max();

            if (maxactivitypercentage == decInvProdCategoryPercentage)
            {
                if (maxactivitypercentage != decLHFGCategoryPercentage && maxactivitypercentage != decSLHCategoryPercentage && maxactivitypercentage != decMBCategoryPercentage)
                {
                    option = 4; //Investment
                }
                else if (decLHFGCategoryPercentage > 0)
                {
                    option = 5; //Highest investment percentage equal with L&H <5% F&G
                    investmentfeeincome = decInvProdCategoryPercentage * feeincome / 100;
                    remainingfeeincome = (100 - decInvProdCategoryPercentage) * feeincome / 100;
                }
                else if (decSLHCategoryPercentage > 0)
                {
                    option = 6; //Highest investment percentage equal with Standard L&H
                    investmentfeeincome = decInvProdCategoryPercentage * feeincome / 100;
                    remainingfeeincome = (100 - decInvProdCategoryPercentage) * feeincome / 100;
                }
                else if (decMBCategoryPercentage > 0)
                {
                    option = 7; //Highest investment percentage equal with Mortgage broking
                    investmentfeeincome = decInvProdCategoryPercentage * feeincome / 100;
                    remainingfeeincome = (100 - decInvProdCategoryPercentage) * feeincome / 100;
                }
            }
            else if (maxactivitypercentage == decLHFGCategoryPercentage)
            {
                option = 3; //L&H <5% F&G
            }
            else if (maxactivitypercentage == decSLHCategoryPercentage)
            {
                option = 2; //Standard L&H
            }
            else if (maxactivitypercentage == decMBCategoryPercentage)
            {
                option = 1; //Mortgage broking
            }


            decimal StandardTermExcess = 0M;
            decimal TermExcess10k = 10000;
            decimal TermExcess25k = 25000;

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0M;
            decimal TermBasePremium1mil = 0M;
            decimal TermBrokerage1mil = 0M;
            decimal TermPremium1mil10kexcess = 0M;
            decimal TermBasePremium1mil10kexcess = 0M;
            decimal TermBrokerage1mil10kexcess = 0M;
            decimal TermPremium1mil25kexcess = 0M;
            decimal TermBasePremium1mil25kexcess = 0M;
            decimal TermBrokerage1mil25kexcess = 0M;

            if (boladvisortruststatus)
            {
                StandardTermExcess = 2500;
            }
            else
            {
                StandardTermExcess = 5000;
            }

            TermPremium1mil = GetPremium(rates, TermLimit1mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, StandardTermExcess, boladvisortruststatus);
            TermBasePremium1mil = TermPremium1mil;
            TermPremium1mil = TermPremium1mil * agreementperiodindays / coverperiodindays;
            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, StandardTermExcess);
            term1millimitpremiumoption.TermLimit = TermLimit1mil;
            term1millimitpremiumoption.Premium = TermPremium1mil;
            term1millimitpremiumoption.BasePremium = TermBasePremium1mil;
            term1millimitpremiumoption.Excess = StandardTermExcess;
            term1millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term1millimitpremiumoption.Brokerage = TermBrokerage1mil;
            term1millimitpremiumoption.DateDeleted = null;
            term1millimitpremiumoption.DeletedBy = null;

            TermPremium1mil10kexcess = GetPremium(rates, TermLimit1mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess10k, boladvisortruststatus);
            TermBasePremium1mil10kexcess = TermPremium1mil10kexcess;
            TermPremium1mil10kexcess = TermPremium1mil10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage1mil10kexcess = TermPremium1mil10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimitpremium10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess10k);
            term1millimitpremium10kexcessoption.TermLimit = TermLimit1mil;
            term1millimitpremium10kexcessoption.Premium = TermPremium1mil10kexcess;
            term1millimitpremium10kexcessoption.BasePremium = TermBasePremium1mil10kexcess;
            term1millimitpremium10kexcessoption.Excess = TermExcess10k;
            term1millimitpremium10kexcessoption.BrokerageRate = agreement.Brokerage;
            term1millimitpremium10kexcessoption.Brokerage = TermBrokerage1mil10kexcess;
            term1millimitpremium10kexcessoption.DateDeleted = null;
            term1millimitpremium10kexcessoption.DeletedBy = null;

            TermPremium1mil25kexcess = GetPremium(rates, TermLimit1mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess25k, boladvisortruststatus);
            TermBasePremium1mil25kexcess = TermPremium1mil25kexcess;
            TermPremium1mil25kexcess = TermPremium1mil25kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage1mil25kexcess = TermPremium1mil25kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimitpremium25kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess25k);
            term1millimitpremium25kexcessoption.TermLimit = TermLimit1mil;
            term1millimitpremium25kexcessoption.Premium = TermPremium1mil25kexcess;
            term1millimitpremium25kexcessoption.BasePremium = TermBasePremium1mil25kexcess;
            term1millimitpremium25kexcessoption.Excess = TermExcess25k;
            term1millimitpremium25kexcessoption.BrokerageRate = agreement.Brokerage;
            term1millimitpremium25kexcessoption.Brokerage = TermBrokerage1mil25kexcess;
            term1millimitpremium25kexcessoption.DateDeleted = null;
            term1millimitpremium25kexcessoption.DeletedBy = null;


            int TermLimit2mil = 2000000;
            decimal TermPremium2mil = 0M;
            decimal TermBasePremium2mil = 0M;
            decimal TermBrokerage2mil = 0M;
            decimal TermPremium2mil10kexcess = 0M;
            decimal TermBasePremium2mil10kexcess = 0M;
            decimal TermBrokerage2mil10kexcess = 0M;
            decimal TermPremium2mil25kexcess = 0M;
            decimal TermBasePremium2mil25kexcess = 0M;
            decimal TermBrokerage2mil25kexcess = 0M;

            TermPremium2mil = GetPremium(rates, TermLimit2mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, StandardTermExcess, boladvisortruststatus);
            TermBasePremium2mil = TermPremium2mil;
            TermPremium2mil = TermPremium2mil * agreementperiodindays / coverperiodindays;
            TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, StandardTermExcess);
            term2millimitpremiumoption.TermLimit = TermLimit2mil;
            term2millimitpremiumoption.Premium = TermPremium2mil;
            term2millimitpremiumoption.BasePremium = TermBasePremium2mil;
            term2millimitpremiumoption.Excess = StandardTermExcess;
            term2millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term2millimitpremiumoption.Brokerage = TermBrokerage2mil;
            term2millimitpremiumoption.DateDeleted = null;
            term2millimitpremiumoption.DeletedBy = null;

            TermPremium2mil10kexcess = GetPremium(rates, TermLimit2mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess10k, boladvisortruststatus);
            TermBasePremium2mil10kexcess = TermPremium2mil10kexcess;
            TermPremium2mil10kexcess = TermPremium2mil10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage2mil10kexcess = TermPremium2mil10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimitpremium10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess10k);
            term2millimitpremium10kexcessoption.TermLimit = TermLimit2mil;
            term2millimitpremium10kexcessoption.Premium = TermPremium2mil10kexcess;
            term2millimitpremium10kexcessoption.BasePremium = TermBasePremium2mil10kexcess;
            term2millimitpremium10kexcessoption.Excess = TermExcess10k;
            term2millimitpremium10kexcessoption.BrokerageRate = agreement.Brokerage;
            term2millimitpremium10kexcessoption.Brokerage = TermBrokerage2mil10kexcess;
            term2millimitpremium10kexcessoption.DateDeleted = null;
            term2millimitpremium10kexcessoption.DeletedBy = null;

            TermPremium2mil25kexcess = GetPremium(rates, TermLimit2mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess25k, boladvisortruststatus);
            TermBasePremium2mil25kexcess = TermPremium2mil25kexcess;
            TermPremium2mil25kexcess = TermPremium2mil25kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage2mil25kexcess = TermPremium2mil25kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimitpremium25kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess25k);
            term2millimitpremium25kexcessoption.TermLimit = TermLimit2mil;
            term2millimitpremium25kexcessoption.Premium = TermPremium2mil25kexcess;
            term2millimitpremium25kexcessoption.BasePremium = TermBasePremium2mil25kexcess;
            term2millimitpremium25kexcessoption.Excess = TermExcess25k;
            term2millimitpremium25kexcessoption.BrokerageRate = agreement.Brokerage;
            term2millimitpremium25kexcessoption.Brokerage = TermBrokerage2mil25kexcess;
            term2millimitpremium25kexcessoption.DateDeleted = null;
            term2millimitpremium25kexcessoption.DeletedBy = null;


            int TermLimit3mil = 3000000;
            decimal TermPremium3mil = 0M;
            decimal TermBasePremium3mil = 0M;
            decimal TermBrokerage3mil = 0M;
            decimal TermPremium3mil10kexcess = 0M;
            decimal TermBasePremium3mil10kexcess = 0M;
            decimal TermBrokerage3mil10kexcess = 0M;
            decimal TermPremium3mil25kexcess = 0M;
            decimal TermBasePremium3mil25kexcess = 0M;
            decimal TermBrokerage3mil25kexcess = 0M;

            TermPremium3mil = GetPremium(rates, TermLimit3mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, StandardTermExcess, boladvisortruststatus);
            TermBasePremium3mil = TermPremium3mil;
            TermPremium3mil = TermPremium3mil * agreementperiodindays / coverperiodindays;
            TermBrokerage3mil = TermPremium3mil * agreement.Brokerage / 100;

            ClientAgreementTerm term3millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil, StandardTermExcess);
            term3millimitpremiumoption.TermLimit = TermLimit3mil;
            term3millimitpremiumoption.Premium = TermPremium3mil;
            term3millimitpremiumoption.BasePremium = TermBasePremium3mil;
            term3millimitpremiumoption.Excess = StandardTermExcess;
            term3millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term3millimitpremiumoption.Brokerage = TermBrokerage3mil;
            term3millimitpremiumoption.DateDeleted = null;
            term3millimitpremiumoption.DeletedBy = null;

            TermPremium3mil10kexcess = GetPremium(rates, TermLimit3mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess10k, boladvisortruststatus);
            TermBasePremium3mil10kexcess = TermPremium3mil10kexcess;
            TermPremium3mil10kexcess = TermPremium3mil10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage3mil10kexcess = TermPremium3mil10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term3millimitpremium10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil, TermExcess10k);
            term3millimitpremium10kexcessoption.TermLimit = TermLimit3mil;
            term3millimitpremium10kexcessoption.Premium = TermPremium3mil10kexcess;
            term3millimitpremium10kexcessoption.BasePremium = TermBasePremium3mil10kexcess;
            term3millimitpremium10kexcessoption.Excess = TermExcess10k;
            term3millimitpremium10kexcessoption.BrokerageRate = agreement.Brokerage;
            term3millimitpremium10kexcessoption.Brokerage = TermBrokerage3mil10kexcess;
            term3millimitpremium10kexcessoption.DateDeleted = null;
            term3millimitpremium10kexcessoption.DeletedBy = null;

            TermPremium3mil25kexcess = GetPremium(rates, TermLimit3mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess25k, boladvisortruststatus);
            TermBasePremium3mil25kexcess = TermPremium3mil25kexcess;
            TermPremium3mil25kexcess = TermPremium3mil25kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage3mil25kexcess = TermPremium3mil25kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term3millimitpremium25kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil, TermExcess25k);
            term3millimitpremium25kexcessoption.TermLimit = TermLimit3mil;
            term3millimitpremium25kexcessoption.Premium = TermPremium3mil25kexcess;
            term3millimitpremium25kexcessoption.BasePremium = TermBasePremium3mil25kexcess;
            term3millimitpremium25kexcessoption.Excess = TermExcess25k;
            term3millimitpremium25kexcessoption.BrokerageRate = agreement.Brokerage;
            term3millimitpremium25kexcessoption.Brokerage = TermBrokerage3mil25kexcess;
            term3millimitpremium25kexcessoption.DateDeleted = null;
            term3millimitpremium25kexcessoption.DeletedBy = null;


            int TermLimit4mil = 4000000;
            decimal TermPremium4mil = 0M;
            decimal TermBasePremium4mil = 0M;
            decimal TermBrokerage4mil = 0M;
            decimal TermPremium4mil10kexcess = 0M;
            decimal TermBasePremium4mil10kexcess = 0M;
            decimal TermBrokerage4mil10kexcess = 0M;
            decimal TermPremium4mil25kexcess = 0M;
            decimal TermBasePremium4mil25kexcess = 0M;
            decimal TermBrokerage4mil25kexcess = 0M;

            TermPremium4mil = GetPremium(rates, TermLimit4mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, StandardTermExcess, boladvisortruststatus);
            TermBasePremium4mil = TermPremium4mil;
            TermPremium4mil = TermPremium4mil * agreementperiodindays / coverperiodindays;
            TermBrokerage4mil = TermPremium4mil * agreement.Brokerage / 100;

            ClientAgreementTerm term4millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit4mil, StandardTermExcess);
            term4millimitpremiumoption.TermLimit = TermLimit4mil;
            term4millimitpremiumoption.Premium = TermPremium4mil;
            term4millimitpremiumoption.BasePremium = TermBasePremium4mil;
            term4millimitpremiumoption.Excess = StandardTermExcess;
            term4millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term4millimitpremiumoption.Brokerage = TermBrokerage4mil;
            term4millimitpremiumoption.DateDeleted = null;
            term4millimitpremiumoption.DeletedBy = null;

            TermPremium4mil10kexcess = GetPremium(rates, TermLimit4mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess10k, boladvisortruststatus);
            TermBasePremium4mil10kexcess = TermPremium4mil10kexcess;
            TermPremium4mil10kexcess = TermPremium4mil10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage4mil10kexcess = TermPremium4mil10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term4millimitpremium10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit4mil, TermExcess10k);
            term4millimitpremium10kexcessoption.TermLimit = TermLimit4mil;
            term4millimitpremium10kexcessoption.Premium = TermPremium4mil10kexcess;
            term4millimitpremium10kexcessoption.BasePremium = TermBasePremium4mil10kexcess;
            term4millimitpremium10kexcessoption.Excess = TermExcess10k;
            term4millimitpremium10kexcessoption.BrokerageRate = agreement.Brokerage;
            term4millimitpremium10kexcessoption.Brokerage = TermBrokerage4mil10kexcess;
            term4millimitpremium10kexcessoption.DateDeleted = null;
            term4millimitpremium10kexcessoption.DeletedBy = null;

            TermPremium4mil25kexcess = GetPremium(rates, TermLimit4mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess25k, boladvisortruststatus);
            TermBasePremium4mil25kexcess = TermPremium4mil25kexcess;
            TermPremium4mil25kexcess = TermPremium4mil25kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage4mil25kexcess = TermPremium4mil25kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term4millimitpremium25kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit4mil, TermExcess25k);
            term4millimitpremium25kexcessoption.TermLimit = TermLimit4mil;
            term4millimitpremium25kexcessoption.Premium = TermPremium4mil25kexcess;
            term4millimitpremium25kexcessoption.BasePremium = TermBasePremium4mil25kexcess;
            term4millimitpremium25kexcessoption.Excess = TermExcess25k;
            term4millimitpremium25kexcessoption.BrokerageRate = agreement.Brokerage;
            term4millimitpremium25kexcessoption.Brokerage = TermBrokerage4mil25kexcess;
            term4millimitpremium25kexcessoption.DateDeleted = null;
            term4millimitpremium25kexcessoption.DeletedBy = null;


            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = 0M;
            decimal TermBasePremium5mil = 0M;
            decimal TermBrokerage5mil = 0M;
            decimal TermPremium5mil10kexcess = 0M;
            decimal TermBasePremium5mil10kexcess = 0M;
            decimal TermBrokerage5mil10kexcess = 0M;
            decimal TermPremium5mil25kexcess = 0M;
            decimal TermBasePremium5mil25kexcess = 0M;
            decimal TermBrokerage5mil25kexcess = 0M;

            TermPremium5mil = GetPremium(rates, TermLimit5mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, StandardTermExcess, boladvisortruststatus);
            TermBasePremium5mil = TermPremium5mil;
            TermPremium5mil = TermPremium5mil * agreementperiodindays / coverperiodindays;
            TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, StandardTermExcess);
            term5millimitpremiumoption.TermLimit = TermLimit5mil;
            term5millimitpremiumoption.Premium = TermPremium5mil;
            term5millimitpremiumoption.BasePremium = TermBasePremium5mil;
            term5millimitpremiumoption.Excess = StandardTermExcess;
            term5millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremiumoption.Brokerage = TermBrokerage5mil;
            term5millimitpremiumoption.DateDeleted = null;
            term5millimitpremiumoption.DeletedBy = null;

            TermPremium5mil10kexcess = GetPremium(rates, TermLimit5mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess10k, boladvisortruststatus);
            TermBasePremium5mil10kexcess = TermPremium5mil10kexcess;
            TermPremium5mil10kexcess = TermPremium5mil10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage5mil10kexcess = TermPremium5mil10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimitpremium10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess10k);
            term5millimitpremium10kexcessoption.TermLimit = TermLimit5mil;
            term5millimitpremium10kexcessoption.Premium = TermPremium5mil10kexcess;
            term5millimitpremium10kexcessoption.BasePremium = TermBasePremium5mil10kexcess;
            term5millimitpremium10kexcessoption.Excess = TermExcess10k;
            term5millimitpremium10kexcessoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremium10kexcessoption.Brokerage = TermBrokerage5mil10kexcess;
            term5millimitpremium10kexcessoption.DateDeleted = null;
            term5millimitpremium10kexcessoption.DeletedBy = null;

            TermPremium5mil25kexcess = GetPremium(rates, TermLimit5mil, intclasscategory, feeincome, option, investmentfeeincome, remainingfeeincome, decMBCategoryPercentage, decSLHCategoryPercentage, decLHFGCategoryPercentage, decInvProdCategoryPercentage, TermExcess25k, boladvisortruststatus);
            TermBasePremium5mil25kexcess = TermPremium5mil25kexcess;
            TermPremium5mil25kexcess = TermPremium5mil25kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage5mil25kexcess = TermPremium5mil25kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimitpremium25kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess25k);
            term5millimitpremium25kexcessoption.TermLimit = TermLimit5mil;
            term5millimitpremium25kexcessoption.Premium = TermPremium5mil25kexcess;
            term5millimitpremium25kexcessoption.BasePremium = TermBasePremium5mil25kexcess;
            term5millimitpremium25kexcessoption.Excess = TermExcess25k;
            term5millimitpremium25kexcessoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremium25kexcessoption.Brokerage = TermBrokerage5mil25kexcess;
            term5millimitpremium25kexcessoption.DateDeleted = null;
            term5millimitpremium25kexcessoption.DeletedBy = null;


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
                        term1millimitpremium10kexcessoption.PremiumDiffer = (TermPremium1mil10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimitpremium10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimitpremium10kexcessoption.TermLimit == term.TermLimit && term1millimitpremium10kexcessoption.Excess == term.Excess)
                        {
                            term1millimitpremium10kexcessoption.Bound = true;
                        }
                        if (term1millimitpremium10kexcessoption.PremiumDiffer < 0)
                        {
                            term1millimitpremium10kexcessoption.PremiumDiffer = 0;
                        }
                        term1millimitpremium25kexcessoption.PremiumDiffer = (TermPremium1mil25kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimitpremium25kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimitpremium25kexcessoption.TermLimit == term.TermLimit && term1millimitpremium25kexcessoption.Excess == term.Excess)
                        {
                            term1millimitpremium25kexcessoption.Bound = true;
                        }
                        if (term1millimitpremium25kexcessoption.PremiumDiffer < 0)
                        {
                            term1millimitpremium25kexcessoption.PremiumDiffer = 0;
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
                        term2millimitpremium10kexcessoption.PremiumDiffer = (TermPremium2mil10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimitpremium10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimitpremium10kexcessoption.TermLimit == term.TermLimit && term2millimitpremium10kexcessoption.Excess == term.Excess)
                        {
                            term2millimitpremium10kexcessoption.Bound = true;
                        }
                        if (term2millimitpremium10kexcessoption.PremiumDiffer < 0)
                        {
                            term2millimitpremium10kexcessoption.PremiumDiffer = 0;
                        }
                        term2millimitpremium25kexcessoption.PremiumDiffer = (TermPremium2mil25kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimitpremium25kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimitpremium25kexcessoption.TermLimit == term.TermLimit && term2millimitpremium25kexcessoption.Excess == term.Excess)
                        {
                            term2millimitpremium25kexcessoption.Bound = true;
                        }
                        if (term2millimitpremium25kexcessoption.PremiumDiffer < 0)
                        {
                            term2millimitpremium25kexcessoption.PremiumDiffer = 0;
                        }
                        term3millimitpremiumoption.PremiumDiffer = (TermPremium3mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term3millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimitpremiumoption.TermLimit == term.TermLimit && term3millimitpremiumoption.Excess == term.Excess)
                        {
                            term3millimitpremiumoption.Bound = true;
                        }
                        if (term3millimitpremiumoption.PremiumDiffer < 0)
                        {
                            term3millimitpremiumoption.PremiumDiffer = 0;
                        }
                        term3millimitpremium10kexcessoption.PremiumDiffer = (TermPremium3mil10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term3millimitpremium10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimitpremium10kexcessoption.TermLimit == term.TermLimit && term3millimitpremium10kexcessoption.Excess == term.Excess)
                        {
                            term3millimitpremium10kexcessoption.Bound = true;
                        }
                        if (term3millimitpremium10kexcessoption.PremiumDiffer < 0)
                        {
                            term3millimitpremium10kexcessoption.PremiumDiffer = 0;
                        }
                        term3millimitpremium25kexcessoption.PremiumDiffer = (TermPremium3mil25kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term3millimitpremium25kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimitpremium25kexcessoption.TermLimit == term.TermLimit && term3millimitpremium25kexcessoption.Excess == term.Excess)
                        {
                            term3millimitpremium25kexcessoption.Bound = true;
                        }
                        if (term3millimitpremium25kexcessoption.PremiumDiffer < 0)
                        {
                            term3millimitpremium25kexcessoption.PremiumDiffer = 0;
                        }
                        term4millimitpremiumoption.PremiumDiffer = (TermPremium4mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term4millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term4millimitpremiumoption.TermLimit == term.TermLimit && term4millimitpremiumoption.Excess == term.Excess)
                        {
                            term4millimitpremiumoption.Bound = true;
                        }
                        if (term4millimitpremiumoption.PremiumDiffer < 0)
                        {
                            term4millimitpremiumoption.PremiumDiffer = 0;
                        }
                        term4millimitpremium10kexcessoption.PremiumDiffer = (TermPremium4mil10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term4millimitpremium10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term4millimitpremium10kexcessoption.TermLimit == term.TermLimit && term4millimitpremium10kexcessoption.Excess == term.Excess)
                        {
                            term4millimitpremium10kexcessoption.Bound = true;
                        }
                        if (term4millimitpremium10kexcessoption.PremiumDiffer < 0)
                        {
                            term4millimitpremium10kexcessoption.PremiumDiffer = 0;
                        }
                        term4millimitpremium25kexcessoption.PremiumDiffer = (TermPremium4mil25kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term4millimitpremium25kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term4millimitpremium25kexcessoption.TermLimit == term.TermLimit && term4millimitpremium25kexcessoption.Excess == term.Excess)
                        {
                            term4millimitpremium25kexcessoption.Bound = true;
                        }
                        if (term4millimitpremium25kexcessoption.PremiumDiffer < 0)
                        {
                            term4millimitpremium25kexcessoption.PremiumDiffer = 0;
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
                        term5millimitpremium10kexcessoption.PremiumDiffer = (TermPremium5mil10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimitpremium10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimitpremium10kexcessoption.TermLimit == term.TermLimit && term5millimitpremium10kexcessoption.Excess == term.Excess)
                        {
                            term5millimitpremium10kexcessoption.Bound = true;
                        }
                        if (term5millimitpremium10kexcessoption.PremiumDiffer < 0)
                        {
                            term5millimitpremium10kexcessoption.PremiumDiffer = 0;
                        }
                        term5millimitpremium25kexcessoption.PremiumDiffer = (TermPremium5mil25kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimitpremium25kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimitpremium25kexcessoption.TermLimit == term.TermLimit && term5millimitpremium25kexcessoption.Excess == term.Excess)
                        {
                            term5millimitpremium25kexcessoption.Bound = true;
                        }
                        if (term5millimitpremium25kexcessoption.PremiumDiffer < 0)
                        {
                            term5millimitpremium25kexcessoption.PremiumDiffer = 0;
                        }
                    }

                }
            }

            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //Cluster Groups
            uwrfclustergroups(underwritingUser, agreement, bolclustergroupsreferral);
            //Dual Insurance
            uwrfdualinsurance(underwritingUser, agreement, boldualinsurancereferral);
            //Investment Activity
            uwrfinvestmentactivity(underwritingUser, agreement, decInv);
            //Other Activity
            uwrfotheractivity(underwritingUser, agreement, decOther);
            //Class 3 referral
            uwrfclass3(underwritingUser, agreement, bolclass3referral);
            //Referred Fire and General Activity
            uwrffgactivity(underwritingUser, agreement, decFG, feeincome);
            //High Fee Income
            uwrfhighfeeincome(underwritingUser, agreement, feeincome);

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


        decimal GetPremium(IDictionary<string, decimal> rates, int termlimit, int intclasscategory, decimal feeincome, int option, decimal investmentfeeincome, decimal remainingfeeincome, decimal decMBCategoryPercentage, decimal decSLHCategoryPercentage, decimal decLHFGCategoryPercentage, decimal decInvProdCategoryPercentage, decimal TermExcess, bool boladvisortruststatus)
        {
            decimal premium = 0M;
            decimal minimumpremium = 0M;
            decimal discountrate = 0M;

            if (intclasscategory == 1 || intclasscategory == 2)
            {
                if (boladvisortruststatus)
                {
                    switch (TermExcess)
                    {
                        case 2500:
                            {
                                discountrate = 0M;
                                break;
                            }
                        case 5000:
                            {
                                discountrate = 0M;
                                break;
                            }
                        case 10000:
                            {
                                discountrate = rates["10kexcessdiscountrateadvisortruststatus"];
                                break;
                            }
                        case 25000:
                            {
                                discountrate = rates["25kexcessdiscountrateadvisortruststatus"];
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
                    switch (TermExcess)
                    {
                        case 2500:
                            {
                                discountrate = 0M;
                                break;
                            }
                        case 5000:
                            {
                                discountrate = 0M;
                                break;
                            }
                        case 10000:
                            {
                                discountrate = rates["10kexcessdiscountrate"];
                                break;
                            }
                        case 25000:
                            {
                                discountrate = rates["25kexcessdiscountrate"];
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }

                if (option == 1) //Mortgage broking
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = feeincome * rates["1millimitpremiumratemb"] / 100 * (100 - discountrate) /100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiummbclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiummbclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = feeincome * rates["2millimitpremiumratemb"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiummbclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiummbclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = feeincome * rates["3millimitpremiumratemb"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiummbclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiummbclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = feeincome * rates["4millimitpremiumratemb"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiummbclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiummbclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = feeincome * rates["5millimitpremiumratemb"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiummbclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiummbclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 2) //Standard L&H
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = feeincome * rates["1millimitpremiumrateslh"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiumslhclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiumslhclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = feeincome * rates["2millimitpremiumrateslh"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiumslhclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiumslhclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = feeincome * rates["3millimitpremiumrateslh"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiumslhclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiumslhclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = feeincome * rates["4millimitpremiumrateslh"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiumslhclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiumslhclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = feeincome * rates["5millimitpremiumrateslh"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiumslhclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiumslhclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 3) //L&H <5% F&G
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = feeincome * rates["1millimitpremiumratelhfg"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiumlhfgclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiumlhfgclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = feeincome * rates["2millimitpremiumratelhfg"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiumlhfgclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiumlhfgclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = feeincome * rates["3millimitpremiumratelhfg"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiumlhfgclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiumlhfgclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = feeincome * rates["4millimitpremiumratelhfg"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiumlhfgclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiumlhfgclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = feeincome * rates["5millimitpremiumratelhfg"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiumlhfgclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiumlhfgclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 4) //Investment
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = feeincome * rates["1millimitpremiumrateinv"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = feeincome * rates["2millimitpremiumrateinv"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = feeincome * rates["3millimitpremiumrateinv"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = feeincome * rates["4millimitpremiumrateinv"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = feeincome * rates["5millimitpremiumrateinv"] / 100 * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 5) //Highest investment percentage equal with L&H <5% F&G
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = (investmentfeeincome * rates["1millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["1millimitpremiumratelhfg"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvlass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = (investmentfeeincome * rates["2millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["2millimitpremiumratelhfg"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = (investmentfeeincome * rates["3millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["3millimitpremiumratelhfg"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = (investmentfeeincome * rates["4millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["4millimitpremiumratelhfg"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = (investmentfeeincome * rates["5millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["5millimitpremiumratelhfg"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 6) //Highest investment percentage equal with Standard L&H
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = (investmentfeeincome * rates["1millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["1millimitpremiumrateslh"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = (investmentfeeincome * rates["2millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["2millimitpremiumrateslh"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = (investmentfeeincome * rates["3millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["3millimitpremiumrateslh"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = (investmentfeeincome * rates["4millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["4millimitpremiumrateslh"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = (investmentfeeincome * rates["5millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["5millimitpremiumrateslh"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }
                else if (option == 7) //Highest investment percentage equal with Mortgage broking
                {
                    switch (termlimit)
                    {
                        case 1000000:
                            {
                                premium = (investmentfeeincome * rates["1millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["1millimitpremiumratemb"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["1millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 2000000:
                            {
                                premium = (investmentfeeincome * rates["2millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["2millimitpremiumratemb"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["2millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 3000000:
                            {
                                premium = (investmentfeeincome * rates["3millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["3millimitpremiumratemb"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["3millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 4000000:
                            {
                                premium = (investmentfeeincome * rates["4millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["4millimitpremiumratemb"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["4millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        case 5000000:
                            {
                                premium = (investmentfeeincome * rates["5millimitpremiumrateinv"] / 100 + remainingfeeincome * rates["5millimitpremiumratemb"] / 100) * (100 - discountrate) / 100;
                                if (intclasscategory == 1)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass1"];
                                }
                                else if (intclasscategory == 2)
                                {
                                    minimumpremium = rates["5millimitminimumpremiuminvclass2"];
                                }
                                premium = (premium > minimumpremium) ? premium : minimumpremium;
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }

            }

            return premium;
        }

        decimal GetPremiumOtherExcess(IDictionary<string, decimal> rates, decimal TermPremium1mil, int intclasscategory, int option, decimal TermExcess, bool boladvisortruststatus)
        {
            decimal excesspremium = 0M;
            decimal excessminimumpremium = 0M;
            decimal discountrate = 0M;

            if (intclasscategory == 1 || intclasscategory == 2)
            {
                if (boladvisortruststatus)
                {
                    switch (TermExcess)
                    {
                        case 10000:
                            {
                                discountrate = rates["10kexcessdiscountrateadvisortruststatus"];
                                break;
                            }
                        case 25000:
                            {
                                discountrate = rates["25kexcessdiscountrateadvisortruststatus"];
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
                    switch (TermExcess)
                    {
                        case 10000:
                            {
                                discountrate = rates["10kexcessdiscountrate"];
                                break;
                            }
                        case 25000:
                            {
                                discountrate = rates["25kexcessdiscountrate"];
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PI"));
                            }
                    }
                }

                if (option == 1) //Mortgage broking
                {
                    
                }


            }

            return excesspremium;
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

        void uwrfdualinsurance(User underwritingUser, ClientAgreement agreement, bool boldualinsurancereferral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdualinsurance" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfdualinsurance").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdualinsurance" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boldualinsurancereferral) //Dual Insurance referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdualinsurance" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdualinsurance" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfdualinsurance" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfclustergroups(User underwritingUser, ClientAgreement agreement, bool bolclustergroupsreferral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclustergroups" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclustergroups").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclustergroups" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolclustergroupsreferral) //Cluster Groups referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclustergroups" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclustergroups" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclustergroups" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfinvestmentactivity(User underwritingUser, ClientAgreement agreement, decimal decInv)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfinvestmentactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfinvestmentactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfinvestmentactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decInv > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfinvestmentactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfinvestmentactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfinvestmentactivity" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfotheractivity(User underwritingUser, ClientAgreement agreement, decimal decOther)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotheractivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decOther > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotheractivity" && cref.DateDeleted == null).Status = "";
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

        void uwrffgactivity(User underwritingUser, ClientAgreement agreement, decimal decFG, decimal feeincome)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffgactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decFG > 5 || (decFG * feeincome) > 125000)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffgactivity" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfhighfeeincome(User underwritingUser, ClientAgreement agreement, decimal feeincome)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighfeeincome" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhighfeeincome").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighfeeincome" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (feeincome > 2500000)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighfeeincome" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighfeeincome" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhighfeeincome" && cref.DateDeleted == null).Status = "";
                }

            }
        }


    }
}


