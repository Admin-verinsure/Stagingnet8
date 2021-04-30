using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class ApolloPIFAPUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public ApolloPIFAPUWModule()
        {
            Name = "Apollo_PIFAP";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PIFAP" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm pifapterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PIFAP" && ct.DateDeleted == null))
                {
                    pifapterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "1millimit1kexcessfappremiumfor2advisor", "1millimit5kexcessfappremiumfor2advisor",
                "2millimit1kexcessfappremiumfor2advisor", "2millimit5kexcessfappremiumfor2advisor", "3millimit1kexcessfappremiumfor2advisor", "3millimit5kexcessfappremiumfor2advisor",
                "5millimit1kexcessfappremiumfor2advisor", "5millimit5kexcessfappremiumfor2advisor", "10millimit1kexcessfappremiumfor2advisor", "10millimit5kexcessfappremiumfor2advisor",
                "1millimit1kexcessfappremiumfor3advisor", "1millimit5kexcessfappremiumfor3advisor",
                "2millimit1kexcessfappremiumfor3advisor", "2millimit5kexcessfappremiumfor3advisor", "3millimit1kexcessfappremiumfor3advisor", "3millimit5kexcessfappremiumfor3advisor",
                "5millimit1kexcessfappremiumfor3advisor", "5millimit5kexcessfappremiumfor3advisor", "10millimit1kexcessfappremiumfor3advisor", "10millimit5kexcessfappremiumfor3advisor",
                "1millimit1kexcessfappremiumfor4advisor", "1millimit5kexcessfappremiumfor4advisor",
                "2millimit1kexcessfappremiumfor4advisor", "2millimit5kexcessfappremiumfor4advisor", "3millimit1kexcessfappremiumfor4advisor", "3millimit5kexcessfappremiumfor4advisor",
                "5millimit1kexcessfappremiumfor4advisor", "5millimit5kexcessfappremiumfor4advisor", "10millimit1kexcessfappremiumfor4advisor", "10millimit5kexcessfappremiumfor4advisor",
                "1millimit1kexcessfappremiumfor5advisor", "1millimit5kexcessfappremiumfor5advisor",
                "2millimit1kexcessfappremiumfor5advisor", "2millimit5kexcessfappremiumfor5advisor", "3millimit1kexcessfappremiumfor5advisor", "3millimit5kexcessfappremiumfor5advisor",
                "5millimit1kexcessfappremiumfor5advisor", "5millimit5kexcessfappremiumfor5advisor", "10millimit1kexcessfappremiumfor5advisor", "10millimit5kexcessfappremiumfor5advisor",
                "1millimit1kexcessfappremiumfor6advisor", "1millimit5kexcessfappremiumfor6advisor",
                "2millimit1kexcessfappremiumfor6advisor", "2millimit5kexcessfappremiumfor6advisor", "3millimit1kexcessfappremiumfor6advisor", "3millimit5kexcessfappremiumfor6advisor",
                "5millimit1kexcessfappremiumfor6advisor", "5millimit5kexcessfappremiumfor6advisor", "10millimit1kexcessfappremiumfor6advisor", "10millimit5kexcessfappremiumfor6advisor",
                "1millimit1kexcessfappremiumfor7advisor", "1millimit5kexcessfappremiumfor7advisor",
                "2millimit1kexcessfappremiumfor7advisor", "2millimit5kexcessfappremiumfor7advisor", "3millimit1kexcessfappremiumfor7advisor", "3millimit5kexcessfappremiumfor7advisor",
                "5millimit1kexcessfappremiumfor7advisor", "5millimit5kexcessfappremiumfor7advisor", "10millimit1kexcessfappremiumfor7advisor", "10millimit5kexcessfappremiumfor7advisor",
                "1millimit1kexcessfappremiumfor8advisor", "1millimit5kexcessfappremiumfor8advisor",
                "2millimit1kexcessfappremiumfor8advisor", "2millimit5kexcessfappremiumfor8advisor", "3millimit1kexcessfappremiumfor8advisor", "3millimit5kexcessfappremiumfor8advisor",
                "5millimit1kexcessfappremiumfor8advisor", "5millimit5kexcessfappremiumfor8advisor", "10millimit1kexcessfappremiumfor8advisor", "10millimit5kexcessfappremiumfor8advisor",
                "1millimit1kexcessfappremiumfor9advisor", "1millimit5kexcessfappremiumfor9advisor",
                "2millimit1kexcessfappremiumfor9advisor", "2millimit5kexcessfappremiumfor9advisor", "3millimit1kexcessfappremiumfor9advisor", "3millimit5kexcessfappremiumfor9advisor",
                "5millimit1kexcessfappremiumfor9advisor", "5millimit5kexcessfappremiumfor9advisor", "10millimit1kexcessfappremiumfor9advisor", "10millimit5kexcessfappremiumfor9advisor",
                "1millimit1kexcessfappremiumfor10advisor", "1millimit5kexcessfappremiumfor10advisor",
                "2millimit1kexcessfappremiumfor10advisor", "2millimit5kexcessfappremiumfor10advisor", "3millimit1kexcessfappremiumfor10advisor", "3millimit5kexcessfappremiumfor10advisor",
                "5millimit1kexcessfappremiumfor10advisor", "5millimit5kexcessfappremiumfor10advisor", "10millimit1kexcessfappremiumfor10advisor", "10millimit5kexcessfappremiumfor10advisor",
                "1millimit1kexcessfappremiumfor11advisor", "1millimit5kexcessfappremiumfor11advisor",
                "2millimit1kexcessfappremiumfor11advisor", "2millimit5kexcessfappremiumfor11advisor", "3millimit1kexcessfappremiumfor11advisor", "3millimit5kexcessfappremiumfor11advisor",
                "5millimit1kexcessfappremiumfor11advisor", "5millimit5kexcessfappremiumfor11advisor", "10millimit1kexcessfappremiumfor11advisor", "10millimit5kexcessfappremiumfor11advisor",
                "1millimit1kexcessfappremiumfor12advisor", "1millimit5kexcessfappremiumfor12advisor",
                "2millimit1kexcessfappremiumfor12advisor", "2millimit5kexcessfappremiumfor12advisor", "3millimit1kexcessfappremiumfor12advisor", "3millimit5kexcessfappremiumfor12advisor",
                "5millimit1kexcessfappremiumfor12advisor", "5millimit5kexcessfappremiumfor12advisor", "10millimit1kexcessfappremiumfor12advisor", "10millimit5kexcessfappremiumfor12advisor"
                );

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
                        
            int fapagreementperiodindays = 0;
            fapagreementperiodindays = (agreement.ExpiryDate - Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value)).Days;

            agreement.InceptionDate = Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value);

            int coverperiodindaysforchange = 0;
            if (DateTime.UtcNow > Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value))
            {
                coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;
            } else
            {
                coverperiodindaysforchange = (agreement.ExpiryDate - agreement.InceptionDate).Days;
            }
            

            string strProfessionalBusiness = "General Insurance Brokers, Life Agents, Investment Advisers, Financial Planning and Mortgage Broking, Consultants and Advisers in the sale of any financial product including referrals to other financial product providers.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;


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
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "PIFAP")
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

            int TermExcess1k = 1000;
            int TermExcess5k = 5000;

            if (intnumberofadvisors > 1)
            {
                int TermLimit1mil1kExcess = 1000000;
                decimal TermPremium1mil1kExcess = 0M;
                decimal TermBrokerage1mil1kExcess = 0M;
                TermPremium1mil1kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit1mil1kExcess, TermExcess1k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage1mil1kExcess = TermPremium1mil1kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term1millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit1mil1kExcess, TermExcess1k);
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
                TermPremium1mil5kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit1mil5kExcess, TermExcess5k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage1mil5kExcess = TermPremium1mil5kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term1millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit1mil5kExcess, TermExcess5k);
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
                TermPremium2mil1kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit2mil1kExcess, TermExcess1k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage2mil1kExcess = TermPremium2mil1kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term2millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit2mil1kExcess, TermExcess1k);
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
                TermPremium2mil5kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit2mil5kExcess, TermExcess5k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage2mil5kExcess = TermPremium2mil5kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term2millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit2mil5kExcess, TermExcess5k);
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
                TermPremium3mil1kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit3mil1kExcess, TermExcess1k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage3mil1kExcess = TermPremium3mil1kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term3millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit3mil1kExcess, TermExcess1k);
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
                TermPremium3mil5kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit3mil5kExcess, TermExcess5k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage3mil5kExcess = TermPremium3mil5kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term3millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit3mil5kExcess, TermExcess5k);
                term3millimit5kexcesspremiumoption.TermLimit = TermLimit2mil5kExcess;
                term3millimit5kexcesspremiumoption.Premium = TermPremium2mil5kExcess;
                term3millimit5kexcesspremiumoption.BasePremium = TermPremium2mil5kExcess;
                term3millimit5kexcesspremiumoption.Excess = TermExcess5k;
                term3millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
                term3millimit5kexcesspremiumoption.Brokerage = TermBrokerage2mil5kExcess;
                term3millimit5kexcesspremiumoption.DateDeleted = null;
                term3millimit5kexcesspremiumoption.DeletedBy = null;

                int TermLimit5mil1kExcess = 5000000;
                decimal TermPremium5mil1kExcess = 0M;
                decimal TermBrokerage5mil1kExcess = 0M;
                TermPremium5mil1kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit5mil1kExcess, TermExcess1k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage5mil1kExcess = TermPremium5mil1kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term5millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit5mil1kExcess, TermExcess1k);
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
                TermPremium5mil5kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit5mil5kExcess, TermExcess5k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage5mil5kExcess = TermPremium5mil5kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term5millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit5mil5kExcess, TermExcess5k);
                term5millimit5kexcesspremiumoption.TermLimit = TermLimit5mil5kExcess;
                term5millimit5kexcesspremiumoption.Premium = TermPremium5mil5kExcess;
                term5millimit5kexcesspremiumoption.BasePremium = TermPremium5mil5kExcess;
                term5millimit5kexcesspremiumoption.Excess = TermExcess5k;
                term5millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
                term5millimit5kexcesspremiumoption.Brokerage = TermBrokerage5mil5kExcess;
                term5millimit5kexcesspremiumoption.DateDeleted = null;
                term5millimit5kexcesspremiumoption.DeletedBy = null;

                int TermLimit10mil1kExcess = 10000000;
                decimal TermPremium10mil1kExcess = 0M;
                decimal TermBrokerage10mil1kExcess = 0M;
                TermPremium10mil1kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit10mil1kExcess, TermExcess1k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage10mil1kExcess = TermPremium10mil1kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term10millimit1kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit10mil1kExcess, TermExcess1k);
                term10millimit1kexcesspremiumoption.TermLimit = TermLimit10mil1kExcess;
                term10millimit1kexcesspremiumoption.Premium = TermPremium10mil1kExcess;
                term10millimit1kexcesspremiumoption.BasePremium = TermPremium10mil1kExcess;
                term10millimit1kexcesspremiumoption.Excess = TermExcess1k;
                term10millimit1kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
                term10millimit1kexcesspremiumoption.Brokerage = TermBrokerage10mil1kExcess;
                term10millimit1kexcesspremiumoption.DateDeleted = null;
                term10millimit1kexcesspremiumoption.DeletedBy = null;

                int TermLimit10mil5kExcess = 10000000;
                decimal TermPremium10mil5kExcess = 0M;
                decimal TermBrokerage10mil5kExcess = 0M;
                TermPremium10mil5kExcess = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit10mil5kExcess, TermExcess5k, agreementperiodindays, coverperiodindays, fapagreementperiodindays);
                TermBrokerage10mil5kExcess = TermPremium10mil5kExcess * agreement.Brokerage / 100;

                ClientAgreementTerm term10millimit5kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit10mil5kExcess, TermExcess5k);
                term10millimit5kexcesspremiumoption.TermLimit = TermLimit10mil5kExcess;
                term10millimit5kexcesspremiumoption.Premium = TermPremium10mil5kExcess;
                term10millimit5kexcesspremiumoption.BasePremium = TermPremium10mil5kExcess;
                term10millimit5kexcesspremiumoption.Excess = TermExcess5k;
                term10millimit5kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
                term10millimit5kexcesspremiumoption.Brokerage = TermBrokerage10mil5kExcess;
                term10millimit5kexcesspremiumoption.DateDeleted = null;
                term10millimit5kexcesspremiumoption.DeletedBy = null;

                //Change policy premium claculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
                    var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PIFAP"));
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
                            term10millimit1kexcesspremiumoption.PremiumDiffer = (TermPremium10mil1kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term10millimit1kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term10millimit1kexcesspremiumoption.TermLimit == term.TermLimit && term10millimit1kexcesspremiumoption.Excess == term.Excess)
                            {
                                term10millimit1kexcesspremiumoption.Bound = true;
                            }
                            if (term10millimit1kexcesspremiumoption.PremiumDiffer < 0)
                            {
                                term10millimit1kexcesspremiumoption.PremiumDiffer = 0;
                            }
                            term10millimit5kexcesspremiumoption.PremiumDiffer = (TermPremium10mil5kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term10millimit5kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term10millimit5kexcesspremiumoption.TermLimit == term.TermLimit && term10millimit5kexcesspremiumoption.Excess == term.Excess)
                            {
                                term10millimit5kexcesspremiumoption.Bound = true;
                            }
                            if (term10millimit5kexcesspremiumoption.PremiumDiffer < 0)
                            {
                                term10millimit5kexcesspremiumoption.PremiumDiffer = 0;
                            }

                        }

                    }
                }

            } 
            else
            {
                ClientAgreementTerm term0millimit0kexcesspremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", 0, 0);
                term0millimit0kexcesspremiumoption.TermLimit = 0;
                term0millimit0kexcesspremiumoption.Premium = 0;
                term0millimit0kexcesspremiumoption.BasePremium = 0;
                term0millimit0kexcesspremiumoption.Excess = 0;
                term0millimit0kexcesspremiumoption.BrokerageRate = agreement.Brokerage;
                term0millimit0kexcesspremiumoption.Brokerage = 0;
                term0millimit0kexcesspremiumoption.DateDeleted = null;
                term0millimit0kexcesspremiumoption.DeletedBy = null;

                //Change policy premium calculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
                    var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PIFAP"));
                    foreach (var term in PreviousAgreement.ClientAgreementTerms)
                    {
                        if (term.Bound)
                        {
                            var PreviousBoundPremium = term.Premium;
                            if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                            {
                                PreviousBoundPremium = term.BasePremium;
                            }
                            term0millimit0kexcesspremiumoption.PremiumDiffer = (0 - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term0millimit0kexcesspremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term0millimit0kexcesspremiumoption.TermLimit == term.TermLimit && term0millimit0kexcesspremiumoption.Excess == term.Excess)
                            {
                                term0millimit0kexcesspremiumoption.Bound = true;
                            }
                            if (term0millimit0kexcesspremiumoption.PremiumDiffer < 0)
                            {
                                term0millimit0kexcesspremiumoption.PremiumDiffer = 0;
                            }

                        }

                    }
                }
            }

            


            //Referral points per agreement
            //Advisor number over 12
            uwrfadvisornumberover12(underwritingUser, agreement, intnumberofadvisors);

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

            string auditLogDetail = "Apollo PIFAP UW created/modified";
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


        decimal GetPremiumForFAP(IDictionary<string, decimal> rates, int intnumberofadvisors, int limitoption, int excessoption, int agreementperiodindays, int coverperiodindays, int fapagreementperiodindays)
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
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor2advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor2advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor2advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor2advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor2advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor2advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor2advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor2advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor2advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor2advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                } 
                else if (intnumberofadvisors == 3)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor3advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor3advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor3advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor3advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor3advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor3advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor3advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor3advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor3advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor3advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 4)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor4advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor4advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor4advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor4advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor4advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor4advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor4advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor4advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor4advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor4advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 5)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor5advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor5advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor5advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor5advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor5advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor5advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor5advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor5advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor5advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor5advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 6)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor6advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor6advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor6advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor6advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor6advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor6advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor6advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor6advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor6advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor6advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 7)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor7advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor7advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor7advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor7advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor7advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor7advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor7advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor7advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor7advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor7advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 8)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor8advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor8advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor8advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor8advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor8advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor8advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor8advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor8advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor8advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor8advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 9)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor9advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor9advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor9advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor9advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor9advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor9advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor9advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor9advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor9advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor9advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 10)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor10advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor10advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor10advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor10advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor10advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor10advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor10advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor10advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor10advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor10advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 11)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor11advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor11advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor11advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor11advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor11advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor11advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor11advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor11advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor11advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor11advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
                else if (intnumberofadvisors == 12)
                {
                    switch (limitoption)
                    {
                        case 1000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["1millimit1kexcessfappremiumfor12advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["1millimit5kexcessfappremiumfor12advisor"];
                                }
                                break;
                            }
                        case 2000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["2millimit1kexcessfappremiumfor12advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["2millimit5kexcessfappremiumfor12advisor"];
                                }
                                break;
                            }
                        case 3000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["3millimit1kexcessfappremiumfor12advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["3millimit5kexcessfappremiumfor12advisor"];
                                }
                                break;
                            }
                        case 5000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["5millimit1kexcessfappremiumfor12advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["5millimit5kexcessfappremiumfor12advisor"];
                                }
                                break;
                            }
                        case 10000000:
                            {
                                if (excessoption == 1000)
                                {
                                    premiumoption = rates["10millimit1kexcessfappremiumfor12advisor"];
                                }
                                else if (excessoption == 5000)
                                {
                                    premiumoption = rates["10millimit5kexcessfappremiumfor12advisor"];
                                }
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }

            }

            premiumoption = premiumoption * fapagreementperiodindays / coverperiodindays;

            return premiumoption;
        }

        void uwrfadvisornumberover12(User underwritingUser, ClientAgreement agreement, int intnumberofadvisors)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover12" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover12" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (intnumberofadvisors > 12)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover12" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover12" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisornumberover12" && cref.DateDeleted == null).Status = "";
                }
            }
        }


    }
}

