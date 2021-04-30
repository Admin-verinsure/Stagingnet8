using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZFSGPIUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZFSGPIUWModule()
        {
            Name = "NZFSG_PI";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "piFP1To10PerExtraPremium", "piPEF0To70PerExtraPremium", "piPEF71To100PerExtraPremium", "piRFG10To20PerExtraPremium",
                "piBFG1To10PerExtraPremium", "pi2millimitincomeunder500kdiscountpremium", "pi2millimitincome500kto600kdiscountpremium", "pi2millimitincome600kto800kdiscountpremium",
                "pi2millimitincome800kto1mildiscountpremium", "pi2millimitincomeunder500kpremium", "pi2millimitincome500kto600kpremium", "pi2millimitincome600kto800kpremium", 
                "pi2millimitincome800kto1milpremium", "pi3millimitextrapremium", "pi5millimitextrapremium", "pi3millimitincomeunder500kdiscountpremium", "pi5millimitincomeunder500kdiscountpremium",
                "pi3millimitincomeunder500kpremium", "pi5millimitincomeunder500kpremium", "maximumfeeincome", "2millimitclextrapremium", "3millimitclextrapremium", "5millimitclextrapremium");

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


            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            agreement.QuoteDate = DateTime.UtcNow;

            decimal feeincome = 0;

            decimal decMBR = 0M;
            decimal decMBC = 0M;
            decimal decIns = 0M;
            decimal decFP = 0M;
            decimal decKS = 0M;
            decimal decBA = 0M;
            decimal decAF = 0M;
            decimal decRFG = 0M;
            decimal decBFG = 0M;
            decimal decOther = 0M;

            decimal decBFG1To10PerExtraPre = 0M;
            decimal decFP1To10PerExtraPre = 0M;
            decimal decPEF0To70PerExtraPre = 0M;
            decimal decPEF71To100PerExtraPre = 0M;
            decimal decRFG10To20PerExtraPre = 0M;

            decimal decPIPremiumTopUp = 0M;

            string strProfessionalBusiness = "Mortgage broking and life, risk, health and medical insurance broking services. Fire and General referrals, including AON domestic placement services only. Advice in respect of ACC reporting status. Advice in relation to Kiwisaver.  Asset Finance.";

            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                if (agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal > 0)
                {
                    feeincome = agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal;
                } else if (agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal > 0) 
                {
                    feeincome = agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal;
                }


                foreach (var uISActivity in agreement.ClientInformationSheet.RevenueData.Activities)
                {
                    if (uISActivity.AnzsciCode == "CUS0020") //Mortgage Broking (Residential)
                    {
                        decMBR = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0021") //Mortgage Broking (Commercial)
                    {
                        decMBC = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0022") //Insurance (Life, medical, disability only)
                    {
                        decIns = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0023") //Financial Planning
                    {
                        decFP = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                        {
                            strProfessionalBusiness += "  Advice in relation to Financial Planning.";
                        }

                        if (uISActivity.Percentage > 0 && uISActivity.Percentage <= 10)
                        {
                            decFP1To10PerExtraPre = rates["piFP1To10PerExtraPremium"];
                        }
                    }
                    else if (uISActivity.AnzsciCode == "CUS0024") //Kiwisaver
                    {
                        decKS = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0025") //Budget Advice
                    {
                        decBA = uISActivity.Percentage;
                    }
                    else if (uISActivity.AnzsciCode == "CUS0026") //Asset Finance 
                    {
                        decAF = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0 && uISActivity.Percentage <= 70)
                            decPEF0To70PerExtraPre = rates["piPEF0To70PerExtraPremium"];
                        if (uISActivity.Percentage > 70 && uISActivity.Percentage <= 100)
                            decPEF71To100PerExtraPre = rates["piPEF71To100PerExtraPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "CUS0027") //Referred Fire and General (i.e. Tower, Aon) 
                    {
                        decRFG = uISActivity.Percentage;
                        if (uISActivity.Percentage >= 10 && uISActivity.Percentage <= 20)
                            decRFG10To20PerExtraPre = rates["piRFG10To20PerExtraPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "CUS0028") //Broking Fire and General (i.e. NZI)
                    {
                        decBFG = uISActivity.Percentage;
                        if (uISActivity.Percentage > 0)
                        {
                            strProfessionalBusiness += "  Advice in relation to Fire and General Broking.";
                        }
                        if (uISActivity.Percentage > 0 && uISActivity.Percentage <= 10)
                            decBFG1To10PerExtraPre = rates["piBFG1To10PerExtraPremium"];
                    }
                    else if (uISActivity.AnzsciCode == "CUS0029") //Other 
                    {
                        decOther = uISActivity.Percentage;
                    }


                }

                decPIPremiumTopUp = decBFG1To10PerExtraPre + decFP1To10PerExtraPre + decPEF0To70PerExtraPre + decPEF71To100PerExtraPre + decRFG10To20PerExtraPre;
            }

            int intnumberofadvisors = 0;
            bool advisorhasnocrmid = false;
            if (agreement.ClientInformationSheet.Organisation.Count > 0)
            {
                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                {
                    var unit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(o => o.Name == "Advisor");
                    if (unit != null)
                    {
                        if (uisorg.DateDeleted == null && !uisorg.Removed)
                        {
                            intnumberofadvisors += 1;
                            if (!advisorhasnocrmid && string.IsNullOrEmpty(unit.MyCRMId))
                            {
                                advisorhasnocrmid = true;
                            }
                        }
                        if (agreement.ClientInformationSheet.IsChange && uisorg.OrgBeenMoved && uisorg.DateDeleted == null)
                        {
                            intnumberofadvisors -= 1;
                        }
                    }
                }                                    
            }

            bool subuisreferred = false;
            //if (agreement.ClientInformationSheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null).Count() > 0)
            //{
            //    foreach (var subuis in agreement.ClientInformationSheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null))
            //    {
            //        if ((subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasDamageLossOptions").First().Value == "1" ||
            //            subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasWithdrawnOptions").First().Value == "1" ||
            //            subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasRefusedOptions").First().Value == "1" ||
            //            subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasStatutoryOffenceOptions").First().Value == "1" ||
            //            subuis.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasLiquidationOptions").First().Value == "1" ||
            //            subuis.ClaimNotifications.Where(subacscn => subacscn.DateDeleted == null && (subacscn.ClaimStatus == "Settled" || subacscn.ClaimStatus == "Precautionary notification only" || subacscn.ClaimStatus == "Part Settled")).Count() > 0) &&
            //            !subuisreferred)
            //        {
            //            subuisreferred = true;
            //        }
            //    }
            //}


            ClientAgreementEndorsement afendorsement = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Professional Services & Business Description Extension");
            if (afendorsement != null)
            {
                afendorsement.DateDeleted = DateTime.UtcNow;
                afendorsement.DeletedBy = underwritingUser;
            }
            if (decAF > 0)
            {
                if (afendorsement != null)
                {
                    afendorsement.DateDeleted = null;
                    afendorsement.DeletedBy = null;
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

            int TermLimit2mil = 2000000;
            int TermLimit3mil = 3000000;
            int TermLimit5mil = 5000000;

            decimal TermPremium2mil = 0M;
            decimal TermPremium3mil = 0M;
            decimal TermPremium5mil = 0M;

            decimal TermBrokerage2mil = 0M;
            decimal TermBrokerage3mil = 0M;
            decimal TermBrokerage5mil = 0M;

            int TermExcess = 0;
            TermExcess = 1000;

            TermPremium2mil = GetPremiumFor(rates, feeincome, TermLimit2mil, intnumberofadvisors, decPIPremiumTopUp);
            ClientAgreementTerm term2millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit2mil, TermExcess);
            term2millimitpremiumoption.TermLimit = TermLimit2mil;
            term2millimitpremiumoption.Premium = TermPremium2mil;
            term2millimitpremiumoption.BasePremium = TermPremium2mil;
            term2millimitpremiumoption.Excess = TermExcess;
            term2millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term2millimitpremiumoption.Brokerage = TermBrokerage2mil;
            term2millimitpremiumoption.DateDeleted = null;
            term2millimitpremiumoption.DeletedBy = null;

            TermPremium3mil = GetPremiumFor(rates, feeincome, TermLimit3mil, intnumberofadvisors, decPIPremiumTopUp);
            ClientAgreementTerm term3millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit3mil, TermExcess);
            term3millimitpremiumoption.TermLimit = TermLimit3mil;
            term3millimitpremiumoption.Premium = TermPremium3mil;
            term3millimitpremiumoption.BasePremium = TermPremium3mil;
            term3millimitpremiumoption.Excess = TermExcess;
            term3millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term3millimitpremiumoption.Brokerage = TermBrokerage3mil;
            term3millimitpremiumoption.DateDeleted = null;
            term3millimitpremiumoption.DeletedBy = null;

            TermPremium5mil = GetPremiumFor(rates, feeincome, TermLimit5mil, intnumberofadvisors, decPIPremiumTopUp);
            ClientAgreementTerm term5millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PI", TermLimit5mil, TermExcess);
            term5millimitpremiumoption.TermLimit = TermLimit5mil;
            term5millimitpremiumoption.Premium = TermPremium5mil;
            term5millimitpremiumoption.BasePremium = TermPremium5mil;
            term5millimitpremiumoption.Excess = TermExcess;
            term5millimitpremiumoption.BrokerageRate = agreement.Brokerage;
            term5millimitpremiumoption.Brokerage = TermBrokerage5mil;
            term5millimitpremiumoption.DateDeleted = null;
            term5millimitpremiumoption.DeletedBy = null;

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
                        term2millimitpremiumoption.PremiumDiffer = (TermPremium2mil - PreviousBoundPremium) * coverperiodindays / agreementperiodindays;
                        term3millimitpremiumoption.PremiumDiffer = (TermPremium3mil - PreviousBoundPremium) * coverperiodindays / agreementperiodindays;
                        term5millimitpremiumoption.PremiumDiffer = (TermPremium5mil - PreviousBoundPremium) * coverperiodindays / agreementperiodindays;
                        term2millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term2millimitpremiumoption.TermLimit == term.TermLimit && term2millimitpremiumoption.Excess == term.Excess)
                        {
                            term2millimitpremiumoption.Bound = true;
                        }
                        term3millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term3millimitpremiumoption.TermLimit == term.TermLimit && term3millimitpremiumoption.Excess == term.Excess)
                        {
                            term3millimitpremiumoption.Bound = true;
                        }
                        term5millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                        if (term5millimitpremiumoption.TermLimit == term.TermLimit && term5millimitpremiumoption.Excess == term.Excess)
                        {
                            term5millimitpremiumoption.Bound = true;
                        }
                        if (term2millimitpremiumoption.PremiumDiffer < 0)
                        {
                            term2millimitpremiumoption.PremiumDiffer = 0;
                        }
                        if (term3millimitpremiumoption.PremiumDiffer < 0)
                        {
                            term3millimitpremiumoption.PremiumDiffer = 0;
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
            //High Fee Income
            uwrfhighfeeincome(underwritingUser, agreement, feeincome, rates);
            //Financial Planning Activity
            uwrffinancialplanningactivity(underwritingUser, agreement, decFP);
            //Referred Fire and General (i.e. Tower, Aon) Activity
            uwrfreferredfireandgeneralactivity(underwritingUser, agreement, decRFG);
            //Broking Fire and General (i.e. NZI) Activity
            uwrfbrokingfireandgeneralactivity(underwritingUser, agreement, decBFG);
            //Other Activity
            uwrfotheractivity(underwritingUser, agreement, decOther);
            //Not a renewal of an existing policy
            uwrfnotrenewal(underwritingUser, agreement);
            //Advisor Claims / Insurance History
            uwrfadvisorpriorinsurance(underwritingUser, agreement, subuisreferred);
            //Broker Referral
            uwrfbrokerreferral(underwritingUser, agreement);
            //Advisor has no CRMID
            uwrfadvisorhasnocrmid(underwritingUser, agreement, advisorhasnocrmid);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            agreement.ProfessionalBusiness = strProfessionalBusiness;
            string retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
            agreement.TerritoryLimit = "Worldwide";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZFSG PI UW created/modified";
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


        decimal GetPremiumFor(IDictionary<string, decimal> rates, decimal feeincome, int limitoption, int intnumberofadvisors, decimal decPIPremiumTopUp)
        {
            decimal indadvisorpremiumoption = 0M;
            decimal decbrokerfee = 50M;
            decimal clextrapremium = 0M;
            decimal premiumoption = 0M;

            switch (limitoption)
            {
                case 2000000:
                    {
                        if (intnumberofadvisors >= 4)
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincomeunder500kdiscountpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kdiscountpremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kdiscountpremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1mildiscountpremium"];
                            }
                        }
                        else
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincomeunder500kpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kpremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kpremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1milpremium"];
                            }
                        }
                        clextrapremium = rates["2millimitclextrapremium"];
                        break;
                    }
                case 3000000:
                    {
                        if (intnumberofadvisors >= 4)
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi3millimitincomeunder500kdiscountpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kdiscountpremium"] + rates["pi3millimitextrapremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kdiscountpremium"] + rates["pi3millimitextrapremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1mildiscountpremium"] + rates["pi3millimitextrapremium"];
                            }
                        }
                        else
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi3millimitincomeunder500kpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kpremium"] + rates["pi3millimitextrapremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kpremium"] + rates["pi3millimitextrapremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1milpremium"] + rates["pi3millimitextrapremium"];
                            }
                        }
                        clextrapremium = rates["3millimitclextrapremium"];
                        break;
                    }
                case 5000000:
                    {
                        if (intnumberofadvisors >= 4)
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi5millimitincomeunder500kdiscountpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kdiscountpremium"] + rates["pi5millimitextrapremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kdiscountpremium"] + rates["pi5millimitextrapremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1mildiscountpremium"] + rates["pi5millimitextrapremium"];
                            }
                        }
                        else
                        {
                            if (feeincome >= 0 && feeincome <= 500000)
                            {
                                indadvisorpremiumoption = rates["pi5millimitincomeunder500kpremium"];
                            }
                            else if (feeincome > 500000 && feeincome <= 600000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome500kto600kpremium"] + rates["pi5millimitextrapremium"];
                            }
                            else if (feeincome > 600000 && feeincome <= 800000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome600kto800kpremium"] + rates["pi5millimitextrapremium"];
                            }
                            else if (feeincome > 800000 && feeincome <= 1000000)
                            {
                                indadvisorpremiumoption = rates["pi2millimitincome800kto1milpremium"] + rates["pi5millimitextrapremium"];
                            }
                        }
                        clextrapremium = rates["5millimitclextrapremium"];
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format("Can not calculate premium for PI"));
                    }
            }

            premiumoption = (indadvisorpremiumoption + clextrapremium + decPIPremiumTopUp + decbrokerfee) * intnumberofadvisors;

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
                    if (feeincome > rates["maximumfeeincome"] || feeincome <= 0)
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
     
        void uwrffinancialplanningactivity(User underwritingUser, ClientAgreement agreement, decimal decFP)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialplanningactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrffinancialplanningactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialplanningactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decFP > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialplanningactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialplanningactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrffinancialplanningactivity" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfreferredfireandgeneralactivity(User underwritingUser, ClientAgreement agreement, decimal decRFG)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfreferredfireandgeneralactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfreferredfireandgeneralactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfreferredfireandgeneralactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decRFG > 10)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfreferredfireandgeneralactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfreferredfireandgeneralactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfreferredfireandgeneralactivity" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfbrokingfireandgeneralactivity(User underwritingUser, ClientAgreement agreement, decimal decBFG)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokingfireandgeneralactivity" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokingfireandgeneralactivity").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokingfireandgeneralactivity" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (decBFG > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokingfireandgeneralactivity" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokingfireandgeneralactivity" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokingfireandgeneralactivity" && cref.DateDeleted == null).Status = "";
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

        void uwrfbrokerreferral(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokerreferral" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfbrokerreferral").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokerreferral" && cref.DateDeleted == null).Status != "Pending")
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokerreferral" && cref.DateDeleted == null).Status = "Pending";
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokerreferral" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfbrokerreferral" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfadvisorhasnocrmid(User underwritingUser, ClientAgreement agreement, bool advisorhasnocrmid)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorhasnocrmid" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisorhasnocrmid").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorhasnocrmid" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (advisorhasnocrmid)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorhasnocrmid" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorhasnocrmid" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfadvisorhasnocrmid" && cref.DateDeleted == null).Status = "";
                }
            }
        }

    }
}
