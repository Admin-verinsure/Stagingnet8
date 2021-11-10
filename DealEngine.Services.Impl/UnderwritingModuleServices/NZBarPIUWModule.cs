using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZBarPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZBarPIUWModule()
        {
            Name = "NZBar_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "piadditionalpremiumjb", "piadditionalpremiumb",
                "pi2millimit5kexcesspremiumjb", "pi2millimit10kexcesspremiumjb", "pi5millimit5kexcesspremiumjb", "pi5millimit10kexcesspremiumjb",
                "pi2millimit5kexcesspremiumcb", "pi2millimit10kexcesspremiumcb", "pi5millimit5kexcesspremiumcb", "pi5millimit10kexcesspremiumcb", "pi10millimit5kexcesspremiumcb", "pi10millimit10kexcesspremiumcb",
                "pi2millimit5kexcesspremium", "pi2millimit10kexcesspremium", "pi5millimit5kexcesspremium", "pi5millimit10kexcesspremium", "pi10millimit5kexcesspremium", "pi10millimit10kexcesspremium");

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

            decimal decCriminalLaw = 0M;
            decimal decOther = 0M;
            int numberofaddjuniorbarrister = 0;
            int numberofaddbarrister = 0;
            bool bolworkoutsidenz = false;
            decimal feeincome = 0M;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
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
                    if (uISActivity.AnzsciCode == "CUSNZBar02") //Criminal Law
                    {
                        if (uISActivity.Percentage > 0)
                        {
                            decCriminalLaw += uISActivity.Percentage;
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUSNZBar20") //Other
                    {
                        if (uISActivity.Percentage > 0)
                            decOther = uISActivity.Percentage;
                    }
                }
            }

            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    if (uisorg.DateDeleted == null && !uisorg.Removed)
                    {
                        var addjuniorbarristerunit = (JBaristerUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "JBarrister" && u.DateDeleted == null);
                        if (addjuniorbarristerunit != null)
                        {
                            numberofaddjuniorbarrister += 1;
                        }

                        var addbarristerunit = (EBaristerUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "EBarrister" && u.DateDeleted == null);
                        if (addbarristerunit != null)
                        {
                            numberofaddbarrister += 1;
                        }
                    }
                }
            }

            //For 1st year set up
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



            //Check Junior Barrister information
            bool boljuniorbarrister = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.IsJuniorBarrister").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.IsJuniorBarrister").First().Value == "1")
                {
                    boljuniorbarrister = true;
                }
            }

            //Check financial member information
            bool bolfinancialmemberreferral = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.hasNzbar").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.hasNzbar").First().Value == "2")
                {
                    bolfinancialmemberreferral = true;
                }
            }

            //Check Fee Income greater than $1,000,000 information
            bool bolfeeincomereferral = false;
            if (feeincome >= 1000000)
            {
                bolfeeincomereferral = true;
            }

            //Check run off cver required
            bool bolrunoffcoverrequired = false;
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasRunOff").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HaveBarristerSole").First().Value == "2" && 
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasRunOff").First().Value == "1")
                {
                    bolrunoffcoverrequired = true;
                }
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPreviouslyUndertaken").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPreviouslyUndertaken").First().Value == "1")
                {
                    bolrunoffcoverrequired = true;
                }
            }
            

            //Calculate premium option

            //Junior Barrister Pricing and None Junior Barrister Pricing
            decimal decadditionbarpremium = 0M;
            //Check if the additional cover for Junior Barrister and Barrister required
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.IsRequirecoverJunior").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.IsRequirecoverJunior").First().Value == "1")
                {
                    decadditionbarpremium = rates["piadditionalpremiumjb"] * numberofaddjuniorbarrister + rates["piadditionalpremiumb"] * numberofaddbarrister;
                }
            }            

            int TermLimit2millimit5kexcess = 2000000;
            decimal TermPremium2millimit5kexcess = 0M;
            decimal TermBasePremium2millimit5kexcess = 0M;
            decimal TermBrokerage2millimit5kexcess = 0M;
            decimal TermExcess2millimit5kexcess = 5000;
            if (boljuniorbarrister)
            {
                TermPremium2millimit5kexcess = rates["pi2millimit5kexcesspremiumjb"];
            }
            else
            {
                if (decCriminalLaw >= 85)
                {
                    TermPremium2millimit5kexcess = rates["pi2millimit5kexcesspremiumcb"];
                }
                else
                {
                    TermPremium2millimit5kexcess = rates["pi2millimit5kexcesspremium"];
                }
                TermPremium2millimit5kexcess += decadditionbarpremium;
            }
            TermBasePremium2millimit5kexcess = TermPremium2millimit5kexcess;
            TermPremium2millimit5kexcess = TermPremium2millimit5kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage2millimit5kexcess = TermPremium2millimit5kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm termpitermoption2millimit5kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2millimit5kexcess, TermExcess2millimit5kexcess);
            termpitermoption2millimit5kexcess.TermLimit = TermLimit2millimit5kexcess;
            termpitermoption2millimit5kexcess.Premium = TermPremium2millimit5kexcess;
            termpitermoption2millimit5kexcess.BasePremium = TermBasePremium2millimit5kexcess;
            termpitermoption2millimit5kexcess.Excess = TermExcess2millimit5kexcess;
            termpitermoption2millimit5kexcess.BrokerageRate = agreement.Brokerage;
            termpitermoption2millimit5kexcess.Brokerage = TermBrokerage2millimit5kexcess;
            termpitermoption2millimit5kexcess.DateDeleted = null;
            termpitermoption2millimit5kexcess.DeletedBy = null;


            int TermLimit2millimit10kexcess = 2000000;
            decimal TermPremium2millimit10kexcess = 0M;
            decimal TermBasePremium2millimit10kexcess = 0M;
            decimal TermBrokerage2millimit10kexcess = 0M;
            decimal TermExcess2millimit10kexcess = 10000;
            if (boljuniorbarrister)
            {
                TermPremium2millimit10kexcess = rates["pi2millimit10kexcesspremiumjb"];
            }
            else
            {
                if (decCriminalLaw >= 85)
                {
                    TermPremium2millimit10kexcess = rates["pi2millimit10kexcesspremiumcb"];
                }
                else
                {
                    TermPremium2millimit10kexcess = rates["pi2millimit10kexcesspremium"];
                }
                TermPremium2millimit10kexcess += decadditionbarpremium;
            }            
            TermBasePremium2millimit10kexcess = TermPremium2millimit10kexcess;
            TermPremium2millimit10kexcess = TermPremium2millimit10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage2millimit10kexcess = TermPremium2millimit10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm termpitermoption2millimit10kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2millimit10kexcess, TermExcess2millimit10kexcess);
            termpitermoption2millimit10kexcess.TermLimit = TermLimit2millimit10kexcess;
            termpitermoption2millimit10kexcess.Premium = TermPremium2millimit10kexcess;
            termpitermoption2millimit10kexcess.BasePremium = TermBasePremium2millimit10kexcess;
            termpitermoption2millimit10kexcess.Excess = TermExcess2millimit10kexcess;
            termpitermoption2millimit10kexcess.BrokerageRate = agreement.Brokerage;
            termpitermoption2millimit10kexcess.Brokerage = TermBrokerage2millimit10kexcess;
            termpitermoption2millimit10kexcess.DateDeleted = null;
            termpitermoption2millimit10kexcess.DeletedBy = null;

            int TermLimit5millimit5kexcess = 5000000;
            decimal TermPremium5millimit5kexcess = 0M;
            decimal TermBasePremium5millimit5kexcess = 0M;
            decimal TermBrokerage5millimit5kexcess = 0M;
            decimal TermExcess5millimit5kexcess = 5000;
            if (boljuniorbarrister)
            {
                TermPremium5millimit5kexcess = rates["pi5millimit5kexcesspremiumjb"];
            }
            else
            {
                if (decCriminalLaw >= 85)
                {
                    TermPremium5millimit5kexcess = rates["pi5millimit5kexcesspremiumcb"];
                }
                else
                {
                    TermPremium5millimit5kexcess = rates["pi5millimit5kexcesspremium"];
                }
                TermPremium5millimit5kexcess += decadditionbarpremium;
            }
            TermBasePremium5millimit5kexcess = TermPremium5millimit5kexcess;
            TermPremium5millimit5kexcess = TermPremium5millimit5kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage5millimit5kexcess = TermPremium5millimit5kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm termpitermoption5millimit5kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5millimit5kexcess, TermExcess5millimit5kexcess);
            termpitermoption5millimit5kexcess.TermLimit = TermLimit5millimit5kexcess;
            termpitermoption5millimit5kexcess.Premium = TermPremium5millimit5kexcess;
            termpitermoption5millimit5kexcess.BasePremium = TermBasePremium5millimit5kexcess;
            termpitermoption5millimit5kexcess.Excess = TermExcess5millimit5kexcess;
            termpitermoption5millimit5kexcess.BrokerageRate = agreement.Brokerage;
            termpitermoption5millimit5kexcess.Brokerage = TermBrokerage5millimit5kexcess;
            termpitermoption5millimit5kexcess.DateDeleted = null;
            termpitermoption5millimit5kexcess.DeletedBy = null;


            int TermLimit5millimit10kexcess = 5000000;
            decimal TermPremium5millimit10kexcess = 0M;
            decimal TermBasePremium5millimit10kexcess = 0M;
            decimal TermBrokerage5millimit10kexcess = 0M;
            decimal TermExcess5millimit10kexcess = 10000;
            if (boljuniorbarrister)
            {
                TermPremium5millimit10kexcess = rates["pi5millimit10kexcesspremiumjb"];
            }
            else
            {
                if (decCriminalLaw >= 85)
                {
                    TermPremium5millimit10kexcess = rates["pi5millimit10kexcesspremiumcb"];
                }
                else
                {
                    TermPremium5millimit10kexcess = rates["pi5millimit10kexcesspremium"];
                }
                TermPremium5millimit10kexcess += decadditionbarpremium;
            }            
            TermBasePremium5millimit10kexcess = TermPremium5millimit10kexcess;
            TermPremium5millimit10kexcess = TermPremium5millimit10kexcess * agreementperiodindays / coverperiodindays;
            TermBrokerage5millimit10kexcess = TermPremium5millimit10kexcess * agreement.Brokerage / 100;

            ClientAgreementTerm termpitermoption5millimit10kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5millimit10kexcess, TermExcess5millimit10kexcess);
            termpitermoption5millimit10kexcess.TermLimit = TermLimit5millimit10kexcess;
            termpitermoption5millimit10kexcess.Premium = TermPremium5millimit10kexcess;
            termpitermoption5millimit10kexcess.BasePremium = TermBasePremium5millimit10kexcess;
            termpitermoption5millimit10kexcess.Excess = TermExcess5millimit10kexcess;
            termpitermoption5millimit10kexcess.BrokerageRate = agreement.Brokerage;
            termpitermoption5millimit10kexcess.Brokerage = TermBrokerage5millimit10kexcess;
            termpitermoption5millimit10kexcess.DateDeleted = null;
            termpitermoption5millimit10kexcess.DeletedBy = null;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                //set admin fee $0
                agreement.BrokerFee = 0M;

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
                        termpitermoption2millimit5kexcess.PremiumDiffer = (TermPremium2millimit5kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpitermoption2millimit5kexcess.PremiumPre = PreviousBoundPremium;
                        if (termpitermoption2millimit5kexcess.TermLimit == term.TermLimit && termpitermoption2millimit5kexcess.Excess == term.Excess)
                        {
                            termpitermoption2millimit5kexcess.Bound = true;
                        }
                        if (termpitermoption2millimit5kexcess.PremiumDiffer < 0)
                        {
                            termpitermoption2millimit5kexcess.PremiumDiffer = 0;
                        }
                        termpitermoption2millimit10kexcess.PremiumDiffer = (TermPremium2millimit10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpitermoption2millimit10kexcess.PremiumPre = PreviousBoundPremium;
                        if (termpitermoption2millimit10kexcess.TermLimit == term.TermLimit && termpitermoption2millimit10kexcess.Excess == term.Excess)
                        {
                            termpitermoption2millimit10kexcess.Bound = true;
                        }
                        if (termpitermoption2millimit10kexcess.PremiumDiffer < 0)
                        {
                            termpitermoption2millimit10kexcess.PremiumDiffer = 0;
                        }

                        termpitermoption5millimit5kexcess.PremiumDiffer = (TermPremium5millimit5kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpitermoption5millimit5kexcess.PremiumPre = PreviousBoundPremium;
                        if (termpitermoption5millimit5kexcess.TermLimit == term.TermLimit && termpitermoption5millimit5kexcess.Excess == term.Excess)
                        {
                            termpitermoption5millimit5kexcess.Bound = true;
                        }
                        if (termpitermoption5millimit5kexcess.PremiumDiffer < 0)
                        {
                            termpitermoption5millimit5kexcess.PremiumDiffer = 0;
                        }
                        termpitermoption5millimit10kexcess.PremiumDiffer = (TermPremium5millimit10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpitermoption5millimit10kexcess.PremiumPre = PreviousBoundPremium;
                        if (termpitermoption5millimit10kexcess.TermLimit == term.TermLimit && termpitermoption5millimit10kexcess.Excess == term.Excess)
                        {
                            termpitermoption5millimit10kexcess.Bound = true;
                        }
                        if (termpitermoption5millimit10kexcess.PremiumDiffer < 0)
                        {
                            termpitermoption5millimit10kexcess.PremiumDiffer = 0;
                        }
                    }

                }
            }

            if (!boljuniorbarrister) //None Junior Barrister Pricing
            {
                int TermLimit10millimit5kexcess = 10000000;
                decimal TermPremium10millimit5kexcess = 0M;
                decimal TermBasePremium10millimit5kexcess = 0M;
                decimal TermBrokerage10millimit5kexcess = 0M;
                decimal TermExcess10millimit5kexcess = 5000;
                if (decCriminalLaw >= 85)
                {
                    TermPremium10millimit5kexcess = rates["pi10millimit5kexcesspremiumcb"];
                }
                else
                {
                    TermPremium10millimit5kexcess = rates["pi10millimit5kexcesspremium"];
                }
                TermPremium10millimit5kexcess += decadditionbarpremium;
                TermBasePremium10millimit5kexcess = TermPremium10millimit5kexcess;
                TermPremium10millimit5kexcess = TermPremium10millimit5kexcess * agreementperiodindays / coverperiodindays;
                TermBrokerage10millimit5kexcess = TermPremium10millimit5kexcess * agreement.Brokerage / 100;

                ClientAgreementTerm termpitermoption10millimit5kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit10millimit5kexcess, TermExcess10millimit5kexcess);
                termpitermoption10millimit5kexcess.TermLimit = TermLimit10millimit5kexcess;
                termpitermoption10millimit5kexcess.Premium = TermPremium10millimit5kexcess;
                termpitermoption10millimit5kexcess.BasePremium = TermBasePremium10millimit5kexcess;
                termpitermoption10millimit5kexcess.Excess = TermExcess10millimit5kexcess;
                termpitermoption10millimit5kexcess.BrokerageRate = agreement.Brokerage;
                termpitermoption10millimit5kexcess.Brokerage = TermBrokerage10millimit5kexcess;
                termpitermoption10millimit5kexcess.DateDeleted = null;
                termpitermoption10millimit5kexcess.DeletedBy = null;


                int TermLimit10millimit10kexcess = 10000000;
                decimal TermPremium10millimit10kexcess = 0M;
                decimal TermBasePremium10millimit10kexcess = 0M;
                decimal TermBrokerage10millimit10kexcess = 0M;
                decimal TermExcess10millimit10kexcess = 10000;
                if (decCriminalLaw >= 85)
                {
                    TermPremium10millimit10kexcess = rates["pi10millimit10kexcesspremiumcb"];
                }
                else
                {
                    TermPremium10millimit10kexcess = rates["pi10millimit10kexcesspremium"];
                }
                TermPremium10millimit10kexcess += decadditionbarpremium;
                TermBasePremium10millimit10kexcess = TermPremium10millimit10kexcess;
                TermPremium10millimit10kexcess = TermPremium10millimit10kexcess * agreementperiodindays / coverperiodindays;
                TermBrokerage10millimit10kexcess = TermPremium10millimit10kexcess * agreement.Brokerage / 100;

                ClientAgreementTerm termpitermoption10millimit10kexcess = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit10millimit10kexcess, TermExcess10millimit10kexcess);
                termpitermoption10millimit10kexcess.TermLimit = TermLimit10millimit10kexcess;
                termpitermoption10millimit10kexcess.Premium = TermPremium10millimit10kexcess;
                termpitermoption10millimit10kexcess.BasePremium = TermBasePremium10millimit10kexcess;
                termpitermoption10millimit10kexcess.Excess = TermExcess10millimit10kexcess;
                termpitermoption10millimit10kexcess.BrokerageRate = agreement.Brokerage;
                termpitermoption10millimit10kexcess.Brokerage = TermBrokerage10millimit10kexcess;
                termpitermoption10millimit10kexcess.DateDeleted = null;
                termpitermoption10millimit10kexcess.DeletedBy = null;

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
                            termpitermoption10millimit5kexcess.PremiumDiffer = (TermPremium10millimit5kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            termpitermoption10millimit5kexcess.PremiumPre = PreviousBoundPremium;
                            if (termpitermoption10millimit5kexcess.TermLimit == term.TermLimit && termpitermoption10millimit5kexcess.Excess == term.Excess)
                            {
                                termpitermoption10millimit5kexcess.Bound = true;
                            }
                            if (termpitermoption10millimit5kexcess.PremiumDiffer < 0)
                            {
                                termpitermoption10millimit5kexcess.PremiumDiffer = 0;
                            }
                            termpitermoption10millimit10kexcess.PremiumDiffer = (TermPremium10millimit10kexcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            termpitermoption10millimit10kexcess.PremiumPre = PreviousBoundPremium;
                            if (termpitermoption10millimit10kexcess.TermLimit == term.TermLimit && termpitermoption10millimit10kexcess.Excess == term.Excess)
                            {
                                termpitermoption10millimit10kexcess.Bound = true;
                            }
                            if (termpitermoption10millimit10kexcess.PremiumDiffer < 0)
                            {
                                termpitermoption10millimit10kexcess.PremiumDiffer = 0;
                            }
                        }

                    }
                }

            }

            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //Financial Member of the New Zealand Bar Association
            uwrffinancialmember(underwritingUser, agreement, bolfinancialmemberreferral);
            //Annual Fee Income greater than $1,000,000
            uwrffeeincome(underwritingUser, agreement, bolfeeincomereferral);
            //Work Outside of NZ
            uwrfworkoutsidenz(underwritingUser, agreement, bolworkoutsidenz);
            //Other Activity
            uwrfotheractivity(underwritingUser, agreement, decOther);
            //Run off cover required
            uwrfrunoffcoverrequired(underwritingUser, agreement, bolrunoffcoverrequired);


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
            agreement.ProfessionalBusiness = "Barristers and other occupations agreed by NZI";
            string retrodate = "Unlimited but excluding known or reported incidents, not including any previous practice as a barrister & solicitor unless specifically agreed by Lumley";
            agreement.TerritoryLimit = "Worldwide excluding USA and Canada";
            agreement.Jurisdiction = "Worldwide excluding USA and Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            //Create agreement audit log
            string auditLogDetail = "NZBar PI UW created/modified";
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

        void uwrffinancialmember(User underwritingUser, ClientAgreement agreement, bool bolfinancialmemberreferral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialmember" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialmember").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialmember" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolfinancialmemberreferral) //Financial Member of the New Zealand Bar Association referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialmember" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialmember" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialmember" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrffeeincome(User underwritingUser, ClientAgreement agreement, bool bolfeeincomereferral)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincome" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincome").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincome" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolfeeincomereferral) //Annual Fee Income greater than $1,000,000 referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincome" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincome" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincome" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfworkoutsidenz(User underwritingUser, ClientAgreement agreement, bool bolworkoutsidenz)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfworkoutsidenz" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfworkoutsidenz").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfworkoutsidenz" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolworkoutsidenz) //Work outside of NZ referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfworkoutsidenz" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfworkoutsidenz" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfworkoutsidenz" && cref.DateDeleted == null).Status = "";
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

        void uwrfrunoffcoverrequired(User underwritingUser, ClientAgreement agreement, bool bolrunoffcoverrequired)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrunoffcoverrequired" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrunoffcoverrequired").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrunoffcoverrequired" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolrunoffcoverrequired) //Run off cover required referral
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrunoffcoverrequired" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrunoffcoverrequired" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrunoffcoverrequired" && cref.DateDeleted == null).Status = "";
                }
            }
        }


    }
}

