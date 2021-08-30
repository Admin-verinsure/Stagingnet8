using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZPIPLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZPIPLUWModule()
        {
            Name = "NZPI_PL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm plterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "PL" && ct.DateDeleted == null))
                {
                    plterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "pl1millimitpremium", "pl1millimitexcess", "pl2millimitpremium", "pl2millimitexcess", "pl5millimitpremium", "pl5millimitexcess");

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

            string strretrodate = "";
            //if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            //{
            //    foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
            //    {
            //        if (preRenewOrRefData.DataType == "preterm")
            //        {
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.GLRetro))
            //            {
            //                strretrodate = preRenewOrRefData.GLRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "GL")
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
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PL"));

                if (renewFromAgreement != null)
                {
                    strretrodate = renewFromAgreement.RetroactiveDate;

                    foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
                    {

                        if (renewendorsement.DateDeleted == null &&
                            renewendorsement.Name != "Business Advice or Service Exclusion")
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }

            }

            string strProfessionalBusiness = "Planning / urban design, resource management, local government advice, transport planning, Environmental policy advice, heritage planning, planning commissioner, market research, land management investigations, disputes resolution, master planning, urban design workshops, training, university lecturing.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            int TermLimit1mil = 1000000;
            decimal TermPremium1mil = 0m;
            decimal TermBrokerage1mil = 0m;
            decimal TermExcess1mil = 0;
            TermPremium1mil = rates["pl1millimitpremium"];
            TermExcess1mil = rates["pl1millimitexcess"];
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium1mil = TermPremium1mil / coverperiodindays * agreementperiodindays;
            TermBrokerage1mil = TermPremium1mil * agreement.Brokerage / 100;

            ClientAgreementTerm termpl1millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit1mil, TermExcess1mil);
            termpl1millimitoption.TermLimit = TermLimit1mil;
            termpl1millimitoption.Premium = TermPremium1mil;
            termpl1millimitoption.BasePremium = TermPremium1mil;
            termpl1millimitoption.Excess = TermExcess1mil;
            termpl1millimitoption.BrokerageRate = agreement.Brokerage;
            termpl1millimitoption.Brokerage = TermBrokerage1mil;
            termpl1millimitoption.DateDeleted = null;
            termpl1millimitoption.DeletedBy = null;

            int TermLimit2mil = 2000000;
            decimal TermPremium2mil = 0m;
            decimal TermBrokerage2mil = 0m;
            decimal TermExcess2mil = 0;
            TermPremium2mil = rates["pl2millimitpremium"];
            TermExcess2mil = rates["pl2millimitexcess"];
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium2mil = TermPremium2mil / coverperiodindays * agreementperiodindays;
            TermBrokerage2mil = TermPremium2mil * agreement.Brokerage / 100;

            ClientAgreementTerm termpl2millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit2mil, TermExcess2mil);
            termpl2millimitoption.TermLimit = TermLimit2mil;
            termpl2millimitoption.Premium = TermPremium2mil;
            termpl2millimitoption.BasePremium = TermPremium2mil;
            termpl2millimitoption.Excess = TermExcess2mil;
            termpl2millimitoption.BrokerageRate = agreement.Brokerage;
            termpl2millimitoption.Brokerage = TermBrokerage2mil;
            termpl2millimitoption.DateDeleted = null;
            termpl2millimitoption.DeletedBy = null;

            int TermLimit5mil = 5000000;
            decimal TermPremium5mil = 0m;
            decimal TermBrokerage5mil = 0m;
            decimal TermExcess5mil = 0;
            TermPremium5mil = rates["pl5millimitpremium"];
            TermExcess5mil = rates["pl5millimitexcess"];
            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium5mil = TermPremium5mil / coverperiodindays * agreementperiodindays;
            TermBrokerage5mil = TermPremium5mil * agreement.Brokerage / 100;

            ClientAgreementTerm termpl5millimitoption = GetAgreementTerm(underwritingUser, agreement, "PL", TermLimit5mil, TermExcess5mil);
            termpl5millimitoption.TermLimit = TermLimit5mil;
            termpl5millimitoption.Premium = TermPremium5mil;
            termpl5millimitoption.BasePremium = TermPremium5mil;
            termpl5millimitoption.Excess = TermExcess5mil;
            termpl5millimitoption.BrokerageRate = agreement.Brokerage;
            termpl5millimitoption.Brokerage = TermBrokerage5mil;
            termpl5millimitoption.DateDeleted = null;
            termpl5millimitoption.DeletedBy = null;


            //Change policy premium claculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "PL"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termpl1millimitoption.PremiumDiffer = (TermPremium1mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpl1millimitoption.PremiumPre = PreviousBoundPremium;
                        if (termpl1millimitoption.TermLimit == term.TermLimit && termpl1millimitoption.Excess == term.Excess)
                        {
                            termpl1millimitoption.Bound = true;
                        }
                        if (termpl1millimitoption.PremiumDiffer < 0)
                        {
                            termpl1millimitoption.PremiumDiffer = 0;
                        }
                        termpl2millimitoption.PremiumDiffer = (TermPremium2mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpl2millimitoption.PremiumPre = PreviousBoundPremium;
                        if (termpl2millimitoption.TermLimit == term.TermLimit && termpl2millimitoption.Excess == term.Excess)
                        {
                            termpl2millimitoption.Bound = true;
                        }
                        if (termpl2millimitoption.PremiumDiffer < 0)
                        {
                            termpl2millimitoption.PremiumDiffer = 0;
                        }
                        termpl5millimitoption.PremiumDiffer = (TermPremium5mil - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termpl5millimitoption.PremiumPre = PreviousBoundPremium;
                        if (termpl5millimitoption.TermLimit == term.TermLimit && termpl5millimitoption.Excess == term.Excess)
                        {
                            termpl5millimitoption.Bound = true;
                        }
                        if (termpl5millimitoption.PremiumDiffer < 0)
                        {
                            termpl5millimitoption.PremiumDiffer = 0;
                        }
                    }

                }
            }


            //Referral points per agreement


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Not Applicable";
            agreement.TerritoryLimit = "Worldwide excluding USA/Canada";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZPI PL UW created/modified";
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




    }
}
