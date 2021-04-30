using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class PMINZELUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public PMINZELUWModule()
        {
            Name = "PMINZ_EL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "EL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm elterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "EL" && ct.DateDeleted == null))
                {
                    elterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "el250klimitpremium", "eltopuppremiumover4employee");

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
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.ELRetro))
            //            {
            //                strretrodate = preRenewOrRefData.ELRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "EL")
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
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "EL"));

                if (renewFromAgreement != null)
                {
                    strretrodate = renewFromAgreement.RetroactiveDate;

                    foreach (var renewendorsement in renewFromAgreement.ClientAgreementEndorsements)
                    {

                        if (renewendorsement.DateDeleted == null)
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, renewendorsement.Name, renewendorsement.Type, product, renewendorsement.Value, renewendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
                        }
                    }
                }


            }


            int TermLimit250k = 250000;
            decimal TermPremium250k = rates["el250klimitpremium"];
            decimal TermBrokerage250k = 0m;

            int TermExcess = 0;

            //Return terms based on the limit options

            TermExcess = 2500;

            if (Convert.ToInt32(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.TotalEmployees").First().Value) > 4)
            {
                TermPremium250k += (Convert.ToInt32(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.TotalEmployees").First().Value) - 4) * rates["eltopuppremiumover4employee"];
            }

            ClientAgreementTerm termsl250klimitoption = GetAgreementTerm(underwritingUser, agreement, "EL", TermLimit250k, TermExcess);
            termsl250klimitoption.TermLimit = TermLimit250k;
            termsl250klimitoption.Premium = TermPremium250k;
            termsl250klimitoption.Excess = TermExcess;
            termsl250klimitoption.BrokerageRate = agreement.Brokerage;
            termsl250klimitoption.Brokerage = TermBrokerage250k;
            termsl250klimitoption.DateDeleted = null;
            termsl250klimitoption.DeletedBy = null;
            termsl250klimitoption.BasePremium = TermPremium250k;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "EL"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termsl250klimitoption.PremiumDiffer = (TermPremium250k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsl250klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termsl250klimitoption.TermLimit == term.TermLimit && termsl250klimitoption.Excess == term.Excess)
                        {
                            termsl250klimitoption.Bound = true;
                        }
                        if (termsl250klimitoption.PremiumDiffer < 0)
                        {
                            termsl250klimitoption.PremiumDiffer = 0;
                        }

                    }

                }

                if (PreviousAgreement != null)
                {
                    strretrodate = PreviousAgreement.RetroactiveDate;

                    foreach (var changeendorsement in PreviousAgreement.ClientAgreementEndorsements)
                    {

                        if (changeendorsement.DateDeleted == null)
                        {
                            ClientAgreementEndorsement newclientendorsement =
                                new ClientAgreementEndorsement(underwritingUser, changeendorsement.Name, changeendorsement.Type, product, changeendorsement.Value, changeendorsement.OrderNumber, agreement);
                            agreement.ClientAgreementEndorsements.Add(newclientendorsement);
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

            string retrodate = "Inception or Date since EL policy first held";
            agreement.TerritoryLimit = "New Zealand";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            string auditLogDetail = "PMINZ EL UW created/modified";
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




    }
}

