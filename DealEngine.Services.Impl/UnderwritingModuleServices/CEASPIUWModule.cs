using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class CEASPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public CEASPIUWModule()
        {
            Name = "CEAS_PI";
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

            //========================================

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "maximumfeeincome", "overseasturnoverloadingau", "overseasturnoverloadingasia", "overseasturnoverloadingwwexclusacanada",
                "CERate", "SERate", "MERate", "EERate", "HCVERate", "CheERate", "GSERate", "NERate", "EnvERate", "HFERate", "MarERate", "PSERate", "MinERate", "DPCERate", "ArchSchoolRate", 
                "ArchResidentialRate", "ArchOtherRate", "ArchConsultingOtherRate", "limitmultiplyer300k", "limitmultiplyer500k", "limitmultiplyer750k", "limitmultiplyer1mil", "limitmultiplyer1andhalfmil",
                "limitmultiplyer2mil", "limitmultiplyer2andhalfmil", "limitmultiplyer3mil", "limitmultiplyer4mil", "limitmultiplyer5mil", "limitmultiplyer6mil", "limitmultiplyer8mil", 
                "limitmultiplyer10mil", "CEMinPremium", "SEMinPremium", "MEMinPremium", "EEMinPremium", "HCVEMinPremium", "CheEMinPremium", "GSEMinPremium", "NEMinPremium", "EnvEMinPremium",
                "HFEMinPremium", "MarEMinPremium", "PSEMinPremium", "MinEMinPremium", "DPCEMinPremium", "ArchSchoolMinPremium", "ArchResidentialMinPremium", "ArchOtherMinPremium", 
                "ArchConsultingOtherMinPremium");

            //========================================

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


            //====================================================
            //terms calculation
          
            decimal feeincome = 0M;
            decimal lastyearfeeincome = 0M;
            decimal decAUPer = 0M;
            decimal decNZPer = 0M;
            decimal decAsiaPer = 0M;
            decimal decCentralSouthAmericaPer = 0M;
            decimal decEuropeAfricaPer = 0M;
            decimal decUSACanadaPer = 0M;

            decimal decAULoading = 0M;
            decimal decAsiaLoading = 0M;
            decimal decWWExclUSACanada = 0M;

            decimal decCE = 0M;
            decimal decSE = 0M;
            decimal decME = 0M;
            decimal decEE = 0M;
            decimal decHCVE = 0M;
            decimal decCheE = 0M;
            decimal decGSE = 0M;
            decimal decNE = 0M;
            decimal decEnvE = 0M;
            decimal decHFE = 0M;
            decimal decMarE = 0M;
            decimal decPSE = 0M;
            decimal decMinE = 0M;
            decimal decDPCE = 0M;
            decimal decArchSchool = 0M;
            decimal decArchResidential = 0M;
            decimal decArchOther = 0M;
            decimal decArchConsultingOther = 0M;


            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                feeincome = agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                lastyearfeeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;

                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (uISTerritory.Location == "New Zealand" && uISTerritory.Percentage > 0)
                    {
                        decNZPer = uISTerritory.Percentage;
                    } 
                    else if (uISTerritory.Location == "USA and Canada" && uISTerritory.Percentage > 0)
                    {
                        decUSACanadaPer = uISTerritory.Percentage;
                    }
                    else if (uISTerritory.Location == "Australia" && uISTerritory.Percentage > 0)
                    {
                        decAUPer = uISTerritory.Percentage;
                    }
                    else if (uISTerritory.Location == "Asia and the Pacific Islands" && uISTerritory.Percentage > 0)
                    {
                        decAsiaPer = uISTerritory.Percentage;
                    }
                    else if (uISTerritory.Location == "Central and South America" && uISTerritory.Percentage > 0)
                    {
                        decCentralSouthAmericaPer = uISTerritory.Percentage;
                    }
                    else if (uISTerritory.Location == "Europe and Africa" && uISTerritory.Percentage > 0)
                    {
                        decEuropeAfricaPer = uISTerritory.Percentage;
                    }
                }

                if (decAUPer > 0) {
                    decAULoading = rates["overseasturnoverloadingau"] / 100;
                }
                if (decAsiaPer > 0)
                {
                    decAsiaLoading = rates["overseasturnoverloadingasia"] / 100;
                }
                if ((decCentralSouthAmericaPer + decEuropeAfricaPer) > 0)
                {
                    decWWExclUSACanada = rates["overseasturnoverloadingwwexclusacanada"] / 100;
                }

                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUS0001") //Civil Engineering
                    {
                        decCE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0002") //Structural Engineering
                    {
                        decSE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0003") //Mechanical Engineering
                    {
                        decME = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0004") //Electrical Engineering
                    {
                        decEE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0005") //Heating, Cooling, Ventilation Engineering
                    {
                        decHCVE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0006") //Chemical Engineering 
                    {
                        decCheE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0007") //Geo-technical/Soil Engineering
                    {
                        decGSE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0008") //Nuclear Engineering 
                    {
                        decNE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0009") //Environmental Engineering 
                    {
                        decEnvE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0010") //Hydraulic & Fire Engineering 
                    {
                        decHFE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0011") //Marine Engineering 
                    {
                        decMarE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0012") //Plumbing Sevices Engineering 
                    {
                        decPSE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0013") //Mining Engineering 
                    {
                        decMinE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0014") //Design of Pollution control equipment 
                    {
                        decDPCE = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0015") //Architectural work - Schools
                    {
                        decArchSchool = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0016") //Architectural work - Residential
                    {
                        decArchResidential = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0017") //Architectural work - Other
                    {
                        decArchOther = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0018") //Any other consulting work
                    {
                        decArchConsultingOther = uISActivity.Percentage;
                    }

                }

            }

            decimal basepremium1mil = 0M;
            basepremium1mil = GetBasePremiumFor1milOption(rates, feeincome, decAULoading, decAsiaLoading, decWWExclUSACanada, decCE, decSE, decME, decEE, decHCVE, decCheE, decGSE, decNE, decEnvE, 
                decHFE, decMarE, decPSE, decMinE, decDPCE, decArchSchool, decArchResidential, decArchOther, decArchConsultingOther);

            //============================================================

            #region terms
            int intelectedlimit = 0;

            decimal TermPremiumDEFAULT = 0m;
            decimal TermBrokerageDEFAULT = 0m;
            int TermExcess = 0;

            int TermLimit300k = 300000;
            decimal TermLimit300kPremium = 0m;
            TermLimit300kPremium = basepremium1mil * rates["limitmultiplyer300k"];
            ClientAgreementTerm termsl300klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit300k, TermExcess);
            termsl300klimitoption.TermLimit = TermLimit300k;
            termsl300klimitoption.Premium = TermLimit300kPremium;
            termsl300klimitoption.BasePremium = TermLimit300kPremium;
            termsl300klimitoption.Excess = TermExcess;
            termsl300klimitoption.BrokerageRate = agreement.Brokerage;
            termsl300klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl300klimitoption.DateDeleted = null;
            termsl300klimitoption.DeletedBy = null;

            int TermLimit500k = 500000;
            decimal TermLimit500kPremium = 0m;
            TermLimit500kPremium = basepremium1mil * rates["limitmultiplyer500k"];
            ClientAgreementTerm termsl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit500k, TermExcess);
            termsl500klimitoption.TermLimit = TermLimit500k;
            termsl500klimitoption.Premium = TermLimit500kPremium;
            termsl500klimitoption.BasePremium = TermLimit500kPremium;
            termsl500klimitoption.Excess = TermExcess;
            termsl500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl500klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl500klimitoption.DateDeleted = null;
            termsl500klimitoption.DeletedBy = null;
            
            int TermLimit750k = 750000;
            decimal TermLimit750kPremium = 0m;
            TermLimit750kPremium = basepremium1mil * rates["limitmultiplyer750k"];
            ClientAgreementTerm termsl750klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit750k, TermExcess);
            termsl750klimitoption.TermLimit = TermLimit750k;
            termsl750klimitoption.Premium = TermLimit750kPremium;
            termsl750klimitoption.BasePremium = TermLimit750kPremium;
            termsl750klimitoption.Excess = TermExcess;
            termsl750klimitoption.BrokerageRate = agreement.Brokerage;
            termsl750klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl750klimitoption.DateDeleted = null;
            termsl750klimitoption.DeletedBy = null;
            
            int TermLimit1000k = 1000000;
            decimal TermLimit1milPremium = 0m;
            TermLimit1milPremium = basepremium1mil * rates["limitmultiplyer1mil"];
            ClientAgreementTerm termsl1000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1000k, TermExcess);
            termsl1000klimitoption.TermLimit = TermLimit1000k;
            termsl1000klimitoption.Premium = TermLimit1milPremium;
            termsl1000klimitoption.BasePremium = TermLimit1milPremium;
            termsl1000klimitoption.Excess = TermExcess;
            termsl1000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl1000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl1000klimitoption.DateDeleted = null;
            termsl1000klimitoption.DeletedBy = null;
            
            int TermLimit1500k = 1500000;
            decimal TermLimit1andhalfmilPremium = 0m;
            TermLimit1andhalfmilPremium = basepremium1mil * rates["limitmultiplyer1andhalfmil"];
            ClientAgreementTerm termsl1500klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1500k, TermExcess);
            termsl1500klimitoption.TermLimit = TermLimit1500k;
            termsl1500klimitoption.Premium = TermLimit1andhalfmilPremium;
            termsl1500klimitoption.BasePremium = TermLimit1andhalfmilPremium;
            termsl1500klimitoption.Excess = TermExcess;
            termsl1500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl1500klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl1500klimitoption.DateDeleted = null;
            termsl1500klimitoption.DeletedBy = null;
            
            int TermLimit2000k = 2000000;
            decimal TermLimit2milPremium = 0m;
            TermLimit2milPremium = basepremium1mil * rates["limitmultiplyer2mil"];
            ClientAgreementTerm termsl2000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2000k, TermExcess);
            termsl2000klimitoption.TermLimit = TermLimit2000k;
            termsl2000klimitoption.Premium = TermLimit2milPremium;
            termsl2000klimitoption.BasePremium = TermLimit2milPremium;
            termsl2000klimitoption.Excess = TermExcess;
            termsl2000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl2000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl2000klimitoption.DateDeleted = null;
            termsl2000klimitoption.DeletedBy = null;

            int TermLimit2500k = 2500000;
            decimal TermLimit2andhalfmilPremium = 0m;
            TermLimit2andhalfmilPremium = basepremium1mil * rates["limitmultiplyer2andhalfmil"];
            ClientAgreementTerm termsl2500klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2500k, TermExcess);
            termsl2500klimitoption.TermLimit = TermLimit2500k;
            termsl2500klimitoption.Premium = TermLimit2andhalfmilPremium;
            termsl2500klimitoption.BasePremium = TermLimit2andhalfmilPremium;
            termsl2500klimitoption.Excess = TermExcess;
            termsl2500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl2500klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl2500klimitoption.DateDeleted = null;
            termsl2500klimitoption.DeletedBy = null;

            int TermLimit3000k = 3000000;
            decimal TermLimit3milPremium = 0m;
            TermLimit3milPremium = basepremium1mil * rates["limitmultiplyer3mil"];
            ClientAgreementTerm termsl3000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit300k, TermExcess);
            termsl3000klimitoption.TermLimit = TermLimit3000k;
            termsl3000klimitoption.Premium = TermLimit3milPremium;
            termsl3000klimitoption.BasePremium = TermLimit3milPremium;
            termsl3000klimitoption.Excess = TermExcess;
            termsl3000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl3000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl3000klimitoption.DateDeleted = null;
            termsl3000klimitoption.DeletedBy = null;
            
            int TermLimit4000k = 4000000;
            decimal TermLimit4milPremium = 0m;
            TermLimit4milPremium = basepremium1mil * rates["limitmultiplyer4mil"];
            ClientAgreementTerm termsl4000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit4000k, TermExcess);
            termsl4000klimitoption.TermLimit = TermLimit4000k;
            termsl4000klimitoption.Premium = TermLimit4milPremium;
            termsl4000klimitoption.BasePremium = TermLimit4milPremium;
            termsl4000klimitoption.Excess = TermExcess;
            termsl4000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl4000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl4000klimitoption.DateDeleted = null;
            termsl4000klimitoption.DeletedBy = null;
            
            int TermLimit5000k = 5000000;
            decimal TermLimit5milPremium = 0m;
            TermLimit5milPremium = basepremium1mil * rates["limitmultiplyer5mil"];
            ClientAgreementTerm termsl5000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5000k, TermExcess);
            termsl5000klimitoption.TermLimit = TermLimit5000k;
            termsl5000klimitoption.Premium = TermLimit5milPremium;
            termsl5000klimitoption.BasePremium = TermLimit5milPremium;
            termsl5000klimitoption.Excess = TermExcess;
            termsl5000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl5000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl5000klimitoption.DateDeleted = null;
            termsl5000klimitoption.DeletedBy = null;
            
            int TermLimit6000k = 6000000;
            decimal TermLimit6milPremium = 0m;
            TermLimit6milPremium = basepremium1mil * rates["limitmultiplyer6mil"];
            ClientAgreementTerm termsl6000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit6000k, TermExcess);
            termsl6000klimitoption.TermLimit = TermLimit6000k;
            termsl6000klimitoption.Premium = TermLimit6milPremium;
            termsl6000klimitoption.BasePremium = TermLimit6milPremium;
            termsl6000klimitoption.Excess = TermExcess;
            termsl6000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl6000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl6000klimitoption.DateDeleted = null;
            termsl6000klimitoption.DeletedBy = null;
            
            int TermLimit8000k = 8000000;
            decimal TermLimit8milPremium = 0m;
            TermLimit8milPremium = basepremium1mil * rates["limitmultiplyer8mil"];
            ClientAgreementTerm termsl8000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit8000k, TermExcess);
            termsl8000klimitoption.TermLimit = TermLimit8000k;
            termsl8000klimitoption.Premium = TermLimit8milPremium;
            termsl8000klimitoption.BasePremium = TermLimit8milPremium;
            termsl8000klimitoption.Excess = TermExcess;
            termsl8000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl8000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl8000klimitoption.DateDeleted = null;
            termsl8000klimitoption.DeletedBy = null;
            
            int TermLimit10000k = 10000000;
            decimal TermLimit10milPremium = 0m;
            TermLimit10milPremium = basepremium1mil * rates["limitmultiplyer10mil"];
            ClientAgreementTerm termsl10000klimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit10000k, TermExcess);
            termsl10000klimitoption.TermLimit = TermLimit10000k;
            termsl10000klimitoption.Premium = TermLimit10milPremium;
            termsl10000klimitoption.BasePremium = TermLimit10milPremium;
            termsl10000klimitoption.Excess = TermExcess;
            termsl10000klimitoption.BrokerageRate = agreement.Brokerage;
            termsl10000klimitoption.Brokerage = TermBrokerageDEFAULT;
            termsl10000klimitoption.DateDeleted = null;
            termsl10000klimitoption.DeletedBy = null;

            if (termsl300klimitoption.Bound)
            {
                intelectedlimit = TermLimit300k;
            } 
            else if (termsl500klimitoption.Bound)
            {
                intelectedlimit = TermLimit500k;
            }
            else if (termsl750klimitoption.Bound)
            {
                intelectedlimit = TermLimit750k;
            }
            else if (termsl1000klimitoption.Bound)
            {
                intelectedlimit = TermLimit1000k;
            }
            else if (termsl1500klimitoption.Bound)
            {
                intelectedlimit = TermLimit1500k;
            }
            else if (termsl2000klimitoption.Bound)
            {
                intelectedlimit = TermLimit2000k;
            }
            else if (termsl2500klimitoption.Bound)
            {
                intelectedlimit = TermLimit2500k;
            }
            else if (termsl3000klimitoption.Bound)
            {
                intelectedlimit = TermLimit3000k;
            }
            else if (termsl4000klimitoption.Bound)
            {
                intelectedlimit = TermLimit4000k;
            }
            else if (termsl5000klimitoption.Bound)
            {
                intelectedlimit = TermLimit5000k;
            }
            else if (termsl6000klimitoption.Bound)
            {
                intelectedlimit = TermLimit6000k;
            }
            else if (termsl8000klimitoption.Bound)
            {
                intelectedlimit = TermLimit8000k;
            }
            else if (termsl10000klimitoption.Bound)
            {
                intelectedlimit = TermLimit10000k;
            }

            #endregion

            //=====================================================
            //Referral points per agreement
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //Members with fee income in excess of $4,000,000
            uwrfhighfeeincome(underwritingUser, agreement, feeincome, rates);
            //Operates in USA and Canada
            uwrfoperatesusaandcanada(underwritingUser, agreement, decUSACanadaPer);
            //Members with fee income over 20% derived from overseas activities (other than North America)
            uwrfoperatesoverseasexclusacanada(underwritingUser, agreement, decAUPer, decAsiaPer, decCentralSouthAmericaPer, decEuropeAfricaPer);
            //Members with a fee income above 250,000 and over 50% of fee income derived from Structural activities
            uwrffeeincomeover250kstructuralover50per(underwritingUser, agreement, feeincome, decSE);
            //Members with over 30% of fee income derived from Geotechnical activities 
            uwrfgeotechnicalover30per(underwritingUser, agreement, decGSE);
            //Members who undertake Design of Pollution control equipment  
            uwrfpollutionactivity(underwritingUser, agreement, decDPCE);
            //Activities noted as other 
            uwrfotheractivity(underwritingUser, agreement, decArchConsultingOther);
            //Current year fee is 25% greater or less than last year fee 
            uwrfcurrentandlastfeeincome(underwritingUser, agreement, feeincome, lastyearfeeincome);
            //Members who elect to take a limit of indemnity over $5,000,000  
            uwrfhigherlimit(underwritingUser, agreement, intelectedlimit);

            //=====================================================

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }


            string auditLogDetail = "CEAS PI UW created/modified";
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

        decimal GetBasePremiumFor1milOption(IDictionary<string, decimal> rates, decimal feeincome, decimal decAULoading, decimal decAsiaLoading, decimal decWWExclUSACanada, decimal decCE,
            decimal decSE, decimal decME, decimal decEE, decimal decHCVE, decimal decCheE, decimal decGSE, decimal decNE, decimal decEnvE, decimal decHFE, decimal decMarE, decimal decPSE, 
            decimal decMinE, decimal decDPCE, decimal decArchSchool, decimal decArchResidential, decimal decArchOther, decimal decArchConsultingOther)
        {
            decimal basepremium = 0M;
            decimal baseminpremium = 0M;

            decimal decCEMinPremium = 0M;
            decimal decSEMinPremium = 0M;
            decimal decMEMinPremium = 0M;
            decimal decEEMinPremium = 0M;
            decimal decHCVEMinPremium = 0M;
            decimal decCheEMinPremium = 0M;
            decimal decGSEMinPremium = 0M;
            decimal decNEMinPremium = 0M;
            decimal decEnvEMinPremium = 0M;
            decimal decHFEMinPremium = 0M;
            decimal decMarEMinPremium = 0M;
            decimal decPSEMinPremium = 0M;
            decimal decMinEMinPremium = 0M;
            decimal decDPCEMinPremium = 0M;
            decimal decArchSchoolMinPremium = 0M;
            decimal decArchResidentialMinPremium = 0M;
            decimal decArchOtherMinPremium = 0M;
            decimal decArchConsultingOtherMinPremium = 0M;

            if (decCE > 0)
            {
                basepremium += feeincome * decCE / 100 * rates["CERate"] / 100;
                decCEMinPremium = rates["CEMinPremium"];
            }
            if (decSE > 0)
            {
                basepremium += feeincome * decSE / 100 * rates["SERate"] / 100;
                decSEMinPremium = rates["SEMinPremium"];
            }
            if (decME > 0)
            {
                basepremium += feeincome * decME / 100 * rates["MERate"] / 100;
                decMEMinPremium = rates["MEMinPremium"];
            }
            if (decEE > 0)
            {
                basepremium += feeincome * decEE / 100 * rates["EERate"] / 100;
                decEEMinPremium = rates["EEMinPremium"];
            }
            if (decHCVE > 0)
            {
                basepremium += feeincome * decHCVE / 100 * rates["HCVERate"] / 100;
                decHCVEMinPremium = rates["HCVEMinPremium"];
            }
            if (decCheE > 0)
            {
                basepremium += feeincome * decCheE / 100 * rates["CheERate"] / 100;
                decCheEMinPremium = rates["CheEMinPremium"];
            }
            if (decGSE > 0)
            {
                basepremium += feeincome * decGSE / 100 * rates["GSERate"] / 100;
                decGSEMinPremium = rates["GSEMinPremium"];
            }
            if (decNE > 0)
            {
                basepremium += feeincome * decNE / 100 * rates["NERate"] / 100;
                decNEMinPremium = rates["NEMinPremium"];
            }
            if (decEnvE > 0)
            {
                basepremium += feeincome * decEnvE / 100 * rates["EnvERate"] / 100;
                decEnvEMinPremium = rates["EnvEMinPremium"];
            }
            if (decHFE > 0)
            {
                basepremium += feeincome * decHFE / 100 * rates["HFERate"] / 100;
                decHFEMinPremium = rates["HFEMinPremium"];
            }
            if (decMarE > 0)
            {
                basepremium += feeincome * decMarE / 100 * rates["MarERate"] / 100;
                decMarEMinPremium = rates["MarEMinPremium"];
            }
            if (decPSE > 0)
            {
                basepremium += feeincome * decPSE / 100 * rates["PSERate"] / 100;
                decPSEMinPremium = rates["PSEMinPremium"];
            }
            if (decMinE > 0)
            {
                basepremium += feeincome * decMinE / 100 * rates["MinERate"] / 100;
                decMinEMinPremium = rates["MinEMinPremium"];
            }
            if (decDPCE > 0)
            {
                basepremium += feeincome * decDPCE / 100 * rates["DPCERate"] / 100;
                decDPCEMinPremium = rates["DPCEMinPremium"];
            }
            if (decArchSchool > 0)
            {
                basepremium += feeincome * decArchSchool / 100 * rates["ArchSchoolRate"] / 100;
                decArchSchoolMinPremium = rates["ArchSchoolMinPremium"];
            }
            if (decArchResidential > 0)
            {
                basepremium += feeincome * decArchResidential / 100 * rates["ArchResidentialRate"] / 100;
                decArchResidentialMinPremium = rates["ArchResidentialMinPremium"];
            }
            if (decArchOther > 0)
            {
                basepremium += feeincome * decArchOther / 100 * rates["ArchOtherRate"] / 100;
                decArchOtherMinPremium = rates["ArchOtherMinPremium"];
            }
            if (decArchConsultingOther > 0)
            {
                basepremium += feeincome * decArchConsultingOther / 100 * rates["ArchConsultingOtherRate"] / 100;
                decArchConsultingOtherMinPremium = rates["ArchConsultingOtherMinPremium"];
            }

            baseminpremium = Math.Max(decCEMinPremium, Math.Max(decSEMinPremium, Math.Max(decMEMinPremium, Math.Max(decEEMinPremium, Math.Max(decHCVEMinPremium, Math.Max(decCheEMinPremium,
                            Math.Max(decGSEMinPremium, Math.Max(decNEMinPremium, Math.Max(decEnvEMinPremium, Math.Max(decHFEMinPremium, Math.Max(decMarEMinPremium, Math.Max(decPSEMinPremium,
                            Math.Max(decMinEMinPremium, Math.Max(decDPCEMinPremium, Math.Max(decArchSchoolMinPremium, Math.Max(decArchResidentialMinPremium,
                            Math.Max(decArchOtherMinPremium, decArchConsultingOtherMinPremium)))))))))))))))));

            basepremium *= 1 + decAULoading + decAsiaLoading + decWWExclUSACanada;

            basepremium = (basepremium > baseminpremium) ? basepremium : baseminpremium;

            return basepremium;
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
                    if (feeincome <= 0 || feeincome > rates["maximumfeeincome"])
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

        void uwrfoperatesusaandcanada(User underwritingUser, ClientAgreement agreement, decimal decUSACanadaPer)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesusaandcanada" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesusaandcanada").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesusaandcanada" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decUSACanadaPer > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesusaandcanada" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesusaandcanada" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesusaandcanada" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfoperatesoverseasexclusacanada(User underwritingUser, ClientAgreement agreement, decimal decAUPer, decimal decAsiaPer, decimal decCentralSouthAmericaPer, decimal decEuropeAfricaPer)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoverseasexclusacanada" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoverseasexclusacanada").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoverseasexclusacanada" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((decAUPer + decAsiaPer + decCentralSouthAmericaPer + decEuropeAfricaPer) > 20)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoverseasexclusacanada" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoverseasexclusacanada" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfoperatesoverseasexclusacanada" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrffeeincomeover250kstructuralover50per(User underwritingUser, ClientAgreement agreement, decimal feeincome, decimal decSE)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincomeover250kstructuralover50per" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffeeincomeover250kstructuralover50per").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincomeover250kstructuralover50per" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (feeincome > 250000 && decSE > 50)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincomeover250kstructuralover50per" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincomeover250kstructuralover50per" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffeeincomeover250kstructuralover50per" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfgeotechnicalover30per(User underwritingUser, ClientAgreement agreement, decimal decGSE)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfgeotechnicalover30per" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfgeotechnicalover30per").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfgeotechnicalover30per" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decGSE > 30)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfgeotechnicalover30per" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfgeotechnicalover30per" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfgeotechnicalover30per" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfpollutionactivity(User underwritingUser, ClientAgreement agreement, decimal decDPCE)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpollutionactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpollutionactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpollutionactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decDPCE > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpollutionactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpollutionactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpollutionactivity" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfotheractivity(User underwritingUser, ClientAgreement agreement, decimal decArchConsultingOther)
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
                    if (decArchConsultingOther > 0)
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

        void uwrfcurrentandlastfeeincome(User underwritingUser, ClientAgreement agreement, decimal feeincome, decimal lastyearfeeincome)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcurrentandlastfeeincome" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcurrentandlastfeeincome").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcurrentandlastfeeincome" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (feeincome > (lastyearfeeincome * 1.25M) || feeincome < (lastyearfeeincome * 0.75M))
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcurrentandlastfeeincome" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcurrentandlastfeeincome" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcurrentandlastfeeincome" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfhigherlimit(User underwritingUser, ClientAgreement agreement, int intelectedlimit)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhigherlimit" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhigherlimit").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhigherlimit" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (intelectedlimit > 5000000)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhigherlimit" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                    && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhigherlimit" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhigherlimit" && cref.DateDeleted == null).Status = "";
                }
            }
        }

    }
}
