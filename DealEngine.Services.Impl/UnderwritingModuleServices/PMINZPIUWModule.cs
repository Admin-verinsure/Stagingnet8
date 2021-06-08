using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class PMINZPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public PMINZPIUWModule()
        {
            Name = "PMINZ_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "piitcomponentrateord", "piconstructioncomponentrateord", "pibusinessdevpmtcomponentrateord", "pimanufacturingcomponentrateord",
                "pifinancialcomponentrateord", "piothercomponentrateord", "piitcomponentminpremiumord", "piconstructioncomponentminpremiumord", "pibusinessdevpmtcomponentminpremiumord",
                "pimanufacturingcomponentminpremiumord", "pifinancialcomponentminpremiumord", "piothercomponentminpremiumord",
                "piitcomponentratecapm", "piconstructioncomponentratecapm", "pibusinessdevpmtcomponentratecapm", "pimanufacturingcomponentratecapm",
                "pifinancialcomponentratecapm", "piothercomponentratecapm", "piitcomponentminpremiumcapm", "piconstructioncomponentminpremiumcapm", "pibusinessdevpmtcomponentminpremiumcapm",
                "pimanufacturingcomponentminpremiumcapm", "pifinancialcomponentminpremiumcapm", "piothercomponentminpremiumcapm",
                "piitcomponentratepmp", "piconstructioncomponentratepmp", "pibusinessdevpmtcomponentratepmp", "pimanufacturingcomponentratepmp",
                "pifinancialcomponentratepmp", "piothercomponentratepmp", "piitcomponentminpremiumpmp", "piconstructioncomponentminpremiumpmp", "pibusinessdevpmtcomponentminpremiumpmp",
                "pimanufacturingcomponentminpremiumpmp", "pifinancialcomponentminpremiumpmp", "piothercomponentminpremiumpmp",
                "piitcomponentratepd", "piconstructioncomponentratepd", "pibusinessdevpmtcomponentratepd", "pimanufacturingcomponentratepd",
                "pifinancialcomponentratepd", "piothercomponentratepd", "piitcomponentminpremiumpd", "piconstructioncomponentminpremiumpd", "pibusinessdevpmtcomponentminpremiumpd",
                "pimanufacturingcomponentminpremiumpd", "pifinancialcomponentminpremiumpd", "piothercomponentminpremiumpd",
                "pi1millimitloadingrate", "pi2millimitloadingrate", "pi5millimitloadingrate", "piminbrokerage", "maximumnumberofpersonnel", "maximumfeeincome", "exppremthresholdgreaterthan",
                "exppremthresholdlessthan");

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

            int TermExcess = 2000;
            decimal feeincome = 0;

            int totalnumberofpersonnel = 0;
            int intordnumber = 0;
            int intcapmnumber = 0;
            int intpmpnumber = 0;
            int intpdnumber = 0;
            bool bolnonpmimember = false;

            decimal decBDSP = 0M;
            decimal decCon = 0M;
            decimal decDM = 0M;
            decimal decFASA = 0M;
            decimal decIT = 0M;
            decimal decMOP = 0M;
            decimal decPMTC = 0M;
            decimal decRCIM = 0M;
            decimal decTM = 0M;
            decimal decOPMA = 0M;
            decimal decNPMA = 0M;
            decimal decOther = 0M;
            decimal decSumActivity = 0M;
            decimal decPRBDSP = 0M;
            decimal decPRCon = 0M;
            decimal decPRFASA = 0M;
            decimal decPRIT = 0M;
            decimal decPRMOP = 0M;
            decimal decPROther = 0M;

            decimal decPIBasePremium = 0M;

            decimal totalfeeincome = 0M;
            int numberoffeeincome = 1;
            bool bolworkoutsidenz = false;
            bool constructionEngineerDetails = false;
            bool bolrenewalpremiumslowerthanexpiring = false;
            bool bolrenewalpremiumshigherthanexpiring = false;

            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    var unit = (PersonnelUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Personnel");
                    if(unit != null)
                    {
                        if (!bolnonpmimember && unit.IsCurrentMembershipPMINZ)
                        {
                            bolnonpmimember = true;
                        }
                        if (unit.CertType == "Ordinary")
                        {
                            intordnumber += 1;
                        }
                        else if (unit.CertType == "PMP")
                        {
                            intpmpnumber += 1;
                        }
                        else if (unit.CertType == "CAPM")
                        {
                            intcapmnumber += 1;
                        }
                        else if (unit.CertType == "ProjectDirector")
                        {
                            intpdnumber += 1;
                        }
                    }                    
                }

                totalnumberofpersonnel = intordnumber + intpmpnumber + intcapmnumber + intpdnumber;
            }


            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                totalfeeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0)
                {
                    totalfeeincome += agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                    numberoffeeincome += 1;
                }
                if (agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal > 0)
                {
                    totalfeeincome += agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal;
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
                    if (uISActivity.AnzsciCode == "M696210") //Business Development & Strategic Planning
                    {
                        decBDSP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "E") //Construction
                    {
                        decCon = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696230") //Design Management
                    {
                        decDM = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696240") //Financial and Accounting Systems Analysis
                    {
                        decFASA = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696250") //Information Technology
                    {
                        decIT = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696260") //Manufacturing & Operational Processes
                    {
                        decMOP = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696270") //Project Management Teaching and Coaching
                    {
                        decPMTC = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696300") //Resource Consent and Implementation Management
                    {
                        decRCIM = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696400") //Telecommunications Management
                    {
                        decTM = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696500") //Other Project Management Activities
                    {
                        decOPMA = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "M696600") //Non Project Management Activities
                    {
                        decNPMA = uISActivity.Percentage;
                    }

                }

                decOther = decPMTC + decOPMA + decDM + decTM + decRCIM;
                decSumActivity = decBDSP + decCon + decIT + decMOP + decNPMA + decOther;

                decPRBDSP = decBDSP / (decSumActivity - decNPMA) * decSumActivity;
                decPRCon = decCon / (decSumActivity - decNPMA) * decSumActivity;
                decPRFASA = decFASA / (decSumActivity - decNPMA) * decSumActivity;
                decPRIT = decIT / (decSumActivity - decNPMA) * decSumActivity;
                decPRMOP = decMOP / (decSumActivity - decNPMA) * decSumActivity;
                decPROther = decOther / (decSumActivity - decNPMA) * decSumActivity;

                if (decCon > 0 && !string.IsNullOrEmpty(agreement.ClientInformationSheet.RevenueData.AdditionalActivityInformation.ConstructionEngineerDetails))
                {
                    constructionEngineerDetails = true;
                }
            }

            ClientAgreementEndorsement cAEConstruction = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Project Managers (Construction)");
            ClientAgreementEndorsement cAENonConstruction = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Project Managers (Non-Construction)");

            if (cAEConstruction != null)
            {
                cAEConstruction.DateDeleted = DateTime.UtcNow;
                cAEConstruction.DeletedBy = underwritingUser;
            }
            if (cAENonConstruction != null)
            {
                cAENonConstruction.DateDeleted = DateTime.UtcNow;
                cAENonConstruction.DeletedBy = underwritingUser;
            }
            if (decCon > 0)
            {
                if (cAEConstruction != null)
                {
                    cAEConstruction.DateDeleted = null;
                    cAEConstruction.DeletedBy = null;
                }
            }
            else
            {
                if (cAENonConstruction != null)
                {
                    cAENonConstruction.DateDeleted = null;
                    cAENonConstruction.DeletedBy = null;
                }
            }

            decPIBasePremium = GetPIBasePremiumFor(rates, feeincome, decPRBDSP, decPRCon, decPRFASA, decPRIT, decPRMOP, decPROther, intordnumber, intpmpnumber, intcapmnumber, intpdnumber);

            decimal MinBrokerage = rates["piminbrokerage"];

            int intexpiringlimit = 0;
            decimal decexpiringpremium = 0m;
            string strretrodate = "";

            //if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            //{
            //    foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
            //    {
            //        if (preRenewOrRefData.DataType == "preterm")
            //        {
            //            intexpiringlimit = Convert.ToInt32(preRenewOrRefData.PIBoundLimit);
            //            decexpiringpremium = Convert.ToDecimal(preRenewOrRefData.PIBoundPremium);
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.PIRetro))
            //            {
            //                strretrodate = preRenewOrRefData.PIRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "PI")
            //        {
            //            if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
            //            {
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

                        if (renewendorsement.DateDeleted == null && renewendorsement.Name != "Project Managers (Construction)" &&
                            renewendorsement.Name != "Project Managers (Non-Construction)" &&
                            renewendorsement.Name != "Building Defects Extension")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }


            }

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = decPIBasePremium * (1 + rates["pi1millimitloadingrate"] / 100);
            decimal TermBrokerage1mil = 0m;
            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;
            decimal TopupBrokerage1mil = 0m;
            TopupBrokerage1mil = (TermBrokerage1mil > MinBrokerage) ? 0M : (MinBrokerage - TermBrokerage1mil);

            TermPremium1mil = Math.Round(Math.Ceiling((TermPremium1mil + TopupBrokerage1mil) / 10), 0) * 10;
            ClientAgreementTerm term1millimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess);
            term1millimitoption.TermLimit = TermLimit1mil;
            term1millimitoption.Premium = TermPremium1mil;
            term1millimitoption.Excess = TermExcess;
            term1millimitoption.BrokerageRate = agreement.Brokerage;
            term1millimitoption.Brokerage = (TermBrokerage1mil > MinBrokerage) ? TermBrokerage1mil : MinBrokerage;
            term1millimitoption.DateDeleted = null;
            term1millimitoption.DeletedBy = null;
            term1millimitoption.BasePremium = TermPremium1mil;

            if (!bolrenewalpremiumslowerthanexpiring && intexpiringlimit == 1000000 && (TermPremium1mil + TopupBrokerage1mil) < (decexpiringpremium * (1 - rates["exppremthresholdgreaterthan"] / 100)))
            {
                bolrenewalpremiumslowerthanexpiring = true;
            }
            if (!bolrenewalpremiumshigherthanexpiring && intexpiringlimit == 1000000 && (TermPremium1mil + TopupBrokerage1mil) > (decexpiringpremium * (1 + rates["exppremthresholdlessthan"] / 100)))
            {
                bolrenewalpremiumshigherthanexpiring = true;
            }

            int TermLimit2mil = 2000000;
            decimal TermPremium2mil = decPIBasePremium * (1 + rates["pi2millimitloadingrate"] / 100);
            decimal TermBrokerage2mil = 0m;
            TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;
            decimal TopupBrokerage2mil = 0m;
            TopupBrokerage2mil = (TermBrokerage2mil > MinBrokerage) ? 0M : (MinBrokerage - TermBrokerage2mil);

            TermPremium2mil = Math.Round(Math.Ceiling((TermPremium2mil + TopupBrokerage2mil) / 10), 0) * 10;
            ClientAgreementTerm term2millimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess);
            term2millimitoption.TermLimit = TermLimit2mil;
            term2millimitoption.Premium = TermPremium2mil;
            term2millimitoption.Excess = TermExcess;
            term2millimitoption.BrokerageRate = agreement.Brokerage;
            term2millimitoption.Brokerage = (TermBrokerage2mil > MinBrokerage) ? TermBrokerage2mil : MinBrokerage;
            term2millimitoption.DateDeleted = null;
            term2millimitoption.DeletedBy = null;
            term2millimitoption.BasePremium = TermPremium2mil;

            if (!bolrenewalpremiumslowerthanexpiring && intexpiringlimit == 2000000 && (TermPremium2mil + TopupBrokerage2mil) < (decexpiringpremium * (1 - rates["exppremthresholdgreaterthan"] / 100)))
            {
                bolrenewalpremiumslowerthanexpiring = true;
            }
            if (!bolrenewalpremiumshigherthanexpiring && intexpiringlimit == 2000000 && (TermPremium2mil + TopupBrokerage2mil) > (decexpiringpremium * (1 + rates["exppremthresholdlessthan"] / 100)))
            {
                bolrenewalpremiumshigherthanexpiring = true;
            }

            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = decPIBasePremium * (1 + rates["pi5millimitloadingrate"] / 100);
            decimal TermBrokerage5mil = 0m;
            TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;
            decimal TopupBrokerage5mil = 0m;
            TopupBrokerage5mil = (TermBrokerage5mil > MinBrokerage) ? 0M : (MinBrokerage - TermBrokerage5mil);

            TermPremium5mil = Math.Round(Math.Ceiling((TermPremium5mil + TopupBrokerage5mil) / 10), 0) * 10;
            ClientAgreementTerm term5millimitoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess);
            term5millimitoption.TermLimit = TermLimit5mil;
            term5millimitoption.Premium = TermPremium5mil;
            term5millimitoption.Excess = TermExcess;
            term5millimitoption.BrokerageRate = agreement.Brokerage;
            term5millimitoption.Brokerage = (TermBrokerage5mil > MinBrokerage) ? TermBrokerage5mil : MinBrokerage;
            term5millimitoption.DateDeleted = null;
            term5millimitoption.DeletedBy = null;
            term5millimitoption.BasePremium = TermPremium5mil;

            if (!bolrenewalpremiumslowerthanexpiring && intexpiringlimit == 5000000 && (TermPremium5mil + TopupBrokerage5mil) < (decexpiringpremium * (1 - rates["exppremthresholdgreaterthan"] / 100)))
            {
                bolrenewalpremiumslowerthanexpiring = true;
            }
            if (!bolrenewalpremiumshigherthanexpiring && intexpiringlimit == 5000000 && (TermPremium5mil + TopupBrokerage5mil) > (decexpiringpremium * (1 + rates["exppremthresholdlessthan"] / 100)))
            {
                bolrenewalpremiumshigherthanexpiring = true;
            }

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
                        term1millimitoption.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term1millimitoption.PremiumPre = PreviousBoundPremium;
                        if (term1millimitoption.TermLimit == term.TermLimit && term1millimitoption.Excess == term.Excess)
                        {
                            term1millimitoption.Bound = true;
                        }
                        if (term1millimitoption.PremiumDiffer < 0)
                        {
                            term1millimitoption.PremiumDiffer = 0;
                        }
                        term2millimitoption.PremiumDiffer = (TermPremium2mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term2millimitoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimitoption.TermLimit == term.TermLimit && term2millimitoption.Excess == term.Excess)
                        {
                            term2millimitoption.Bound = true;
                        }
                        if (term2millimitoption.PremiumDiffer < 0)
                        {
                            term2millimitoption.PremiumDiffer = 0;
                        }
                        term5millimitoption.PremiumDiffer = (TermPremium5mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        term5millimitoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimitoption.TermLimit == term.TermLimit && term5millimitoption.Excess == term.Excess)
                        {
                            term5millimitoption.Bound = true;
                        }
                        if (term5millimitoption.PremiumDiffer < 0)
                        {
                            term5millimitoption.PremiumDiffer = 0;
                        }

                    }

                }

                if (PreviousAgreement != null)
                {
                    strretrodate = PreviousAgreement.RetroactiveDate;

                    foreach (var changeendorsement in PreviousAgreement.ClientAgreementEndorsements)
                    {

                        if (changeendorsement.DateDeleted == null && changeendorsement.Name != "Project Managers (Construction)" &&
                            changeendorsement.Name != "Project Managers (Non-Construction)" &&
                            changeendorsement.Name != "Building Defects Extension")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, changeendorsement.Name, changeendorsement.Type, product, changeendorsement.Value, changeendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }
            }

            //Referral points per agreement
            //Number of Personnel
            uwrfnumberofpersonnel(underwritingUser, agreement, totalnumberofpersonnel, rates);
            //Non PMINZ Members
            uwrfnonpminzmembers(underwritingUser, agreement, bolnonpmimember);
            //Other or Non PM Activities
            uwrfotherornonpmactivities(underwritingUser, agreement, decOPMA, decNPMA);
            //Operates Outside of NZ
            uwrfoperatesoutsideofnz(underwritingUser, agreement, bolworkoutsidenz);
            //High Fee Income
            uwrfhighfeeincome(underwritingUser, agreement, feeincome, rates);
            //Contracting Services
            uwrfcontractingservices(underwritingUser, agreement);
            //Claims / Insurance History
            uwrfpriorinsurance(underwritingUser, agreement);
            //No Projects Managed
            uwrfnoprojectsmanaged(underwritingUser, agreement);
            //Renewal Premiums Lower than Expiring
            uwrfrenewalpremiumslowerthanexpiring(underwritingUser, agreement, bolrenewalpremiumslowerthanexpiring);
            //Renewal Premiums Higher than Expiring
            uwrfrenewalpremiumshigherthanexpiring(underwritingUser, agreement, bolrenewalpremiumshigherthanexpiring);
            //Capacity of an Engineer to Contract
            uwrfcapacityofanengineertocontract(underwritingUser, agreement);
            //Construction revenue as role as Engineer to the Contract
            uwrfconstructionrevenue(underwritingUser, agreement, constructionEngineerDetails);
            //Component Specification Activities
            uwrfcomponentspecificationactivities(underwritingUser, agreement);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Inception or Date since PI policy first held";
            agreement.TerritoryLimit = "Worldwide excluding USA/Canada";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            string auditLogDetail = "PMINZ PI UW created/modified";
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


        decimal GetPIBasePremiumFor(IDictionary<string, decimal> rates, decimal feeincome, decimal decPRBDSP, decimal decPRCon, decimal decPRFASA, decimal decPRIT, decimal decPRMOP, decimal decPROther,
            int intordnumber, int intpmpnumber, int intcapmnumber, int intpdnumber)
        {
            decimal pibasepremium = 0M;
            decimal basepremiumOrd = 0M;
            decimal basepremiumCAPM = 0M;
            decimal basepremiumPMP = 0M;
            decimal basepremiumPD = 0M;
            decimal minpremiumOrd = 0M;
            decimal minpremiumCAPM = 0M;
            decimal minpremiumPMP = 0M;
            decimal minpremiumPD = 0M;

            if (intordnumber > 0)
            {
                basepremiumOrd = (feeincome * decPRIT / 100 * rates["piitcomponentrateord"] / 100) + (feeincome * decPRCon / 100 * rates["piconstructioncomponentrateord"] / 100) +
                            (feeincome * decPRBDSP / 100 * rates["pibusinessdevpmtcomponentrateord"] / 100) + (feeincome * decPRMOP / 100 * rates["pimanufacturingcomponentrateord"] / 100) +
                            (feeincome * decPRFASA / 100 * rates["pifinancialcomponentrateord"] / 100) + (feeincome * decPROther / 100 * rates["piothercomponentrateord"] / 100);
                minpremiumOrd = (decPRIT / 100 * rates["piitcomponentminpremiumord"]) + (decPRCon / 100 * rates["piconstructioncomponentminpremiumord"]) +
                                (decPRBDSP / 100 * rates["pibusinessdevpmtcomponentminpremiumord"]) + (decPRMOP / 100 * rates["pimanufacturingcomponentminpremiumord"]) +
                                (decPRFASA / 100 * rates["pifinancialcomponentminpremiumord"]) + (decPROther / 100 * rates["piothercomponentminpremiumord"]);
                basepremiumOrd = (basepremiumOrd > minpremiumOrd) ? basepremiumOrd : minpremiumOrd;
            }
            if (intcapmnumber > 0)
            {
                basepremiumCAPM = (feeincome * decPRIT / 100 * rates["piitcomponentratecapm"] / 100) + (feeincome * decPRCon / 100 * rates["piconstructioncomponentratecapm"] / 100) +
                            (feeincome * decPRBDSP / 100 * rates["pibusinessdevpmtcomponentratecapm"] / 100) + (feeincome * decPRMOP / 100 * rates["pimanufacturingcomponentratecapm"] / 100) +
                            (feeincome * decPRFASA / 100 * rates["pifinancialcomponentratecapm"] / 100) + (feeincome * decPROther / 100 * rates["piothercomponentratecapm"] / 100);
                minpremiumCAPM = (decPRIT / 100 * rates["piitcomponentminpremiumcapm"]) + (decPRCon / 100 * rates["piconstructioncomponentminpremiumcapm"]) +
                                (decPRBDSP / 100 * rates["pibusinessdevpmtcomponentminpremiumcapm"]) + (decPRMOP / 100 * rates["pimanufacturingcomponentminpremiumcapm"]) +
                                (decPRFASA / 100 * rates["pifinancialcomponentminpremiumcapm"]) + (decPROther / 100 * rates["piothercomponentminpremiumcapm"]);
                basepremiumCAPM = (basepremiumCAPM > minpremiumCAPM) ? basepremiumCAPM : minpremiumCAPM;
            }
            if (intpmpnumber > 0)
            {
                basepremiumPMP = (feeincome * decPRIT / 100 * rates["piitcomponentratepmp"] / 100) + (feeincome * decPRCon / 100 * rates["piconstructioncomponentratepmp"] / 100) +
                            (feeincome * decPRBDSP / 100 * rates["pibusinessdevpmtcomponentratepmp"] / 100) + (feeincome * decPRMOP / 100 * rates["pimanufacturingcomponentratepmp"] / 100) +
                            (feeincome * decPRFASA / 100 * rates["pifinancialcomponentratepmp"] / 100) + (feeincome * decPROther / 100 * rates["piothercomponentratepmp"] / 100);
                minpremiumPMP = (decPRIT / 100 * rates["piitcomponentminpremiumpmp"]) + (decPRCon / 100 * rates["piconstructioncomponentminpremiumpmp"]) +
                                (decPRBDSP / 100 * rates["pibusinessdevpmtcomponentminpremiumpmp"]) + (decPRMOP / 100 * rates["pimanufacturingcomponentminpremiumpmp"]) +
                                (decPRFASA / 100 * rates["pifinancialcomponentminpremiumpmp"]) + (decPROther / 100 * rates["piothercomponentminpremiumpmp"]);
                basepremiumPMP = (basepremiumPMP > minpremiumPMP) ? basepremiumPMP : minpremiumPMP;
            }
            if (intpdnumber > 0)
            {
                basepremiumPD = (feeincome * decPRIT / 100 * rates["piitcomponentratepd"] / 100) + (feeincome * decPRCon / 100 * rates["piconstructioncomponentratepd"] / 100) +
                            (feeincome * decPRBDSP / 100 * rates["pibusinessdevpmtcomponentratepd"] / 100) + (feeincome * decPRMOP / 100 * rates["pimanufacturingcomponentratepd"] / 100) +
                            (feeincome * decPRFASA / 100 * rates["pifinancialcomponentratepd"] / 100) + (feeincome * decPROther / 100 * rates["piothercomponentratepd"] / 100);
                minpremiumPD = (decPRIT / 100 * rates["piitcomponentminpremiumpd"]) + (decPRCon / 100 * rates["piconstructioncomponentminpremiumpd"]) +
                                (decPRBDSP / 100 * rates["pibusinessdevpmtcomponentminpremiumpd"]) + (decPRMOP / 100 * rates["pimanufacturingcomponentminpremiumpd"]) +
                                (decPRFASA / 100 * rates["pifinancialcomponentminpremiumpd"]) + (decPROther / 100 * rates["piothercomponentminpremiumpd"]);
                basepremiumPD = (basepremiumPD > minpremiumPD) ? basepremiumPD : minpremiumPD;
            }

            pibasepremium = Math.Max(basepremiumOrd, Math.Max(basepremiumCAPM, Math.Max(basepremiumPMP, basepremiumPD)));

            return pibasepremium;
        }


        void uwrfnumberofpersonnel(User underwritingUser, ClientAgreement agreement, int totalnumberofpersonnel, IDictionary<string, decimal> rates)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnumberofpersonnel" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnumberofpersonnel").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnumberofpersonnel" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (totalnumberofpersonnel > rates["maximumnumberofpersonnel"])
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnumberofpersonnel" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnumberofpersonnel" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnumberofpersonnel" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfnonpminzmembers(User underwritingUser, ClientAgreement agreement, bool bolnonpmimember)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonpminzmembers" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonpminzmembers") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfoperatesoutsideofnz").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonpminzmembers").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonpminzmembers").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonpminzmembers").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonpminzmembers").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonpminzmembers" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (!bolnonpmimember)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonpminzmembers" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonpminzmembers" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonpminzmembers" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfotherornonpmactivities(User underwritingUser, ClientAgreement agreement, decimal decOPMA, decimal decNPMA)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherornonpmactivities" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfotherornonpmactivities").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherornonpmactivities" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decOPMA > 0 || decNPMA > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherornonpmactivities" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherornonpmactivities" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfotherornonpmactivities" && cref.DateDeleted == null).Status = "";
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

        void uwrfcontractingservices(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractingservices" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractingservices").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractingservices" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.ContractingServicesOptions").First().Value != null)
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.ContractingServicesOptions").First().Value != "10")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractingservices" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractingservices" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractingservices" && cref.DateDeleted == null).Status = "";
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

        void uwrfnoprojectsmanaged(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnoprojectsmanaged" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnoprojectsmanaged").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnoprojectsmanaged" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasManagedProjectOptions").First().Value == "2")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnoprojectsmanaged" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnoprojectsmanaged" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnoprojectsmanaged" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfrenewalpremiumslowerthanexpiring(User underwritingUser, ClientAgreement agreement, bool bolrenewalpremiumslowerthanexpiring)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumslowerthanexpiring" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumslowerthanexpiring").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumslowerthanexpiring" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolrenewalpremiumslowerthanexpiring)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumslowerthanexpiring" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumslowerthanexpiring" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumslowerthanexpiring" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfrenewalpremiumshigherthanexpiring(User underwritingUser, ClientAgreement agreement, bool bolrenewalpremiumshigherthanexpiring)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumshigherthanexpiring" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfrenewalpremiumshigherthanexpiring").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumshigherthanexpiring" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (bolrenewalpremiumshigherthanexpiring)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumshigherthanexpiring" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumshigherthanexpiring" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfrenewalpremiumshigherthanexpiring" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfcapacityofanengineertocontract(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcapacityofanengineertocontract" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcapacityofanengineertocontract").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcapacityofanengineertocontract" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasEngineerOptions").First().Value == "1")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcapacityofanengineertocontract" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcapacityofanengineertocontract" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcapacityofanengineertocontract" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfconstructionrevenue(User underwritingUser, ClientAgreement agreement, bool constructionEngineerDetails)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfconstructionrevenue" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfconstructionrevenue").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfconstructionrevenue" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (constructionEngineerDetails)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfconstructionrevenue" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfconstructionrevenue" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfconstructionrevenue" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfcomponentspecificationactivities(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcomponentspecificationactivities" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcomponentspecificationactivities").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcomponentspecificationactivities" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "PIViewModel.HasIncludedDesignOptions").First().Value == "1")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcomponentspecificationactivities" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcomponentspecificationactivities" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcomponentspecificationactivities" && cref.DateDeleted == null).Status = "";
                }
            }
        }

    }
}
