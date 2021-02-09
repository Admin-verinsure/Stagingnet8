using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class ApolloCLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public ApolloCLUWModule()
        {
            Name = "Apollo_CL";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "CL" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm clterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "CL" && ct.DateDeleted == null))
                {
                    clterm.Delete(underwritingUser);
                }
            }

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "clsocialengineeringextpremium", "cl100klimitpremiumbase", "cl100klimitpremiumultra",
                "cl250klimitpremiumultra");

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

            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddYears(-1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            decimal extpremium = 0m;

            string strProfessionalBusiness = "General Insurance Brokers, Life Agents, Investment Advisers, Financial Planning and Mortgage Broking, Consultants and Advisers in the sale of any financial product including referrals to other financial product providers.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;


            string strretrodate = "";
            if (agreement.ClientInformationSheet.PreRenewOrRefDatas.Count() > 0)
            {
                foreach (var preRenewOrRefData in agreement.ClientInformationSheet.PreRenewOrRefDatas)
                {
                    if (preRenewOrRefData.DataType == "preterm")
                    {
                        if (!string.IsNullOrEmpty(preRenewOrRefData.CLRetro))
                        {
                            strretrodate = preRenewOrRefData.CLRetro;
                        }

                    }
                    if (preRenewOrRefData.DataType == "preendorsement" && preRenewOrRefData.EndorsementProduct == "CL")
                    {
                        if (agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == preRenewOrRefData.EndorsementTitle) == null)
                        {
                            ClientAgreementEndorsement clientAgreementEndorsement = new ClientAgreementEndorsement(underwritingUser, preRenewOrRefData.EndorsementTitle, "Exclusion", product, preRenewOrRefData.EndorsementText, 130, agreement);
                            agreement.ClientAgreementEndorsements.Add(clientAgreementEndorsement);
                        }
                    }
                }
            }


            ClientAgreementEndorsement cAECLExt = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Social Engineering Fraud Extension");
            ClientAgreementEndorsement cAECLDRB = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Data Recovery and Business Interruption Exclusion (DRB)");
            ClientAgreementEndorsement cAECLUPM = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Unencrypted Portable Media Exclusion (UPM)");

            if (cAECLExt != null)
            {
                cAECLExt.DateDeleted = DateTime.UtcNow;
                cAECLExt.DeletedBy = underwritingUser;
            }
            if (cAECLDRB != null)
            {
                cAECLDRB.DateDeleted = DateTime.UtcNow;
                cAECLDRB.DeletedBy = underwritingUser;
            }
            if (cAECLUPM != null)
            {
                cAECLUPM.DateDeleted = DateTime.UtcNow;
                cAECLUPM.DeletedBy = underwritingUser;
            }

            bool ultraoption = false;
            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").First().Value == "1")
            {
                ultraoption = true;
            }

            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").First().Value == "1" && 
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
            {
                extpremium = rates["clsocialengineeringextpremium"];

                if (cAECLExt != null)
                {
                    cAECLExt.DateDeleted = null;
                    cAECLExt.DeletedBy = null;
                }
            }
            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasAccessControlOptions").First().Value == "2")
            {
                if (cAECLUPM != null)
                {
                    cAECLUPM.DateDeleted = null;
                    cAECLUPM.DeletedBy = null;
                }
            }
            if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1" &&
                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasBackupOptions").First().Value == "2")
            {
                if (cAECLDRB != null)
                {
                    cAECLDRB.DateDeleted = null;
                    cAECLDRB.DeletedBy = null;
                }
            }


            int TermExcess = 0;
            TermExcess = 2500;

            if (!ultraoption)
            {
                int TermLimit100kBase = 100000;
                decimal TermPremium100kBase = 0m;
                decimal TermBrokerage100kBase = 0m;
                TermPremium100kBase = rates["cl100klimitpremiumbase"] + extpremium;

                //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
                TermPremium100kBase = TermPremium100kBase / coverperiodindays * agreementperiodindays;

                TermBrokerage100kBase = TermPremium100kBase * agreement.Brokerage / 100;

                ClientAgreementTerm termcl100klimitoptionbase = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit100kBase, TermExcess);
                termcl100klimitoptionbase.TermLimit = TermLimit100kBase;
                termcl100klimitoptionbase.Premium = TermPremium100kBase;
                termcl100klimitoptionbase.BasePremium = TermPremium100kBase;
                termcl100klimitoptionbase.Excess = TermExcess;
                termcl100klimitoptionbase.BrokerageRate = agreement.Brokerage;
                termcl100klimitoptionbase.Brokerage = TermBrokerage100kBase;
                termcl100klimitoptionbase.DateDeleted = null;
                termcl100klimitoptionbase.DeletedBy = null;

                //Change policy premium claculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
                    var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                    foreach (var term in PreviousAgreement.ClientAgreementTerms)
                    {
                        if (term.Bound)
                        {
                            var PreviousBoundPremium = term.Premium;
                            if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                            {
                                PreviousBoundPremium = term.BasePremium;
                            }
                            termcl100klimitoptionbase.PremiumDiffer = (TermPremium100kBase - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            termcl100klimitoptionbase.PremiumPre = PreviousBoundPremium;
                            if (termcl100klimitoptionbase.TermLimit == term.TermLimit && termcl100klimitoptionbase.Excess == term.Excess)
                            {
                                termcl100klimitoptionbase.Bound = true;
                            }
                            if (termcl100klimitoptionbase.PremiumDiffer < 0)
                            {
                                termcl100klimitoptionbase.PremiumDiffer = 0;
                            }

                        }

                    }
                }


            } else
            {
                int TermLimit100kUltra = 100000;
                decimal TermPremium100kUltra = 0m;
                decimal TermBrokerage100kUltra = 0m;
                TermPremium100kUltra = rates["cl100klimitpremiumultra"] + extpremium;

                //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
                TermPremium100kUltra = TermPremium100kUltra / coverperiodindays * agreementperiodindays;

                TermBrokerage100kUltra = TermPremium100kUltra * agreement.Brokerage / 100;

                ClientAgreementTerm termcl100klimitoptionultra = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit100kUltra, TermExcess);
                termcl100klimitoptionultra.TermLimit = TermLimit100kUltra;
                termcl100klimitoptionultra.Premium = TermPremium100kUltra;
                termcl100klimitoptionultra.BasePremium = TermPremium100kUltra;
                termcl100klimitoptionultra.Excess = TermExcess;
                termcl100klimitoptionultra.BrokerageRate = agreement.Brokerage;
                termcl100klimitoptionultra.Brokerage = TermBrokerage100kUltra;
                termcl100klimitoptionultra.DateDeleted = null;
                termcl100klimitoptionultra.DeletedBy = null;

                int TermLimit250kUltra = 250000;
                decimal TermPremium250kUltra = 0m;
                decimal TermBrokerage250kUltra = 0m;
                TermPremium250kUltra = rates["cl250klimitpremiumultra"] + extpremium;

                //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
                TermPremium250kUltra = TermPremium250kUltra / coverperiodindays * agreementperiodindays;

                TermBrokerage250kUltra = TermPremium250kUltra * agreement.Brokerage / 100;

                ClientAgreementTerm termcl250klimitoptionultra = GetAgreementTerm(underwritingUser, agreement, "CL", TermLimit250kUltra, TermExcess);
                termcl250klimitoptionultra.TermLimit = TermLimit250kUltra;
                termcl250klimitoptionultra.Premium = TermPremium250kUltra;
                termcl250klimitoptionultra.BasePremium = TermPremium250kUltra;
                termcl250klimitoptionultra.Excess = TermExcess;
                termcl250klimitoptionultra.BrokerageRate = agreement.Brokerage;
                termcl250klimitoptionultra.Brokerage = TermBrokerage250kUltra;
                termcl250klimitoptionultra.DateDeleted = null;
                termcl250klimitoptionultra.DeletedBy = null;

                //Change policy premium claculation
                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                {
                    var PreviousAgreement = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                    foreach (var term in PreviousAgreement.ClientAgreementTerms)
                    {
                        if (term.Bound)
                        {
                            var PreviousBoundPremium = term.Premium;
                            if (term.BasePremium > 0 && PreviousAgreement.ClientInformationSheet.IsChange)
                            {
                                PreviousBoundPremium = term.BasePremium;
                            }
                            termcl100klimitoptionultra.PremiumDiffer = (TermPremium100kUltra - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            termcl100klimitoptionultra.PremiumPre = PreviousBoundPremium;
                            if (termcl100klimitoptionultra.TermLimit == term.TermLimit && termcl100klimitoptionultra.Excess == term.Excess)
                            {
                                termcl100klimitoptionultra.Bound = true;
                            }
                            if (termcl100klimitoptionultra.PremiumDiffer < 0)
                            {
                                termcl100klimitoptionultra.PremiumDiffer = 0;
                            }
                            termcl250klimitoptionultra.PremiumDiffer = (TermPremium250kUltra - PreviousBoundPremium) * coverperiodindaysforchange / agreementperiodindays;
                            termcl250klimitoptionultra.PremiumPre = PreviousBoundPremium;
                            if (termcl250klimitoptionultra.TermLimit == term.TermLimit && termcl250klimitoptionultra.Excess == term.Excess)
                            {
                                termcl250klimitoptionultra.Bound = true;
                            }
                            if (termcl250klimitoptionultra.PremiumDiffer < 0)
                            {
                                termcl250klimitoptionultra.PremiumDiffer = 0;
                            }

                        }

                    }
                }

            }

            //Referral points per agreement
            //Not a renewal of an existing policy
            uwrfnotrenewalcl(underwritingUser, agreement);


            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            string retrodate = agreement.InceptionDate.ToString("dd/MM/yyyy");
            agreement.TerritoryLimit = "Worldwide excluding USA/Canada";
            agreement.Jurisdiction = "Worldwide excluding USA/Canada";
            agreement.RetroactiveDate = retrodate;
            if (!String.IsNullOrEmpty(strretrodate))
            {
                agreement.RetroactiveDate = strretrodate;
            }

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Apollo CL UW created/modified";
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


        void uwrfnotrenewalcl(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotrenewalcl").OrderNumber));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.Product.IsOptionalProduct && agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasExistingPolicyOptions").First().Value == "2")
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotrenewalcl" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }
            }
        }




    }
}


