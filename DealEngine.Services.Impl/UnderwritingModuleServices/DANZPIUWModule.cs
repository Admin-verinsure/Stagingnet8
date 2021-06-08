using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class DANZPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public DANZPIUWModule()
        {
            Name = "DANZ_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "piBSSTopUpPremium", "piIRTopUpPremium", "piQSTopUpPremium", "piSETopUpPremium",
                "piTPTopUpPremium", "piValTopUpPremium", "piUPTopUpPremium", "piEPTopUpPremium", "piRMTopUpPremium",
                "piProjMTopUpPremium", "pitermexcess5000discount", "pitermexcess10kdiscount", 
                "pi500klimitincomeunder100kpremium", "pi500klimitincome100kto200kpremium", "pi500klimitincome200kto500kpremium",
                "pi1millimitincomeunder100kpremium", "pi1millimitincome100kto200kpremium", "pi1millimitincome200kto500kpremium", 
                "pi2millimitincomeunder100kpremium", "pi2millimitincome100kto200kpremium", "pi2millimitincome200kto500kpremium", "piwwextpremium", "maximumfeeincome");

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

            decimal feeincome = 0;

            decimal decBSS= 0M;
            decimal decArchRSD = 0M;
            decimal decArchRMD = 0M;
            decimal decArchCB = 0M;
            decimal decArchI = 0M;
            decimal decIR = 0M;
            decimal decQS = 0M;
            decimal decSE = 0M;
            decimal decTP = 0M;
            decimal decVal = 0M;
            decimal decUP = 0M;
            decimal decProjM = 0M;
            decimal decEP = 0M;
            decimal decRM = 0M;
            decimal decOther = 0M;

            decimal decBSSTopUpPre = 0M;
            decimal decIRTopUpPre = 0M;
            decimal decQSTopUpPre = 0M;
            decimal decSETopUpPre = 0M;
            decimal decTPTopUpPre = 0M;
            decimal decValTopUpPre = 0M;
            decimal decUPTopUpPre = 0M;
            decimal decProjMTopUpPre = 0M;
            decimal decEPTopUpPre = 0M;
            decimal decRMTopUpPre = 0M;

            decimal decPIPremiumTopUp = 0M;
            decimal totalfeeincome = 0M;
            int numberoffeeincome = 1;
            bool bolworkoutsidenz = false;
            bool bolsitelicensecategory3 = false;
            decimal extpremium = 0m;

            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {                
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    var unit = (PersonnelUnit)uisorg.OrganisationalUnits.FirstOrDefault(o => o.Name == "Personnel");
                    if(unit != null)
                    {
                        if (!bolsitelicensecategory3 && unit.SiteLicensed == "Category 3")
                        {
                            bolsitelicensecategory3 = true;
                        }
                    }
                }
            }


            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                totalfeeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;

                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0 && 
                    (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal) )
                {
                    totalfeeincome += agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                    numberoffeeincome += 1;
                }
                
                feeincome = totalfeeincome / numberoffeeincome;

                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (!bolworkoutsidenz && uISTerritory.Location != "New Zealand" && uISTerritory.Percentage > 0) //Work outside New Zealand Check
                    {
                        bolworkoutsidenz = true;
                    }
                }

                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "E322") //Building Structure Services
                    {
                        decBSS = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decBSSTopUpPre = rates["piBSSTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692121") //Architecture - residential single dwellings
                    {
                        decArchRSD = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M692122") //Architecture - residential multi dwellings
                    {
                        decArchRMD = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M692123") //Architecture - commercial building
                    {
                        decArchCB = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M692124") //Architecture - interior
                    {
                        decArchI = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M692160") //Inspection Reports 
                    {
                        decIR = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decIRTopUpPre = rates["piIRTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692170") //Quantity Surveying 
                    {
                        decQS = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decQSTopUpPre = rates["piQSTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692180") //Structural Engineering 
                    {
                        decSE = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decSETopUpPre = rates["piSETopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692190") //Town Planning 
                    {
                        decTP = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decTPTopUpPre = rates["piTPTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692210") //Valuations 
                    {
                        decVal = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decValTopUpPre = rates["piValTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692211") //Urban planning 
                    {
                        decUP = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decUPTopUpPre = rates["piUPTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692212") //Environmental planning 
                    {
                        decEP = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decEPTopUpPre = rates["piEPTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692213") //Resource management 
                    {
                        decRM = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decRMTopUpPre = rates["piRMTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "M692214") //Project management 
                    {
                        decProjM = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                            decProjMTopUpPre = rates["piProjMTopUpPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "S") //Other Services
                    {
                        decOther = uISActivity.Percentage;
                    }

                }

                decPIPremiumTopUp = Math.Max(decBSSTopUpPre, Math.Max(decIRTopUpPre, Math.Max(decQSTopUpPre, Math.Max(decSETopUpPre, Math.Max(decTPTopUpPre, Math.Max(decValTopUpPre, Math.Max(decUPTopUpPre, Math.Max(decProjMTopUpPre, Math.Max(decEPTopUpPre, decRMTopUpPre)))))))));
            }

            ClientAgreementEndorsement cAEIR = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Pre Purchase Inspections and Building Surveying Exclusion");
            if (cAEIR != null)
            {
                cAEIR.DateDeleted = DateTime.UtcNow;
                cAEIR.DeletedBy = underwritingUser;
            }
            if (decIR > 0)
            {
                if (cAEIR != null)
                {
                    cAEIR.DateDeleted = null;
                    cAEIR.DeletedBy = null;
                }
            }
            ClientAgreementEndorsement cAEQS = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Quantity Surveying Exclusion");
            if (cAEQS != null)
            {
                cAEQS.DateDeleted = DateTime.UtcNow;
                cAEQS.DeletedBy = underwritingUser;
            }
            if (decQS > 0)
            {
                if (cAEQS != null)
                {
                    cAEQS.DateDeleted = null;
                    cAEQS.DeletedBy = null;
                }
            }
            ClientAgreementEndorsement cAEProjM = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Project Managers Endorsement");
            if (cAEProjM != null)
            {
                cAEProjM.DateDeleted = DateTime.UtcNow;
                cAEProjM.DeletedBy = underwritingUser;
            }
            if (decProjM > 0)
            {
                if (cAEProjM != null)
                {
                    cAEProjM.DateDeleted = null;
                    cAEProjM.DeletedBy = null;
                }
            }
            ClientAgreementEndorsement cAEWTExt = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Leaky Building Write-Back Endorsement – Optional Extension Higher sub limits");
            if (cAEWTExt != null)
            {
                cAEWTExt.DateDeleted = DateTime.UtcNow;
                cAEWTExt.DeletedBy = underwritingUser;
            }
            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasLeakyBuildingCoverOptions").First().Value != null && 
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasLeakyBuildingCoverOptions").First().Value == "1")
            {
                extpremium = rates["piwwextpremium"];
                if (cAEWTExt != null)
                {
                    cAEWTExt.DateDeleted = null;
                    cAEWTExt.DeletedBy = null;
                }
            }

            bool bolcustomendorsementrenew = false;
            string strretrodate = "";

            //if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            //{
            //    foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
            //    {
            //        if (preRenewOrRefData.DataType == "preterm")
            //        {
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.PIRetro))
            //            {
            //                strretrodate = preRenewOrRefData.PIRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "PI")
            //        {
            //            if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
            //            {
            //                bolcustomendorsementrenew = true;
            //                ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
            //                agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
            //            }
            //        }
            //    }
            //}

            if (agreement.ClientInformationSheet.IsRenewawl && agreement.ClientInformationSheet.RenewFromInformationSheet != null)
            {
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PI"));

                if (renewFromAgreement != null)
                {
                    strretrodate = renewFromAgreement.RetroactiveDate;

                    foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
                    {
                        if (renewendorsement.DateDeleted == null &&
                            renewendorsement.Name != "Related or Associated Entities & Family Members" && renewendorsement.Name != "Non Imputation" &&
                            renewendorsement.Name != "Leaky Building Write-Back 2013 Endorsement" && renewendorsement.Name != "Design & Construct" &&
                            renewendorsement.Name != "DANZ Licensed Building Practitioners Complaints Endorsement" && renewendorsement.Name != "Pre Purchase Inspections and Building Surveying Exclusion" &&
                            renewendorsement.Name != "Quantity Surveying Exclusion" && renewendorsement.Name != "Project Managers Endorsement" &&
                            renewendorsement.Name != "Leaky Building Write-Back Endorsement – Optional Extension Higher sub limits")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                            bolcustomendorsementrenew = true;
                        }
                    }
                }
                
            }


            int TermLimit500k = 500000;
            int TermLimit1mil = 1000000;
            int TermLimit2mil = 2000000;

            int TermExcess2500 = 2500;
            int TermExcess5000 = 5000;
            decimal TermExcess5000Discount = rates["pitermexcess5000discount"];
            int TermExcess10k = 10000;
            decimal TermExcess10kDiscount = rates["pitermexcess10kdiscount"];

            decimal TermPremium500k2500Excess = 0m;
            decimal TermPremium500k5000Excess = 0m;
            decimal TermPremium500k10kExcess = 0m;
            decimal TermBrokerage500k2500Excess = 0m;
            decimal TermBrokerage500k5000Excess = 0m;
            decimal TermBrokerage500k10kExcess = 0m;

            TermPremium500k2500Excess = GetPremiumFor(rates, feeincome, TermLimit500k) + extpremium + decPIPremiumTopUp;
            TermPremium500k5000Excess = TermPremium500k2500Excess - TermExcess5000Discount;
            TermPremium500k10kExcess = TermPremium500k2500Excess - TermExcess10kDiscount;
            TermBrokerage500k2500Excess = TermPremium500k2500Excess * agreement.Brokerage / 100;
            TermBrokerage500k5000Excess = TermPremium500k5000Excess * agreement.Brokerage / 100;
            TermBrokerage500k10kExcess = TermPremium500k10kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term500klimit2500excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit500k, TermExcess2500);
            term500klimit2500excessoption.TermLimit = TermLimit500k;
            term500klimit2500excessoption.Premium = TermPremium500k2500Excess;
            term500klimit2500excessoption.Excess = TermExcess2500;
            term500klimit2500excessoption.BrokerageRate = agreement.Brokerage;
            term500klimit2500excessoption.Brokerage = TermBrokerage500k2500Excess;
            term500klimit2500excessoption.DateDeleted = null;
            term500klimit2500excessoption.DeletedBy = null;
            term500klimit2500excessoption.BasePremium = TermPremium500k2500Excess;

            ClientAgreementTerm term500klimit5000excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit500k, TermExcess5000);
            term500klimit5000excessoption.TermLimit = TermLimit500k;
            term500klimit5000excessoption.Premium = TermPremium500k5000Excess;
            term500klimit5000excessoption.Excess = TermExcess5000;
            term500klimit5000excessoption.BrokerageRate = agreement.Brokerage;
            term500klimit5000excessoption.Brokerage = TermBrokerage500k5000Excess;
            term500klimit5000excessoption.DateDeleted = null;
            term500klimit5000excessoption.DeletedBy = null;
            term500klimit5000excessoption.BasePremium = TermPremium500k5000Excess;

            ClientAgreementTerm term500klimit10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit500k, TermExcess10k);
            term500klimit10kexcessoption.TermLimit = TermLimit500k;
            term500klimit10kexcessoption.Premium = TermPremium500k10kExcess;
            term500klimit10kexcessoption.Excess = TermExcess10k;
            term500klimit10kexcessoption.BrokerageRate = agreement.Brokerage;
            term500klimit10kexcessoption.Brokerage = TermBrokerage500k10kExcess;
            term500klimit10kexcessoption.DateDeleted = null;
            term500klimit10kexcessoption.DeletedBy = null;
            term500klimit10kexcessoption.BasePremium = TermPremium500k10kExcess;

            decimal TermPremium1mil2500Excess = 0m;
            decimal TermPremium1mil5000Excess = 0m;
            decimal TermPremium1mil10kExcess = 0m;
            decimal TermBrokerage1mil2500Excess = 0m;
            decimal TermBrokerage1mil5000Excess = 0m;
            decimal TermBrokerage1mil10kExcess = 0m;

            TermPremium1mil2500Excess = GetPremiumFor(rates, feeincome, TermLimit1mil) + extpremium + decPIPremiumTopUp;
            TermPremium1mil5000Excess = TermPremium1mil2500Excess - TermExcess5000Discount;
            TermPremium1mil10kExcess = TermPremium1mil2500Excess - TermExcess10kDiscount;
            TermBrokerage1mil2500Excess = TermPremium1mil2500Excess * agreement.Brokerage / 100;
            TermBrokerage1mil5000Excess = TermPremium1mil5000Excess * agreement.Brokerage / 100;
            TermBrokerage1mil10kExcess = TermPremium1mil10kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimit2500excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess2500);
            term1millimit2500excessoption.TermLimit = TermLimit1mil;
            term1millimit2500excessoption.Premium = TermPremium1mil2500Excess;
            term1millimit2500excessoption.Excess = TermExcess2500;
            term1millimit2500excessoption.BrokerageRate = agreement.Brokerage;
            term1millimit2500excessoption.Brokerage = TermBrokerage1mil2500Excess;
            term1millimit2500excessoption.DateDeleted = null;
            term1millimit2500excessoption.DeletedBy = null;
            term1millimit2500excessoption.BasePremium = TermPremium1mil2500Excess;

            ClientAgreementTerm term1millimit5000excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess5000);
            term1millimit5000excessoption.TermLimit = TermLimit1mil;
            term1millimit5000excessoption.Premium = TermPremium1mil5000Excess;
            term1millimit5000excessoption.Excess = TermExcess5000;
            term1millimit5000excessoption.BrokerageRate = agreement.Brokerage;
            term1millimit5000excessoption.Brokerage = TermBrokerage1mil5000Excess;
            term1millimit5000excessoption.DateDeleted = null;
            term1millimit5000excessoption.DeletedBy = null;
            term1millimit5000excessoption.BasePremium = TermPremium1mil5000Excess;

            ClientAgreementTerm term1millimit10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess10k);
            term1millimit10kexcessoption.TermLimit = TermLimit1mil;
            term1millimit10kexcessoption.Premium = TermPremium1mil10kExcess;
            term1millimit10kexcessoption.Excess = TermExcess10k;
            term1millimit10kexcessoption.BrokerageRate = agreement.Brokerage;
            term1millimit10kexcessoption.Brokerage = TermBrokerage1mil10kExcess;
            term1millimit10kexcessoption.DateDeleted = null;
            term1millimit10kexcessoption.DeletedBy = null;
            term1millimit10kexcessoption.BasePremium = TermPremium1mil10kExcess;

            decimal TermPremium2mil2500Excess = 0m;
            decimal TermPremium2mil5000Excess = 0m;
            decimal TermPremium2mil10kExcess = 0m;
            decimal TermBrokerage2mil2500Excess = 0m;
            decimal TermBrokerage2mil5000Excess = 0m;
            decimal TermBrokerage2mil10kExcess = 0m;

            TermPremium2mil2500Excess = GetPremiumFor(rates, feeincome, TermLimit2mil) + extpremium + decPIPremiumTopUp;
            TermPremium2mil5000Excess = TermPremium2mil2500Excess - TermExcess5000Discount;
            TermPremium2mil10kExcess = TermPremium2mil2500Excess - TermExcess10kDiscount;
            TermBrokerage2mil2500Excess = TermPremium2mil2500Excess * agreement.Brokerage / 100;
            TermBrokerage2mil5000Excess = TermPremium2mil5000Excess * agreement.Brokerage / 100;
            TermBrokerage2mil10kExcess = TermPremium2mil10kExcess * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimit2500excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess2500);
            term2millimit2500excessoption.TermLimit = TermLimit2mil;
            term2millimit2500excessoption.Premium = TermPremium2mil2500Excess;
            term2millimit2500excessoption.Excess = TermExcess2500;
            term2millimit2500excessoption.BrokerageRate = agreement.Brokerage;
            term2millimit2500excessoption.Brokerage = TermBrokerage2mil2500Excess;
            term2millimit2500excessoption.DateDeleted = null;
            term2millimit2500excessoption.DeletedBy = null;
            term2millimit2500excessoption.BasePremium = TermPremium2mil2500Excess;

            ClientAgreementTerm term2millimit5000excessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess5000);
            term2millimit5000excessoption.TermLimit = TermLimit2mil;
            term2millimit5000excessoption.Premium = TermPremium2mil5000Excess;
            term2millimit5000excessoption.Excess = TermExcess5000;
            term2millimit5000excessoption.BrokerageRate = agreement.Brokerage;
            term2millimit5000excessoption.Brokerage = TermBrokerage2mil5000Excess;
            term2millimit5000excessoption.DateDeleted = null;
            term2millimit5000excessoption.DeletedBy = null;
            term2millimit5000excessoption.BasePremium = TermPremium2mil5000Excess;

            ClientAgreementTerm term2millimit10kexcessoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess10k);
            term2millimit10kexcessoption.TermLimit = TermLimit2mil;
            term2millimit10kexcessoption.Premium = TermPremium2mil10kExcess;
            term2millimit10kexcessoption.Excess = TermExcess10k;
            term2millimit10kexcessoption.BrokerageRate = agreement.Brokerage;
            term2millimit10kexcessoption.Brokerage = TermBrokerage2mil10kExcess;
            term2millimit10kexcessoption.DateDeleted = null;
            term2millimit10kexcessoption.DeletedBy = null;
            term2millimit10kexcessoption.BasePremium = TermPremium2mil10kExcess;

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
                        term500klimit2500excessoption.PremiumDiffer = (TermPremium500k2500Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term500klimit2500excessoption.PremiumPre = PreviousBoundPremium;
                        if (term500klimit2500excessoption.TermLimit == term.TermLimit && term500klimit2500excessoption.Excess == term.Excess)
                        {
                            term500klimit2500excessoption.Bound = true;
                        }
                        if (term500klimit2500excessoption.PremiumDiffer < 0)
                        {
                            term500klimit2500excessoption.PremiumDiffer = 0;
                        }
                        term500klimit5000excessoption.PremiumDiffer = (TermPremium500k5000Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term500klimit5000excessoption.PremiumPre = PreviousBoundPremium;
                        if (term500klimit5000excessoption.TermLimit == term.TermLimit && term500klimit5000excessoption.Excess == term.Excess)
                        {
                            term500klimit5000excessoption.Bound = true;
                        }
                        if (term500klimit5000excessoption.PremiumDiffer < 0)
                        {
                            term500klimit5000excessoption.PremiumDiffer = 0;
                        }
                        term500klimit10kexcessoption.PremiumDiffer = (TermPremium500k10kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term500klimit10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term500klimit10kexcessoption.TermLimit == term.TermLimit && term500klimit10kexcessoption.Excess == term.Excess)
                        {
                            term500klimit10kexcessoption.Bound = true;
                        }
                        if (term500klimit10kexcessoption.PremiumDiffer < 0)
                        {
                            term500klimit10kexcessoption.PremiumDiffer = 0;
                        }
                        term1millimit2500excessoption.PremiumDiffer = (TermPremium1mil2500Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimit2500excessoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimit2500excessoption.TermLimit == term.TermLimit && term1millimit2500excessoption.Excess == term.Excess)
                        {
                            term1millimit2500excessoption.Bound = true;
                        }
                        if (term1millimit2500excessoption.PremiumDiffer < 0)
                        {
                            term1millimit2500excessoption.PremiumDiffer = 0;
                        }
                        term1millimit5000excessoption.PremiumDiffer = (TermPremium1mil5000Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimit5000excessoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimit5000excessoption.TermLimit == term.TermLimit && term1millimit5000excessoption.Excess == term.Excess)
                        {
                            term1millimit5000excessoption.Bound = true;
                        }
                        if (term1millimit5000excessoption.PremiumDiffer < 0)
                        {
                            term1millimit5000excessoption.PremiumDiffer = 0;
                        }
                        term1millimit10kexcessoption.PremiumDiffer = (TermPremium1mil10kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimit10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimit10kexcessoption.TermLimit == term.TermLimit && term1millimit2500excessoption.Excess == term.Excess)
                        {
                            term1millimit10kexcessoption.Bound = true;
                        }
                        if (term1millimit10kexcessoption.PremiumDiffer < 0)
                        {
                            term1millimit10kexcessoption.PremiumDiffer = 0;
                        }
                        term2millimit2500excessoption.PremiumDiffer = (TermPremium2mil2500Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimit2500excessoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimit2500excessoption.TermLimit == term.TermLimit && term2millimit2500excessoption.Excess == term.Excess)
                        {
                            term2millimit2500excessoption.Bound = true;
                        }
                        if (term2millimit2500excessoption.PremiumDiffer < 0)
                        {
                            term2millimit2500excessoption.PremiumDiffer = 0;
                        }
                        term2millimit5000excessoption.PremiumDiffer = (TermPremium2mil5000Excess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimit5000excessoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimit5000excessoption.TermLimit == term.TermLimit && term2millimit5000excessoption.Excess == term.Excess)
                        {
                            term2millimit5000excessoption.Bound = true;
                        }
                        if (term2millimit5000excessoption.PremiumDiffer < 0)
                        {
                            term2millimit5000excessoption.PremiumDiffer = 0;
                        }
                        term2millimit10kexcessoption.PremiumDiffer = (TermPremium2mil10kExcess - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimit10kexcessoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimit10kexcessoption.TermLimit == term.TermLimit && term2millimit10kexcessoption.Excess == term.Excess)
                        {
                            term2millimit10kexcessoption.Bound = true;
                        }
                        if (term2millimit10kexcessoption.PremiumDiffer < 0)
                        {
                            term2millimit10kexcessoption.PremiumDiffer = 0;
                        }
                    }

                }

                if (PreviousAgreement != null)
                {
                    strretrodate = PreviousAgreement.RetroactiveDate;

                    foreach (var changeendorsement in PreviousAgreement.ClientAgreementEndorsements)
                    {
                        if (changeendorsement.DateDeleted == null &&
                            changeendorsement.Name != "Related or Associated Entities & Family Members" && changeendorsement.Name != "Non Imputation" &&
                            changeendorsement.Name != "Leaky Building Write-Back 2013 Endorsement" && changeendorsement.Name != "Design & Construct" &&
                            changeendorsement.Name != "DANZ Licensed Building Practitioners Complaints Endorsement" && changeendorsement.Name != "Pre Purchase Inspections and Building Surveying Exclusion" &&
                            changeendorsement.Name != "Quantity Surveying Exclusion" && changeendorsement.Name != "Project Managers Endorsement" &&
                            changeendorsement.Name != "Leaky Building Write-Back Endorsement – Optional Extension Higher sub limits")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, changeendorsement.Name, changeendorsement.Type, product, changeendorsement.Value, changeendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                            bolcustomendorsementrenew = true;
                        }
                    }
                }
            }


            //Referral points per agreement
            //Operates Outside of NZ
            uwrfoperatesoutsideofnz(underwritingUser, agreement, bolworkoutsidenz);
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //High Fee Income
            uwrfhighfeeincome(underwritingUser, agreement, feeincome, rates);
            //Negative Turnover
            //uwrfnegativefeeincome(underwritingUser, agreement, feeincome, rates);
            //Substancial Business Changes
            uwrfsubstancialbusinesschanges(underwritingUser, agreement);
            //Staff Dishonesty
            uwrfstaffdishonesty(underwritingUser, agreement);
            //Site License Category3
            uwrfsitelicensecategory3(underwritingUser, agreement, bolsitelicensecategory3);
            //DANZ Member
            uwrfnotdanzmember(underwritingUser, agreement);
            //Excluded Activities (I.e. Inspection reports, Valuation, Other)
            uwrfexcludedactivities(underwritingUser, agreement, decIR, decVal, decOther);
            //Over 10% Allied Professions Activities (Structural Engineer, Quantity Surveying, Building Services Engineer, Architect and Town Planning, Urban Planning , 
            //Environmental planning, Inspection reports, Project Management, Resource Management, Local government Policy advice and Valuations)
            uwrfover10perap(underwritingUser, agreement, decSE, decQS, decBSS, decTP, decUP, decEP, decIR, decProjM, decRM, decVal);
            //Custom Endorsement renew
            uwrfcustomendorsementrenew(underwritingUser, agreement, bolcustomendorsementrenew);
            //Not a renewal of an existing policy
            //uwrfnotrenewal(underwritingUser, agreement);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            agreement.ProfessionalBusiness = "Building Design Practitioner, Architectural Design, Mechanical Design, Electrical Design, Structural Design, Civil Design, Draughting and associated ancillary activities";
            string retrodate = "Policy Inception";
            agreement.TerritoryLimit = "Worldwide excluding USA/Canada";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            string auditLogDetail = "DANZ PI UW created/modified";
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
                } else
                {
                    if (DateTime.UtcNow > product.DefaultInceptionDate)
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


        decimal GetPremiumFor(IDictionary<string, decimal> rates, decimal feeincome, int limitoption)
        {
            decimal premiumoption = 0M;

            switch (limitoption)
            {
                case 500000:
                    {
                        if (feeincome >= 0 && feeincome <= 100000)
                        {
                            premiumoption = rates["pi500klimitincomeunder100kpremium"];
                        }
                        else if (feeincome > 100000 && feeincome <= 200000)
                        {
                            premiumoption = rates["pi500klimitincome100kto200kpremium"];
                        }
                        else if (feeincome > 200000 && feeincome <= 500000)
                        {
                            premiumoption = rates["pi500klimitincome200kto500kpremium"];
                        }
                        break;
                    }
                case 1000000:
                    {
                        if (feeincome >= 0 && feeincome <= 100000)
                        {
                            premiumoption = rates["pi1millimitincomeunder100kpremium"];
                        }
                        else if (feeincome > 100000 && feeincome <= 200000)
                        {
                            premiumoption = rates["pi1millimitincome100kto200kpremium"];
                        }
                        else if (feeincome > 200000 && feeincome <= 500000)
                        {
                            premiumoption = rates["pi1millimitincome200kto500kpremium"];
                        }
                        break;
                    }
                case 2000000:
                    {
                        if (feeincome >= 0 && feeincome <= 100000)
                        {
                            premiumoption = rates["pi2millimitincomeunder100kpremium"];
                        }
                        else if (feeincome > 100000 && feeincome <= 200000)
                        {
                            premiumoption = rates["pi2millimitincome100kto200kpremium"];
                        }
                        else if (feeincome > 200000 && feeincome <= 500000)
                        {
                            premiumoption = rates["pi2millimitincome200kto500kpremium"];
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format("Can not calculate premium for PI"));
                    }
            }

            return premiumoption;
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

        void uwrfsitelicensecategory3(User underwritingUser, ClientAgreement agreement, bool bolsitelicensecategory3)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsitelicensecategory3" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3v").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsitelicensecategory3").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsitelicensecategory3" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolsitelicensecategory3)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsitelicensecategory3" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsitelicensecategory3" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsitelicensecategory3" && cref.DateDeleted == null).Status = "";
                }
            }
         
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

        void uwrfhighfeeincome(User underwritingUser, ClientAgreement agreement, decimal feeincome, IDictionary<string, decimal> rates)
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
                    if (feeincome > rates["maximumfeeincome"])
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

        void uwrfnegativefeeincome(User underwritingUser, ClientAgreement agreement, decimal feeincome, IDictionary<string, decimal> rates)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnegativefeeincome" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnegativefeeincome").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnegativefeeincome" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (feeincome < 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnegativefeeincome" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnegativefeeincome" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnegativefeeincome" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfsubstancialbusinesschanges(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsubstancialbusinesschanges" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfsubstancialbusinesschanges").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsubstancialbusinesschanges" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasSubstantialChangeOptions").First().Value == "1")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsubstancialbusinesschanges" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsubstancialbusinesschanges" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfsubstancialbusinesschanges" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfstaffdishonesty(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfstaffdishonesty" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfstaffdishonesty").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfstaffdishonesty" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasPersonnelDismissedOptions").First().Value == "1")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfstaffdishonesty" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfstaffdishonesty" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfstaffdishonesty" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfnotdanzmember(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotdanzmember" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotdanzmember").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotdanzmember" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasDANZOptions").First().Value == "2")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotdanzmember" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotdanzmember" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotdanzmember" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfexcludedactivities(User underwritingUser, ClientAgreement agreement, decimal decIR, decimal decVal, decimal decOther)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfexcludedactivities" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfexcludedactivities").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfexcludedactivities" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decIR > 0 || decVal > 0 || decOther > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfexcludedactivities" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfexcludedactivities" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfexcludedactivities" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfover10perap(User underwritingUser, ClientAgreement agreement, decimal decSE, decimal decQS, decimal decBSS, decimal decTP, decimal decUP, decimal decEP, decimal decIR, decimal decProjM, decimal decRM, decimal decVal)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfover10perap" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfover10perap").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfover10perap" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decSE > 10 || decQS > 10 || decBSS > 10 || decTP > 10 || decUP > 10 || decEP > 10 || decIR > 10 || decProjM > 10 || decRM > 10 || decVal > 10)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfover10perap" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfover10perap" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfover10perap" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfcustomendorsementrenew(User underwritingUser, ClientAgreement agreement, bool bolcustomendorsementrenew)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcustomendorsementrenew" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcustomendorsementrenew") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").Name,
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

        void uwrfnotrenewal(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewal" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewal").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewal" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasExistingPolicyOptions").First().Value == "2")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewal" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewal" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewal" && cref.DateDeleted == null).Status = "";
                }
            }
        }

    }
}
