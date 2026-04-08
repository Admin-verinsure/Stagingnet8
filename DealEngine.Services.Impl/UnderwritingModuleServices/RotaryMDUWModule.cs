using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl.UnderwritingModuleServices
{
    public class RotaryMDUWModule : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public RotaryMDUWModule()
        {
            Name = "Rotary_MD";
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

            if (agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "MD" && ct.DateDeleted == null) != null)
            {
                foreach (ClientAgreementTerm mdterm in agreement.ClientAgreementTerms.Where(ct => ct.SubTermType == "MD" && ct.DateDeleted == null))
                {
                    mdterm.Delete(underwritingUser);
                }
            }

            //IDictionary<string, decimal> rates = BuildRulesTable(agreement, "mdpremium",);

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "mdexcess", "mdlimit", "mdpremium");
            //IDictionary<string, decimal> rates = BuildRulesTable(agreement, "mdexcess", "mdlimit", "mdpremium", "mdextensionpremiumover", "mdadditionaladminfeeover", "mdadditionaladminfeeover", "mdstandardadminfee");

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

            bool bolworkoutsidenz = false;
            bool assetover5000 = false;
            bool runout = false;


            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "MLViewModel.HasRunoutOptions").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "MLViewModel.HasRunoutOptions").First().Value == "1")
                {
                    runout = true;
                }
            }

            
            if (agreement.ClientInformationSheet.RevenueData != null)
            {
                foreach (var uISTerritory in agreement.ClientInformationSheet.RevenueData.Territories)
                {
                    if (!bolworkoutsidenz && uISTerritory.Location != "New Zealand" && uISTerritory.Percentage > 0) //Work outside New Zealand Check
                    {
                        bolworkoutsidenz = true;
                    }
                }
            }

            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "TAViewModel.HasClubTrustAssetMore").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "TAViewModel.HasClubTrustAssetMore").First().Value == "1")
                {
                    assetover5000 = true;
                }
            }


            bool isclubtrustselect = false;
            ////ClientInformationAnswer TransitionalLicenseNum = await _clientInformationAnswer.GetSheetAnsByName("FAPViewModel.TransitionalLicenseNum", clientInformationSheetID);

            //if (agreement.ClientInformationSheet.)
            bool HasClubTrustAssetMore = false;
            int extLimit = 0;
            int extExcess = 500;
            decimal extPremium = 0M;
            decimal adminFee = 0M;
            int extLoading = 0;
            bool extRequired = false;
            if (agreement.ClientInformationSheet.ClubTrustAssetsInfo != null)
            {
                foreach (var clubTrustAssetsInfo in agreement.ClientInformationSheet.ClubTrustAssetsInfo)
                {
                    if (clubTrustAssetsInfo.CurrentVal > 25000)
                    {
                        decimal dec1 = Convert.ToDecimal(clubTrustAssetsInfo.CurrentVal - 25000) / 5000;
                        extLoading += Convert.ToInt32(Math.Ceiling(dec1));
                        extLimit += clubTrustAssetsInfo.CurrentVal;
                        extRequired = true;
                    }
                }
            }
            //extPremium = extLoading * rates["mdextensionpremiumover"];
            //adminFee = rates["mdstandardadminfee"] + extLoading * rates["mdadditionaladminfeeover"];

            int agreementperiodindays = 0;
            agreementperiodindays = (agreement.ExpiryDate - agreement.InceptionDate).Days;

            agreement.QuoteDate = DateTime.UtcNow;

            int coverperiodindays = 0;
            coverperiodindays = (agreement.ExpiryDate - agreement.ExpiryDate.AddYears(-1)).Days;

            int coverperiodindaysforchange = 0;
            coverperiodindaysforchange = (agreement.ExpiryDate - DateTime.UtcNow).Days;

            string strProfessionalBusiness = "Community Services, fundraising activities and public events.";

            agreement.ProfessionalBusiness = strProfessionalBusiness;

            int TermExcess = 0;
            TermExcess = Convert.ToInt32(rates["mdexcess"]);

            int TermLimit = 0;
            TermLimit = Convert.ToInt32(rates["mdlimit"]);
            decimal TermPremium = 0M;
            decimal TermBrokerage = 0M;
            //TermPremium = rates["mdpremium"];


            //Enable pre-rate premium (turned on after implementing change, any remaining policy and new policy will use be pre-rated)
            TermPremium = TermPremium / coverperiodindays * agreementperiodindays;
            TermBrokerage = TermPremium * agreement.Brokerage / 100;
            var attr = informationSheet?.OrganisationAttribute;
            decimal premium = 0;

            TermPremium += CalculatePremium(informationSheet, attr);


            ClientAgreementTerm termoption = GetAgreementTerm(underwritingUser, agreement, "MD", TermLimit, TermExcess);
            termoption.TermLimit = TermLimit;
            termoption.Premium = TermPremium;
            termoption.BasePremium = TermPremium;
            termoption.Excess = TermExcess;
            termoption.BrokerageRate = agreement.Brokerage;
            termoption.Brokerage = TermBrokerage;
            termoption.DateDeleted = null;
            termoption.DeletedBy = null;

            //add Extension Additional cover for Scheduled assets extension
            foreach (ClientAgreementTermExtension mdtermextension in agreement.ClientAgreementTermExtensions.Where(ctex => ctex.DateDeleted == null))
            {
                mdtermextension.Delete(underwritingUser);
            }

            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "TAViewModel.HasClubTrustAssetMore").Any())
            {
                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "TAViewModel.HasClubTrustAssetMore").First().Value == "1")
                {
                    if (extRequired)
                    {
                        ClientAgreementTermExtension termMDextension = GetAgreementExtensionTerm(underwritingUser, agreement, extLimit, extExcess, extPremium, "Extension Additional cover for Scheduled assets");
                        termMDextension.ExtentionName = "Extension Additional cover for Scheduled assets";
                        termMDextension.HideLimitExcess = false;
                        termMDextension.Premium = extPremium;
                        termMDextension.BasePremium = extPremium;
                        termMDextension.DateDeleted = null;
                        termMDextension.DeletedBy = null;
                        termMDextension.Bound = true;
                    }
                }
            }

            if (adminFee > 1750)
            {
                adminFee = 1750;
            }

            agreement.BrokerFee = 37.5m; 

            //Referral points per agreement
            //Operates Outside of NZ
            uwrfoperatesoutsideofnz(underwritingUser, agreement, bolworkoutsidenz);
            if (assetover5000 || runout)
            {
                UWTask(underwritingUser, agreement, assetover5000, runout);

            }
           

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            agreement.TerritoryLimit = "New Zealand";
            agreement.Jurisdiction = "New Zealand";

            agreement.InsuredName = informationSheet.Owner.Name;

            string auditLogDetail = "Rotary MD UW created/modified";
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

        //Extension Additional cover for Scheduled assets extension
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

        void UWTask(User underwritingUser, ClientAgreement agreement, bool assetover5000 , bool runout)
        {
            if (assetover5000)
            {
                bool alreadyExists = agreement.Product.OdooTaskSpecs
                    .Any(t => t.Title == "Case trade MD cover over $5000");

                if (!alreadyExists)
                {
                    var odooTaskSpec = new OdooTaskSpec(
                        "Case trade MD cover over $5000",
                        agreement.ClientInformationSheet.Owner.OdooProjectId,
                        agreement.Product,
                        notes: "Case trade MD cover over $5000"
                    );

                    agreement.Product.OdooTaskSpecs.Add(odooTaskSpec);
                }
            }


            if (runout)
            {
                bool alreadyExists = agreement.Product.OdooTaskSpecs
                    .Any(t => t.Title == "Case trade runout cover");

                if (!alreadyExists)
                {
                    var odooTaskSpec = new OdooTaskSpec(
                        "Case trade runout cover",
                        44,
                        agreement.Product,
                        notes: "Case trade runout cover"
                    );

                    agreement.Product.OdooTaskSpecs.Add(odooTaskSpec);
                }
            }




        }



        private decimal CalculatePremium(ClientInformationSheet informationSheet, OrganisationAttribute attr)
        {
            decimal entityChargeTotal = 0m;
            //const decimal GST = 0.15m;
            // decimal BrokerFee = 0m;


            foreach (var organisation in  informationSheet.Organisation.Where(o => o.DateDeleted == null && !o.Removed && o.OrganisationType.Name != "Private"))
            {
                
                    foreach (var unit in organisation.OrganisationalUnits.Where(u => u.DateDeleted == null))
                    {
                        // Skip "Administrator" units
                        if (unit.Name == "Administrator" || unit.Name == "Person - Individual" || unit.Name  == "Corporation – Limited liability"
                        || unit.Name == "Head Office")
                            continue;

                        // Add entity charge ($195 per active entity)
                        entityChargeTotal += 195m;
                    }
                
                   
            }
            // entityChargeTotal += entityChargeTotal ;


            return entityChargeTotal;

        }
    }
}

