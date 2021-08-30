using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZPIPIUWModule2021 : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZPIPIUWModule2021()
        {
            Name = "NZPI_PI_2021";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "1millimitpremium1employee",
                "2millimitpremium1employee", "5millimitpremium1employee", "1millimitpremium2employee", "2millimitpremium2employee", "5millimitpremium2employee", "1millimitpremiumover2employee",
                "2millimitpremiumover2employee", "5millimitpremiumover2employee", "maxfeeincome", "pibrokerfee");

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

            decimal feeincometotallastandnextyr = 0M;
            decimal feeincomeaverage = 0M;
            decimal decOther = 0M;
            decimal decLALP = 0M;
            bool bolworkoutsidenz = false;

            string strProfessionalBusiness = "Planning / urban design, resource management, local government advice, transport planning, Environmental policy advice, heritage planning, planning commissioner, market research, land management investigations, disputes resolution, master planning, urban design workshops, training, university lecturing.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.CurrentYearTotal > 0)
                {
                    feeincometotallastandnextyr += agreement.ClientInformationSheet.RevenueData.CurrentYearTotal;
                }
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincometotallastandnextyr += agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                }

                feeincomeaverage = feeincometotallastandnextyr / 2;

                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (!bolworkoutsidenz && uISTerritory.Location != "New Zealand" && uISTerritory.Percentage > 0) //Work outside New Zealand Check
                    {
                        bolworkoutsidenz = true;
                    }
                }

                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUS0044") //Other
                    {
                        decOther = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUSNZPI01") //Landscape Architecture & Landscape Planning
                    {
                        if (uISActivity.Percentage > 0)
                            decLALP = uISActivity.Percentage;
                    }
                }
            }

            int intnumberofadvisors = 0;
            int intnumberofnoneplanner = 0;
            int intnumberofcontractor = 0;
            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    var principleadvisorunit1 = (PlannerUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => (u.Name == "Planner") && u.DateDeleted == null);

                    if (principleadvisorunit1 != null)
                    {
                        if (uisorg.DateDeleted == null && !uisorg.Removed)
                        {
                            intnumberofadvisors += 1;

                            if (!principleadvisorunit1.IsNZPIAMember)
                            {
                                intnumberofnoneplanner += 1;
                            }
                        }
                    }

                    var principleadvisorunit2 = (PlannerUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => (u.Name == "Contractor") && u.DateDeleted == null);

                    if (principleadvisorunit2 != null)
                    {
                        if (uisorg.DateDeleted == null && !uisorg.Removed)
                        {
                            intnumberofadvisors += 1;
                            intnumberofcontractor += 1;
                        }
                    }
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
                            renewendorsement.Name != "Project Managers (Non-Construction)" && 
                            renewendorsement.Name != "Building Defects Endorsement" && 
                            renewendorsement.Name != "Pollution Exclusion with Writeback")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                            bolcustomendorsementrenew = true;
                        }
                    }
                }

            }

            ClientAgreementEndorsement cAELPLAIncl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Inclusion");
            ClientAgreementEndorsement cAELPLAExcl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Exclusion");

            if (cAELPLAIncl != null)
            {
                cAELPLAIncl.DateDeleted = DateTime.UtcNow;
                cAELPLAIncl.DeletedBy = underwritingUser;
            }
            if (cAELPLAExcl != null)
            {
                cAELPLAExcl.DateDeleted = DateTime.UtcNow;
                cAELPLAExcl.DeletedBy = underwritingUser;
            }

            int TermExcess = 0;
            TermExcess = 2000;

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0M;
            decimal TermBrokerage1mil = 0M;

            TermPremium1mil = GetPremiumFor(rates, TermLimit1mil, intnumberofadvisors);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium1mil = TermPremium1mil / coverperiodindays * agreementperiodindays;
            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

            ClientAgreementTerm term1millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit1mil, TermExcess);
            term1millimitpremiumoption.TermLimit = TermLimit1mil;
            term1millimitpremiumoption.Premium = TermPremium1mil;
            term1millimitpremiumoption.BasePremium = TermPremium1mil;
            term1millimitpremiumoption.Excess = TermExcess;
            term1millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term1millimitpremiumoption.Brokerage = TermBrokerage1mil;
            term1millimitpremiumoption.DateDeleted = null;
            term1millimitpremiumoption.DeletedBy = null;

            int TermLimit2mil = 2000000;
            decimal TermPremium2mil = 0M;
            decimal TermBrokerage2mil = 0M;

            TermPremium2mil = GetPremiumFor(rates, TermLimit2mil, intnumberofadvisors);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium2mil = TermPremium2mil / coverperiodindays * agreementperiodindays;
            TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;

            ClientAgreementTerm term2millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess);
            term2millimitpremiumoption.TermLimit = TermLimit2mil;
            term2millimitpremiumoption.Premium = TermPremium2mil;
            term2millimitpremiumoption.BasePremium = TermPremium2mil;
            term2millimitpremiumoption.Excess = TermExcess;
            term2millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term2millimitpremiumoption.Brokerage = TermBrokerage2mil;
            term2millimitpremiumoption.DateDeleted = null;
            term2millimitpremiumoption.DeletedBy = null;

            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = 0M;
            decimal TermBrokerage5mil = 0M;

            TermPremium5mil = GetPremiumFor(rates, TermLimit5mil, intnumberofadvisors);
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium5mil = TermPremium5mil / coverperiodindays * agreementperiodindays;
            TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;

            ClientAgreementTerm term5millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess);
            term5millimitpremiumoption.TermLimit = TermLimit5mil;
            term5millimitpremiumoption.Premium = TermPremium5mil;
            term5millimitpremiumoption.BasePremium = TermPremium5mil;
            term5millimitpremiumoption.Excess = TermExcess;
            term5millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremiumoption.Brokerage = TermBrokerage5mil;
            term5millimitpremiumoption.DateDeleted = null;
            term5millimitpremiumoption.DeletedBy = null;

            //add Landscape Architecture & Landscape Planning extension

            foreach (ClientAgreementTermExtension pitermextension in agreement.ClientAgreementTermExtensions.Where(ctex => ctex.DateDeleted == null))
            {
                pitermextension.Delete(underwritingUser);
            }
            if (decLALP > 0)
            {
                ClientAgreementTermExtension termLALPextension = GetAgreementExtensionTerm(underwritingUser, agreement, 0, 0M, 400, "Landscape Architecture & Landscape Planning");
                termLALPextension.ExtentionName = "Landscape Architecture & Landscape Planning";
                termLALPextension.DateDeleted = null;
                termLALPextension.DeletedBy = null;
            }


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
            //Other Business Activities
            uwrfotheractivities(underwritingUser, agreement, decOther);
            //Turnover is more than $1,000,000
            uwrfmaxfeeincome(underwritingUser, agreement, feeincomeaverage, rates);
            //Any firm as a whole are non NZPI members
            uwrfnonenzpimember(underwritingUser, agreement, intnumberofnoneplanner);
            //Contractor cover requested
            uwrfcontractor(underwritingUser, agreement, intnumberofcontractor);
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

            string retrodate = "Unlimited excluding known claims or circumstances";
            agreement.TerritoryLimit = "New Zealand";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZPI PI UW created/modified";
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

        ClientAgreementTermExtension GetAgreementExtensionTerm(User CurrentUser, ClientAgreement agreement, int limitoption, decimal excessoption, decimal premiumoption, string extensionName)
        {
            ClientAgreementTermExtension extensionTerm = agreement.ClientAgreementTermExtensions.FirstOrDefault(tex => tex.DateDeleted != null && tex.ExtentionName == extensionName);

            if (extensionTerm == null)
            {
                extensionTerm = new ClientAgreementTermExtension(CurrentUser, limitoption, excessoption, premiumoption, agreement);
                agreement.ClientAgreementTermExtensions.Add(extensionTerm);
            }

            return extensionTerm;
        }

        decimal GetPremiumFor(IDictionary<string, decimal> rates, int limitoption, int intnumberofadvisors)
        {
            decimal premiumoption = 0M;
            decimal premiumoptionperemployee = 0M;

            if (intnumberofadvisors == 1)
            {
                switch (limitoption)
                {
                    case 1000000:
                        {
                            premiumoptionperemployee = rates["1millimitpremium1employee"];
                            break;
                        }
                    case 2000000:
                        {
                            premiumoptionperemployee = rates["2millimitpremium1employee"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoptionperemployee = rates["5millimitpremium1employee"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }
            else if (intnumberofadvisors == 2)
            {
                switch (limitoption)
                {
                    case 1000000:
                        {
                            premiumoptionperemployee = rates["1millimitpremium2employee"];
                            break;
                        }
                    case 2000000:
                        {
                            premiumoptionperemployee = rates["2millimitpremium2employee"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoptionperemployee = rates["5millimitpremium2employee"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }
            else if (intnumberofadvisors >= 3)
            {
                switch (limitoption)
                {
                    case 1000000:
                        {
                            premiumoptionperemployee = rates["1millimitpremiumover2employee"];
                            break;
                        }
                    case 2000000:
                        {
                            premiumoptionperemployee = rates["2millimitpremiumover2employee"];
                            break;
                        }
                    case 5000000:
                        {
                            premiumoptionperemployee = rates["5millimitpremiumover2employee"];
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Can not calculate premium for PI"));
                        }
                }
            }

            premiumoption = Math.Ceiling(premiumoptionperemployee * intnumberofadvisors);

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

        void uwrfmaxfeeincome(User underwritingUser, ClientAgreement agreement, decimal feeincomeaverage, IDictionary<string, decimal> rates)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxfeeincome" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxfeeincome").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxfeeincome" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (feeincomeaverage > rates["maxfeeincome"])
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxfeeincome" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxfeeincome" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxfeeincome" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfnonenzpimember(User underwritingUser, ClientAgreement agreement, int intnumberofnoneplanner)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonenzpimember" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnonenzpimember").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonenzpimember" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (intnumberofnoneplanner > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonenzpimember" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonenzpimember" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnonenzpimember" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfcontractor(User underwritingUser, ClientAgreement agreement, int intnumberofcontractor)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractor" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfcontractor").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractor" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (intnumberofcontractor > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractor" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractor" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfcontractor" && cref.DateDeleted == null).Status = "";
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

