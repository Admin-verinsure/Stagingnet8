using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class NZBarSLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public NZBarSLUWModule()
        {
            Name = "NZBar_SL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "SL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm slterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "SL" && ct.DateDeleted == null))
                {
                    slterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "sltermlimit", "sltermexcess", "sltermpremium");

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

            //For 1st year set up
            string strretrodate = "";
            if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            {
                foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
                {
                    if (preRenewOrRefData.DataType == "preterm")
                    {
                        if (!string.IsNullOrEmpty(preRenewOrRefData.SLRetro))
                        {
                            strretrodate = preRenewOrRefData.SLRetro;
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "SL")
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
            //    var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "SL"));

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

            int TermLimit = 0;
            decimal TermPremium = 0M;
            decimal TermBasePremium = 0M;
            decimal TermBrokerage = 0M;
            decimal TermExcess = 0M;
            TermLimit = Convert.ToInt32(rates["sltermlimit"]);
            TermExcess = rates["sltermexcess"];
            TermPremium = rates["sltermpremium"];
            TermBasePremium = TermPremium;
            TermPremium = TermPremium * agreementperiodindays / coverperiodindays;
            TermBrokerage = TermPremium * agreement.Brokerage / 100;

            ClientAgreementTerm termsltermoption = GetAgreementTerm(underwritingUser, agreement, "SL", TermLimit, TermExcess);
            termsltermoption.TermLimit = TermLimit;
            termsltermoption.Premium = TermPremium;
            termsltermoption.BasePremium = TermBasePremium;
            termsltermoption.Excess = TermExcess;
            termsltermoption.BrokerageRate = agreement.Brokerage;
            termsltermoption.Brokerage = TermBrokerage;
            termsltermoption.DateDeleted = null;
            termsltermoption.DeletedBy = null;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.Where(a => a.DateDeleted == null).
                    FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "SL"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termsltermoption.PremiumDiffer = (TermPremium - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsltermoption.PremiumPre = PreviousBoundPremium;
                        if (termsltermoption.TermLimit == term.TermLimit && termsltermoption.Excess == term.Excess)
                        {
                            termsltermoption.Bound = true;
                        }
                        if (termsltermoption.PremiumDiffer < 0)
                        {
                            termsltermoption.PremiumDiffer = 0;
                        }
                    }
                }
            }

            ////Referral points per agreement


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            agreement.ProfessionalBusiness = "Barristers and other occupations agreed by NZI";
            string retrodate = "Unlimited but excluding known or reported incidents, not including any previous practice as a barrister & solicitor unless specifically agreed by Lumley";
            agreement.TerritoryLimit = "Worldwide excluding USA and Canada";
            agreement.Jurisdiction = "Worldwide excluding USA and Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "NZBar SL UW created/modified";
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

