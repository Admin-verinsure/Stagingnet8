using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class AbbottPIFAPUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public AbbottPIFAPUWModule()
        {
            Name = "Abbott_PIFAP";
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

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "1millimitfappremiumfor2advisor", "2millimitfappremiumfor2advisor", "3millimitfappremiumfor2advisor", "5millimitfappremiumfor2advisor",
                "1millimitfappremiumfor3advisor", "2millimitfappremiumfor3advisor", "3millimitfappremiumfor3advisor", "5millimitfappremiumfor3advisor",
                "1millimitfappremiumfor4advisor", "2millimitfappremiumfor4advisor", "3millimitfappremiumfor4advisor", "5millimitfappremiumfor4advisor",
                "1millimitfappremiumfor5advisor", "2millimitfappremiumfor5advisor", "3millimitfappremiumfor5advisor", "5millimitfappremiumfor5advisor",
                "1millimitfappremiumfor6advisor", "2millimitfappremiumfor6advisor", "3millimitfappremiumfor6advisor", "5millimitfappremiumfor6advisor",
                "1millimitfappremiumfor7advisor", "2millimitfappremiumfor7advisor", "3millimitfappremiumfor7advisor", "5millimitfappremiumfor7advisor",
                "1millimitfappremiumfor8advisor", "2millimitfappremiumfor8advisor", "3millimitfappremiumfor8advisor", "5millimitfappremiumfor8advisor",
                "1millimitfappremiumfor9advisor", "2millimitfappremiumfor9advisor", "3millimitfappremiumfor9advisor", "5millimitfappremiumfor9advisor",
                "1millimitfappremiumfor10advisor", "2millimitfappremiumfor10advisor", "3millimitfappremiumfor10advisor", "5millimitfappremiumfor10advisor",
                "1millimitfappremiumfor11advisor", "2millimitfappremiumfor11advisor", "3millimitfappremiumfor11advisor", "5millimitfappremiumfor11advisor",
                "1millimitfappremiumfor12advisor", "2millimitfappremiumfor12advisor", "3millimitfappremiumfor12advisor", "5millimitfappremiumfor12advisor"
                );

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

            //Abbott programme is not a full year policy
            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddMonths(-9).AddDays(1)).Days;

            int fapagreementperiodindays = 0;
            fapagreementperiodindays = (agreement.ExpiryDate - Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value)).Days;

            agreement.InceptionDate = Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value);

            int coverperiodindaysforchange = 0;
            if (DateTime.UtcNow > Convert.ToDateTime(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value))
            {
                coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;
            }
            else
            {
                coverperiodindaysforchange = (agreement.ExpiryDate - agreement.InceptionDate).Days;
            }

            string strProfessionalBusiness = "Sales & Promotion of Life, Investment & General Insurance products, Financial planning & Mortgage Brokering Services";

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

            int TermExcess = 1000;

            if (intnumberofadvisors > 1)
            {
                int TermLimit1mil = 1000000;
                decimal TermPremium1mil = 0M;
                decimal TermBrokerage1mil = 0M;
                TermPremium1mil = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit1mil, agreementperiodindays, fapagreementperiodindays);
                TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

                ClientAgreementTerm term1millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit1mil, TermExcess);
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
                TermPremium2mil = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit2mil, agreementperiodindays, fapagreementperiodindays);
                TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;

                ClientAgreementTerm term2millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit2mil, TermExcess);
                term2millimitpremiumoption.TermLimit = TermLimit2mil;
                term2millimitpremiumoption.Premium = TermPremium2mil;
                term2millimitpremiumoption.BasePremium = TermPremium2mil;
                term2millimitpremiumoption.Excess = TermExcess;
                term2millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term2millimitpremiumoption.Brokerage = TermBrokerage2mil;
                term2millimitpremiumoption.DateDeleted = null;
                term2millimitpremiumoption.DeletedBy = null;


                int TermLimit3mil = 3000000;
                decimal TermPremium3mil = 0M;
                decimal TermBrokerage3mil = 0M;
                TermPremium3mil = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit3mil, agreementperiodindays, fapagreementperiodindays);
                TermBrokerage3mil = TermPremium3mil * agreement.Brokerage / 100;

                ClientAgreementTerm term3millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit3mil, TermExcess);
                term3millimitpremiumoption.TermLimit = TermLimit3mil;
                term3millimitpremiumoption.Premium = TermPremium3mil;
                term3millimitpremiumoption.BasePremium = TermPremium3mil;
                term3millimitpremiumoption.Excess = TermExcess;
                term3millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term3millimitpremiumoption.Brokerage = TermBrokerage3mil;
                term3millimitpremiumoption.DateDeleted = null;
                term3millimitpremiumoption.DeletedBy = null;


                int TermLimit5mil = 5000000;
                decimal TermPremium5mil = 0M;
                decimal TermBrokerage5mil = 0M;
                TermPremium5mil = GetPremiumForFAP(rates, intnumberofadvisors, TermLimit5mil, agreementperiodindays, fapagreementperiodindays);
                TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;

                ClientAgreementTerm term5millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", TermLimit5mil, TermExcess);
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


            }
            else
            {
                ClientAgreementTerm term0millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "PIFAP", 0, 0);
                term0millimitpremiumoption.TermLimit = 0;
                term0millimitpremiumoption.Premium = 0;
                term0millimitpremiumoption.BasePremium = 0;
                term0millimitpremiumoption.Excess = 0;
                term0millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term0millimitpremiumoption.Brokerage = 0;
                term0millimitpremiumoption.DateDeleted = null;
                term0millimitpremiumoption.DeletedBy = null;

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
                            term0millimitpremiumoption.PremiumDiffer = (0 - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            term0millimitpremiumoption.PremiumPre = PreviousBoundPremium;
                            if (term0millimitpremiumoption.TermLimit == term.TermLimit && term0millimitpremiumoption.Excess == term.Excess)
                            {
                                term0millimitpremiumoption.Bound = true;
                            }
                            if (term0millimitpremiumoption.PremiumDiffer < 0)
                            {
                                term0millimitpremiumoption.PremiumDiffer = 0;
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

            string retrodate = "Unlimited excluding known claims and circumstances";
            agreement.TerritoryLimit = "Australia and New Zealand";
            agreement.Jurisdiction = "Australia and New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Abbott PIFAP UW created/modified";
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


        decimal GetPremiumForFAP(IDictionary<string, decimal> rates, int intnumberofadvisors, int limitoption, int agreementperiodindays, int fapagreementperiodindays)
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
                                premiumoption = rates["1millimitfappremiumfor2advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor2advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor2advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor2advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor3advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor3advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor3advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor3advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor4advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor4advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor4advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor4advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor5advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor5advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor5advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor5advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor6advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor6advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor6advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor6advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor7advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor7advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor7advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor7advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor8advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor8advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor8advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor8advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor9advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor9advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor9advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor9advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor10advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor10advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor10advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor10advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor11advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor11advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor11advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor11advisor"];
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
                                premiumoption = rates["1millimitfappremiumfor12advisor"];
                                break;
                            }
                        case 2000000:
                            {
                                premiumoption = rates["2millimitfappremiumfor12advisor"];
                                break;
                            }
                        case 3000000:
                            {
                                premiumoption = rates["3millimitfappremiumfor12advisor"];
                                break;
                            }
                        case 5000000:
                            {
                                premiumoption = rates["5millimitfappremiumfor12advisor"];
                                break;
                            }
                        default:
                            {
                                throw new Exception(string.Format("Can not calculate premium for PIFAP"));
                            }
                    }
                }
            }

            premiumoption = premiumoption * fapagreementperiodindays / agreementperiodindays;

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
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfadvisornumberover12").OrderNumber));
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
            }
        }


    }
}

