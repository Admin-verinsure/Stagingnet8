using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class PMINZEDUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public PMINZEDUWModule()
        {
            Name = "PMINZ_ED";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "ED" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm edterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "ED" && ct.DateDeleted == null))
                {
                    edterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "ed500klimitpremium");

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
            //            if (!string.IsNullOrEmpty(preRenewOrRefData.EDRetro))
            //            {
            //                strretrodate = preRenewOrRefData.EDRetro;
            //            }

            //        }
            //        if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "ED")
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
                var renewFromAgreement = agreement.ClientInformationSheet.RenewFromInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "ED"));

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

            int TermLimit500k = 500000;
            decimal TermPremium500k = rates["ed500klimitpremium"];
            decimal TermBrokerage500k = 0m;

            int TermExcess = 0;

            ClientAgreementTerm termsl500klimitoption = GetAgreementTerm(underwritingUser, agreement, "ED", TermLimit500k, TermExcess);
            termsl500klimitoption.TermLimit = TermLimit500k;
            termsl500klimitoption.Premium = TermPremium500k;
            termsl500klimitoption.Excess = TermExcess;
            termsl500klimitoption.BrokerageRate = agreement.Brokerage;
            termsl500klimitoption.Brokerage = TermBrokerage500k;
            termsl500klimitoption.DateDeleted = null;
            termsl500klimitoption.DeletedBy = null;
            termsl500klimitoption.BasePremium = TermPremium500k;

            //Change policy premium calculation
            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
            {
                var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "ED"));
                foreach (var term in PreviousAgreement.ClientAgreementTerms)
                {
                    if (term.Bound)
                    {
                        var PreviousBoundPremium = term.Premium;
                        if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                        {
                            PreviousBoundPremium = term.BasePremium;
                        }
                        termsl500klimitoption.PremiumDiffer = (TermPremium500k - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                        termsl500klimitoption.PremiumPre = PreviousBoundPremium;
                        if (termsl500klimitoption.TermLimit == term.TermLimit && termsl500klimitoption.Excess == term.Excess)
                        {
                            termsl500klimitoption.Bound = true;
                        }
                        if (termsl500klimitoption.PremiumDiffer < 0)
                        {
                            termsl500klimitoption.PremiumDiffer = 0;
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

            //Referral points per agreement
            //ED Issues
            uwredissue(underwritingUser, agreement);


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = "Inception or Date since ED policy first held";
            agreement.TerritoryLimit = "New Zealand";
            agreement.Jurisdiction = "New Zealand";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            string auditLogDetail = "PMINZ ED UW created/modified";
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

        void uwredissue(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwredissue" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwredissue").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                {
                    if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwredissue" && cref.DateDeleted == null).Status != "Pending")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.CoveredOptions").First().Value == "2" ||
                            (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.CoveredOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.LegalAdvisorOptions").First().Value == "2") ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.IsInsuredClaimOptions").First().Value == "1")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwredissue" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }

                    if (agreement.ClientInformationSheet.IsRenewawl
                                && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwredissue" && cref.DateDeleted == null).DoNotCheckForRenew)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwredissue" && cref.DateDeleted == null).Status = "";
                    }
                }
            }
        }



    }
}
