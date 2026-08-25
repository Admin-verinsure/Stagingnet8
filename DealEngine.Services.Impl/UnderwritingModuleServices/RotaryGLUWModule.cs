using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Crypto.Agreement;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class RotaryGLUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; } = "Rotary_MLGGL";

        public RotaryGLUWModule() { Name = "Rotary_MLGGL"; }
        public bool Underwrite(User CurrentUser, ClientInformationSheet informationSheet) { throw new NotImplementedException(); }

        private const decimal GST = 0.15m;

        public bool Underwrite(User underwritingUser, ClientInformationSheet informationSheet, Product product, string reference)
        {
            ClientAgreement agreement = GetClientAgreement(underwritingUser, informationSheet, informationSheet.Programme, product, reference);
            bool orgtype = false;

            try
            {

                if (agreement.ClientAgreementRules.Count == 0)
                    foreach (var rule in product.Rules.Where(r => !string.IsNullOrWhiteSpace(r.Name)))
                        agreement.ClientAgreementRules.Add(new ClientAgreementRule(underwritingUser, rule, agreement));

                if (agreement.ClientAgreementEndorsements.Count == 0)
                    foreach (var endorsement in product.Endorsements.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
                        agreement.ClientAgreementEndorsements.Add(new ClientAgreementEndorsement(underwritingUser, endorsement, agreement));

                if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "GL" && ct.DateDeleted == null) != null)
                {
                    foreach (ClientAgreementTerm asterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "GL" && ct.DateDeleted == null))
                    {
                        asterm.Delete(underwritingUser);
                    }
                }

                IDictionary<string, decimal> rates = BuildRulesTable(agreement, "plpremium_rotary_upto15_members", "plpremium_district_over15_permember",
                                                                                 "plpremium_district_upto40_members", "plpremium_district_over40_permember",
                                                                                 "plpremium_club_nontrading_trust", "plpremium_special_trading_trust_company",
                                                                                 "plpremium_special_trading_over1m", "plpremium_local_chapter_fellowship");

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

                agreement.QuoteDate = DateTime.UtcNow;

                string strProfessionalBusiness = "Community Services, fundraising activities and public events.";

                agreement.ProfessionalBusiness = strProfessionalBusiness;

                int TermExcess = 0;
                TermExcess = 5000;

                int TermLimit1mil = 10000000;
                // var organisation = informationSheet.Organisation.FirstOrDefault();
                var attr = informationSheet?.OrganisationAttribute;
                decimal premium = 0;
                agreement.BrokerFee = 0;
                var clubtrust1only = 0;
                IList<Organisation> organisations = informationSheet.Organisation ?? new List<Organisation>();
                bool isOutsideNZ = informationSheet.Owner != null
                   && informationSheet.Owner.IsOutsideNZ;

                     // Normal rating flow.
                    foreach (Organisation organisation in organisations.Where(org => org.Removed == false && org.OrganisationType?.Name != "Private"))
                    {
                        var orgAttr = organisation?.OrganisationAttribute;
                        var effectiveAttr = HasAnyRotaryRatingInput(orgAttr) ? orgAttr : attr;
                        decimal orgPremium = 0m;

                        foreach (var unit in (organisation.OrganisationalUnits ?? new List<OrganisationalUnit>()).Where(u => u.DateDeleted == null))
                        {
                            if (unit.Name == "RotaryClubTrustOneOnly")
                            {
                                clubtrust1only += 1;
                            }

                            // =============================================
                            // FULL ANNUAL PREMIUM
                            // =============================================
                            orgPremium += CalculatePremium(
                                unit.Name,
                                effectiveAttr,
                                agreement,
                                clubtrust1only, rates);
                        }

                        // Fallback for clients that have no unit rows or non-rating unit names.
                        if (orgPremium <= 0 && !string.IsNullOrWhiteSpace(organisation.OrganisationType?.Name))
                        {
                            orgPremium += CalculatePremium(
                                organisation.OrganisationType.Name,
                                effectiveAttr,
                                agreement,
                                clubtrust1only,
                                rates);
                        }

                        premium += orgPremium;
                    }



                if (isOutsideNZ)
                {
                    // 🔥 SPECIAL RULE
                    // $2.50 per month → convert to yearly or prorated

                    decimal monthlyPremium = 2.50m;

                    // yearly base
                    decimal yearlyPremium = monthlyPremium * 12;

                    premium = yearlyPremium;
                }
                else
                {
                    // =============================================
                    // PRORATA PREMIUM
                    // =============================================

                    DateTime fullAnnualExpiry = agreement.InceptionDate.AddYears(1);

                    decimal fullAnnualDays =
                        (decimal)(fullAnnualExpiry - agreement.InceptionDate).TotalDays;

                    decimal actualPolicyDays =
                        (decimal)(agreement.ExpiryDate - agreement.InceptionDate).TotalDays;
                    // Apply prorata
                    premium = Math.Round(
                        (premium / fullAnnualDays) * actualPolicyDays,
                        2);

                }


                bool Doesvolunteerpolicechecked = true;
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "GLViewModel.Doesvolunteerpolicechecked").Any())
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "GLViewModel.Doesvolunteerpolicechecked").First().Value == "2")
                    {
                        Doesvolunteerpolicechecked = false;
                    }
                }

                bool HasClubTrustEvent = true;
                bool ExtraCoverOptions = true;


                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EventsViewModel.HasClubTrustEvent").Any())
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EventsViewModel.HasClubTrustEvent").First().Value == "1")
                    {
                        HasClubTrustEvent = true;
                    }
                }

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EventsViewModel.ExtraCoverOptions").Any())
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EventsViewModel.ExtraCoverOptions").First().Value == "1")
                    {
                        ExtraCoverOptions = true;
                    }
                }

                if (orgtype || !Doesvolunteerpolicechecked)
                {
                    // UWTask(underwritingUser, agreement, orgtype, Doesvolunteerpolicechecked);
                    UWTask(underwritingUser, agreement, orgtype, Doesvolunteerpolicechecked, HasClubTrustEvent, ExtraCoverOptions);

                }

                ClientAgreementTerm term1millimitpremiumoption = GetAgreementTerm(underwritingUser, agreement, "GL", TermLimit1mil, TermExcess);
                term1millimitpremiumoption.TermLimit = TermLimit1mil;
                term1millimitpremiumoption.Premium = premium;
                term1millimitpremiumoption.BasePremium = premium;
                term1millimitpremiumoption.Excess = TermExcess;
                term1millimitpremiumoption.BrokerageRate = agreement.Brokerage;
                term1millimitpremiumoption.Brokerage = 0m;
                term1millimitpremiumoption.DateDeleted = null;
                term1millimitpremiumoption.DeletedBy = null;
                decimal targetPremium = 50m * 1.15m; // 50 + 15%


                if (premium == targetPremium)
                {
                    agreement.BrokerFee = 0;
                }


                //Referral points per agreement
                uwrasreferral(underwritingUser, agreement);
                uwrfpriorinsurance(underwritingUser, agreement);

                //Update agreement status
                if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
                {
                    agreement.Status = "Referred";
                }
                else
                {
                    agreement.Status = "Quoted";
                }

                agreement.TerritoryLimit = "Worldwide";
                agreement.Jurisdiction = "Worldwide";

                agreement.InsuredName = informationSheet.Owner.Name;

                string auditLogDetail = "Rotary AS UW created/modified";
                AuditLog auditLog = new AuditLog(underwritingUser, informationSheet, agreement, auditLogDetail);
                agreement.ClientAgreementAuditLogs.Add(auditLog);

               
            }
            catch (Exception)
            {
                agreement.Status = "Referred";
            }
            return true;

        }


        void UWTask(User underwritingUser, ClientAgreement agreement ,bool orgtype, bool Doesvolunteerpolicechecked 
                                         , bool HasClubTrustEvent, bool ExtraCoverOptions)
        {
            if (orgtype)
            {
                bool alreadyExists = agreement.Product.OdooTaskSpecs
                    .Any(t => t.Title == "Trust governance check");

                if (!alreadyExists)
                {
                    var odooTaskSpec = new OdooTaskSpec(
                        "Trust governance check",
                        agreement.ClientInformationSheet.Owner.OdooProjectId,
                        agreement.Product,
                        notes: "Trust governance check"
                    );

                    agreement.Product.OdooTaskSpecs.Add(odooTaskSpec);
                }
            }

            if (Doesvolunteerpolicechecked)
            {
                bool alreadyExists = agreement.Product.OdooTaskSpecs
                    .Any(t => t.Title == " Youth programme check");

                if (!alreadyExists)
                {
                    var odooTaskSpec = new OdooTaskSpec(
                        "Case  Youth programme check",
                        44,
                        agreement.Product,
                        notes: " Youth programme check"
                    );

                    agreement.Product.OdooTaskSpecs.Add(odooTaskSpec);
                }
            }

            if (HasClubTrustEvent || ExtraCoverOptions)
            {
                bool alreadyExists = agreement.Product.OdooTaskSpecs
                    .Any(t => t.Title == " Event management check");

                if (!alreadyExists)
                {
                    var odooTaskSpec = new OdooTaskSpec(
                        "Case  Event management check",
                        44,
                        agreement.Product,
                        notes: "Event management check"
                    );

                    agreement.Product.OdooTaskSpecs.Add(odooTaskSpec);
                }
            }

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
            {
                var agreementRule = agreement.ClientAgreementRules
                    .FirstOrDefault(r => r.DateDeleted == null && string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));

                var raw = agreementRule?.Value;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    var productRule = agreement.Product?.Rules
                        .FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
                    raw = productRule?.Value;
                }

                if (string.IsNullOrWhiteSpace(raw))
                {
                    throw new InvalidOperationException("Missing required GL rating rule: " + name);
                }

                if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    throw new InvalidOperationException("Invalid numeric value for GL rating rule: " + name + " (value='" + raw + "')");
                }

                dict[name] = parsedValue;
            }

            return dict;
        }

        bool HasAnyRotaryRatingInput(OrganisationAttribute attr)
        {
            if (attr == null)
            {
                return false;
            }

            return (attr.ActiveFeePaying ?? 0) > 0
                   || (attr.Family ?? 0) > 0
                   || (attr.Corporate ?? 0) > 0
                   || (attr.DistrictTotal ?? 0) > 0
                   || (attr.SPT_Total ?? 0) > 0
                   || (attr.SPT_Revenue ?? 0) > 0;
        }

        void uwrasreferral(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrasreferral" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrasreferral").DoNotCheckForRenew));
                
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrasreferral" && cref.DateDeleted == null).Status != "Pending")
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrasreferral" && cref.DateDeleted == null).Status = "Pending";
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrasreferral" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrasreferral" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        private decimal CalculatePremium(string orgType, OrganisationAttribute attr, ClientAgreement agreement, int clubtrust1only, IDictionary<string, decimal> rates)
        {
            decimal total = 0m;

            // =============================================
            // COUNTED MEMBERS (different for each org type)
            // =============================================
            int countedMembers = 0;

            if (orgType == "RotaryClub")
            {
                countedMembers =
                    (attr?.ActiveFeePaying ?? 0) +
                    (attr?.Family ?? 0) +
                    ((attr?.Corporate ?? 0) * 3);
            }
            else if (orgType == "Rotaract" || orgType == "RotaryCommunityCorp" || orgType == "RotaryCommunityCorps")
            {
                countedMembers += 1;   // Each counts as 1 club/member equivalent
            }

            // =============================================
            // 1️⃣ CLUB PREMIUMS (Rotary, Rotaract, Community Corp)
            // =============================================
            if (orgType == "RotaryClub" ||
                orgType == "Rotaract" ||
                orgType == "RotaryCommunityCorp" ||
                orgType == "RotaryCommunityCorps")
            {
                decimal basePremium = rates["plpremium_rotary_upto15_members"]; // includes admin fee
                if (countedMembers < 15)
                {
                    total += basePremium;
                }
                else
                {
                    int extraMembers = countedMembers - 15;
                    total += basePremium + (extraMembers * rates["plpremium_district_over15_permember"]);
                }
                agreement.BrokerFee += 37.50m;
            }

            // =============================================
            // 2️⃣ DISTRICT PREMIUMS
            // =============================================
            else if (orgType == "RotaryDistrict")
            {
                int clubCount = attr?.DistrictTotal ?? 0;

                if (clubCount <= 40)
                {
                    total += rates["plpremium_district_upto40_members"];
                }
                else
                {
                    total += rates["plpremium_district_upto40_members"] + ((clubCount - 40) * rates["plpremium_district_over40_permember"]);
                }
                agreement.BrokerFee += 37.50m;

            }

            // =============================================
            // 3️⃣ TRUST (ONE ONLY INCLUDED)
            // =============================================
            else if (orgType == "RotaryClubTrustOneOnly")
            {

                if (clubtrust1only > 1)
                {
                    total += (clubtrust1only - 1) * 195m;
                    agreement.BrokerFee += 37.50m;
                }
            }

            // =============================================
            // 4️⃣ SPECIAL PURPOSE TRUST / COMPANY
            // =============================================
            else if (orgType == "RotarySpecialPurposeTrust" ||
                     orgType == "RotaryCompany")
            {

                decimal basePremium = rates["plpremium_special_trading_trust_company"];
                bool over1m = false;
                if (attr != null)
                {
                    over1m = attr.SPT_RevenueOver1m?.ToLower() == "yes";
                }

                if (over1m)
                {
                    decimal revenue = attr.SPT_Revenue ?? 0;

                    // Count number of extra $1 million blocks
                    decimal increments = Math.Ceiling((revenue - 1_000_000m) / 1_000_000m);

                    if (increments > 0)
                        basePremium += increments * rates["plpremium_special_trading_over1m"];
                }

                total += basePremium;
                agreement.BrokerFee += 37.50m;

            }

            else if (orgType == "RotaryMultiDistrictOrganisation" ||
                     orgType == "RotaryRIActionGroup" ||
                     orgType == "RotaryRIFellowshipGroup" ||
                     orgType == "RotaryOther")
            {
                decimal basePremium = rates["plpremium_local_chapter_fellowship"];
                total += basePremium;
                agreement.BrokerFee += 37.50m;

            }

            // =============================================
            // APPLY GST LAST
            // =============================================
            // total += total * GST;

            return total;
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


    }
}
