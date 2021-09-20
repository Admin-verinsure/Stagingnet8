using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace DealEngine.WebUI.Models
{
    public class InformationViewModel : BaseViewModel
    {
        public InformationViewModel() { }
        public InformationViewModel(ClientInformationSheet clientInformationSheet, User OrgUser, User CurrentUser)
        {
            ELViewModel = new ELViewModel(); //Employment Liability Insurance
            EPLViewModel = new EPLViewModel(); //Employers Practices Insurance
            CLIViewModel = new CLIViewModel(); //Cyber Liability Insurance
            PIViewModel = new PIViewModel(); //Professional Indemnity
            DAOLIViewModel = new DAOLIViewModel(); //Directors officers liability
            GLViewModel = new GLViewModel(); //General liability 
            SLViewModel = new SLViewModel(); //Statutory Liability
            FAPViewModel = new FAPViewModel(); //Financial Advisor
            ClaimsHistoryViewModel = new ClaimsHistoryViewModel();
            OTViewModel = new OTViewModel();//OutsideTrustees
            IPViewModel = new IPViewModel();
            GeneralViewModel = new GeneralViewModel();
            User = CurrentUser;
            Programme = clientInformationSheet.Programme.BaseProgramme;
            RevenueDataViewModel = new RevenueDataViewModel(clientInformationSheet.Programme.BaseProgramme);
            RoleDataViewModel = new RoleDataViewModel(clientInformationSheet.Programme.BaseProgramme);
            LocationViewModel = new LocationViewModel(clientInformationSheet);
            JobViewModel = new JobViewModel(clientInformationSheet);
            ProjectViewModel = new ProjectViewModel(clientInformationSheet);
            ResearchHouseViewModel = new ResearchHouseViewModel(clientInformationSheet);
            OrganisationViewModel = new OrganisationViewModel(clientInformationSheet, OrgUser);
            BoatViewModel = new BoatViewModel();
            ClientInformationSheet = clientInformationSheet;
            Status = clientInformationSheet.Status;
            AnswerSheetId = clientInformationSheet.Id;
            ClientProgramme = clientInformationSheet.Programme;
        }
        public User User { get; set; }
        public OrganisationViewModel OrganisationViewModel { get; set; }
        public BoatViewModel BoatViewModel { get; set; }
        public Domain.Entities.Programme Programme { get; set; }
        public Guid AnswerSheetId { get; set; }
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string ProgNamedPartyName { get; set; }
        public string SectionView { get; set; }
        public String[][] LimitsSelected { get; set; }
        public List<string> selectedUpdateType { get; set; }

        public List<InformationSection> Section { get; set; }
        public List<string> ListSection { get; set; }
        public IEnumerable<InformationSectionViewModel> Sections { get; set; }
        public LocationViewModel LocationViewModel { get; set; }

        //public AdminViewModel AdminViewModel { get; set; }
        public UpdateTypesViewModel UpdateTypesViewModel { get; set; }

        public JobViewModel JobViewModel { get; set; }

        public List<BoatUse> BoatUsesList { get; set; }
        public List<SelectListItem> BoatUseslist { get; set; }
        public IEnumerable<OrganisationViewModel> MarinaLocations { get; set; }
        public IEnumerable<ClaimViewModel> Claims { get; set; }
        public string Advisory { get; set; }
        public RevenueDataViewModel RevenueDataViewModel { get; set; }
        public RoleDataViewModel RoleDataViewModel { get; set; }
        public ClaimsHistoryViewModel ClaimsHistoryViewModel { get; set; }
        public EPLViewModel EPLViewModel { get; set; }
        public ELViewModel ELViewModel { get; set; }
        public CLIViewModel CLIViewModel { get; set; }
        public PIViewModel PIViewModel { get; set; }
        public DAOLIViewModel DAOLIViewModel { get; set; }
        public GLViewModel GLViewModel { get; set; }
        public SLViewModel SLViewModel { get; set; }
        public FAPViewModel FAPViewModel { get; set; }
        public ProjectViewModel ProjectViewModel { get;set;}
        public ResearchHouseViewModel ResearchHouseViewModel { get; set; }
        public IList<string> Wizardsteps { get; set; }
        public ClientInformationSheet ClientInformationSheet { get; set; }
        public ClientProgramme ClientProgramme { get; internal set; }
        public ClientAgreement ClientAgreement { get; internal set; }
        public OTViewModel OTViewModel { get; internal set; }
        public IPViewModel IPViewModel { get; internal set; }
        public GeneralViewModel GeneralViewModel { get; internal set; }

        
    }


    public class InformationSectionViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<InformationItemViewModel> Items { get; set; }

        public string CustomView { get; set; }

        public int Position { get; set; }
    }
    public class InformationItemViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ControlType { get; set; }
        public string Class { get; set; }

        public string Label { get; set; }

        public string Icon { get; set; }

        public int Width { get; set; }

        public ItemType Type { get; set; }

        public string DefaultText { get; set; }

        public IEnumerable<SelectListItem> Options { get; set; }

        public IList<InformationItem> ConditionalList { get; set; }
        public string Value { get; set; }
        public Rule Rule { get; set; }

        public IEnumerable<LabelPersentageViewModel> LabelPercentageValue { get; set; }

        public InformationItemViewModel()
        {
            Options = new List<SelectListItem>();
        }

    }
    public enum ItemType
    {
        LABEL,
        TEXTBOX,
        TEXTAREA,
        DROPDOWNLIST,
        PERCENTAGEBREAKDOWN,
        MULTISELECT,
        JSBUTTON,
        SUBMITBUTTON,
        MOTORVEHICLELIST,
        STATICVEHICLEPLANTLIST,
        SECTIONBREAK
    }
    public class RevenueDataViewModel
    {
        public RevenueDataViewModel() { }
        public RevenueDataViewModel(Domain.Entities.Programme programme)
        {
            Territories = GetTerritories(programme);
            Activities = GetActivities(programme);
            AdditionalActivityViewModel = new AdditionalActivityViewModel();
        }
        private IList<BusinessActivity> GetActivities(Domain.Entities.Programme programme)
        {
            Activities = new List<BusinessActivity>();
            foreach (var template in programme.BusinessActivityTemplates)
            {
                Activities.Add(new BusinessActivity(null)
                {
                    Description = template.Description,
                    AnzsciCode = template.AnzsciCode,
                    Selected = false,
                    Percentage = 0
                });
            }
            return Activities;
        }
        private IList<Territory> GetTerritories(Domain.Entities.Programme programme)
        {
            Territories = new List<Territory>();
            foreach (var template in programme.TerritoryTemplates)
            {
                Territories.Add(new Territory(null)
                {
                    TemplateId = template.Id,
                    Location = template.Location,
                    Percentage = 0,
                    Selected = false
                });
            }
            return Territories;
        }
        public IList<Territory> Territories { get; set; }
        public IList<BusinessActivity> Activities { get; set; }
        public decimal NextFinancialYearTotal { get; set; }
        public decimal CurrentYearTotal { get; set; }
        public decimal LastFinancialYearTotal { get; set; }
        public AdditionalActivityViewModel AdditionalActivityViewModel { get; set; }
    }
    public class AdditionalActivityViewModel
    {
        public AdditionalActivityViewModel(AdditionalActivityInformation additionalActivityInformation = null)
        {
            SetOptions();
        }

        public void SetOptions()
        {
            HasInspectionReportOptions = GetSelectListOptions();
            HasDisclaimerReportsOptions = GetSelectListOptions();
            HasObservationServicesOptions = GetSelectListOptions();
            HasRecommendedCladdingOptions = GetSelectListOptions();
            HasStateSchoolOptions = GetSelectListOptions();
            HasIssuedCertificatesOptions = GetSelectListOptions();
        }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        public IList<SelectListItem> HasInspectionReportOptions { get; set; }
        public IList<SelectListItem> HasDisclaimerReportsOptions { get; set; }
        public IList<SelectListItem> HasObservationServicesOptions { get; set; }
        public IList<SelectListItem> HasRecommendedCladdingOptions { get; set; }
        public IList<SelectListItem> HasStateSchoolOptions { get; set; }
        public IList<SelectListItem> HasIssuedCertificatesOptions { get; set; }
        public string QualificationDetails { get; set; }
        public string ValuationDetails { get; set; }
        public string OtherDetails { get; set; }
        public string OtherServices { get; set; }
        public string RebuildDetails { get; set; }
        public string InspectionReportDetails { get; set; }
        public string OtherProjectManagementDetails { get; set; }
        public string NonProjectManagementDetails { get; set; }
        public decimal ConstructionCommercialDetails { get; set; }
        public decimal ConstructionDwellingDetails { get; set; }
        public decimal ConstructionIndustrialDetails { get; set; }
        public decimal ConstructionInfrastructureDetails { get; set; }
        public decimal ConstructionSchoolDetails { get; set; }
        public string ConstructionEngineerDetails { get; set; }
        
    }
    public class RoleDataViewModel
    {
        public RoleDataViewModel() { }
        public RoleDataViewModel(Domain.Entities.Programme programme)
        {
            DataRoles = GetRoles(programme);            
        }

        private IList<SharedDataRole> GetRoles(Domain.Entities.Programme programme)
        {
            DataRoles = new List<SharedDataRole>();
            foreach (var template in programme.SharedDataRoleTemplates)
            {
                DataRoles.Add(new SharedDataRole(null)
                {
                    TemplateId = template.Id,
                    Name = template.Name,
                    Total = 0,
                    Selected = false
                });
            }
            return DataRoles;
        }

        public AdditionalRoleInformationViewModel AdditionalRoleInformationViewModel { get; set; }
        public IList<SharedDataRole> DataRoles { get; set; }
    }
    public class AdditionalRoleInformationViewModel
    {
        public string OtherDetails { get; set; }
    }
    public class BusinessActivityViewModel
    {
        public int Classification { get; set; }
        public string AnzsciCode { get; set; }
        public string Description { get; set; }
    }
    public class ClaimsHistoryViewModel
    {
        public ClaimsHistoryViewModel()
        {
            HasDamageLossOptions = GetSelectListOptions();
            HasWithdrawnOptions = GetSelectListOptions();
            HasRefusedOptions = GetSelectListOptions();
            HasStatutoryOffenceOptions = GetSelectListOptions();
            HasLiquidationOptions = GetSelectListOptions();
        }

        public string DamageLossDetails { get; set; }
        public string WithdrawnDetails { get; set; }
        public string RefusedDetails { get; set; }
        public string StatutoryOffenceDetails { get; set; }
        public string LiquidationDetails { get; set; }
        public IList<SelectListItem> HasDamageLossOptions { get; set; }
        public IList<SelectListItem> HasWithdrawnOptions { get; set; }
        public IList<SelectListItem> HasRefusedOptions { get; set; }
        public IList<SelectListItem> HasStatutoryOffenceOptions { get; set; }
        public IList<SelectListItem> HasLiquidationOptions { get; set; }
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
    }
    public class SLViewModel
    {
        public SLViewModel()
        {
            HasSLOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
            HasAMLCFTExtensionOptions = GetSelectListOptions();
            HasManageAMLOptions = GetSelectListOptions();
            HasReportingEntityOptions = GetSelectListOptions();
            HasTrainingOptions = GetSelectListOptions();
        }               
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        public IList<SelectListItem> HasSLOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }
        public IList<SelectListItem> HasAMLCFTExtensionOptions { get; set; }
        public IList<SelectListItem> HasManageAMLOptions { get; set; }
        public IList<SelectListItem> HasReportingEntityOptions { get; set; }
        public IList<SelectListItem> HasTrainingOptions { get; set; }

        public int CoverAmount { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }
    }
    public class ELViewModel
    {
        public ELViewModel()
        {
            HasELOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
        }                
        
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        public IList<SelectListItem> HasELOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }     
        public int CoverAmount { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }

    }
    public class EPLViewModel
    {
        public EPLViewModel()
        {
            HasEPLOptions = GetSelectListOptions();
            CoveredOptions = GetSelectListOptions();
            LegalAdvisorOptions = GetSelectListOptions();
            CasualBasisOptions = GetSelectListOptions();
            DefinedOptions = GetSelectListOptions();
            ManualOptions = GetSelectListOptions();
            PostingNoticesOptions = GetSelectListOptions();
            StaffRedundancyOptions = GetSelectListOptions();
            HasEPLIOptions = GetSelectListOptions();
            IsInsuredClaimOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
            HaveAnyEmployeeYN = GetSelectListOptions();
            IssuingFundtransferYN = GetSelectListOptions();
            ReconcilingStatementsYN = GetSelectListOptions();
            AmendingFundsYN = GetSelectListOptions();

        }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }    
        public IList<SelectListItem> HasEPLOptions { get; set; }
        public IList<SelectListItem> HasEPLIOptions { get; set; }
        public IList<SelectListItem> CoveredOptions { get; set; }
        public IList<SelectListItem> LegalAdvisorOptions { get; set; }
        public IList<SelectListItem> CasualBasisOptions { get; set; }
        public IList<SelectListItem> DefinedOptions { get; set; }
        public IList<SelectListItem> ManualOptions { get; set; }
        public IList<SelectListItem> PostingNoticesOptions { get; set; }
        public IList<SelectListItem> StaffRedundancyOptions { get; set; }
        public IList<SelectListItem> IsInsuredClaimOptions { get; set; }
        public IList<SelectListItem> HaveAnyEmployeeYN { get; set; }
        public IList<SelectListItem> IssuingFundtransferYN { get; set; }
        public IList<SelectListItem> ReconcilingStatementsYN { get; set; }
        public IList<SelectListItem> AmendingFundsYN { get; set; }

        public int CoverAmount { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }
        public int TotalEmployees { get; set; }
        public string InsuredClaimDetails { get; set; }
    }
    public class CLIViewModel
    {
        public CLIViewModel()
        {
            HasCLIOptions = GetSelectListOptions1();
            HasSecurityOptions = GetSelectListOptions();
            HasAccessControlOptions = GetSelectListOptions();
            HasProhibitAccessOptions = GetSelectListOptions();
            HasBackupOptions = GetSelectListOptions();
            HasDomiciledOperationOptions = GetSelectListOptions();
            HasActivityOptions = GetSelectListOptions();
            HasConfidencialOptions = GetSelectListOptions();
            HasBreachesOptions = GetSelectListOptions();
            HasKnowledgeOptions = GetSelectListOptions();
            HasOptionalCLEOptions = GetSelectListOptions1();
            HasProceduresOptions = GetSelectListOptions();
            HasApprovedVendorsOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptionsCyberRenew();
            HasLocationOptions = GetSelectListOptions();
            HasOptionalUltraOptions = GetSelectListOptions();
        }
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "True", Value = "1"
                },
                new SelectListItem
                { Text = "False", Value = "2" }
            };
        }
        private IList<SelectListItem> GetSelectListOptions1()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        private IList<SelectListItem> GetSelectListOptionsCyberRenew()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" },
                new SelectListItem
                { Text = "Never had Cyber", Value = "3" }
            };
        }
        public IList<SelectListItem> HasCLIOptions { get; set; }
        public IList<SelectListItem> HasSecurityOptions { get; set; }
        public IList<SelectListItem> HasAccessControlOptions { get; set; }
        public IList<SelectListItem> HasProhibitAccessOptions { get; set; }
        public IList<SelectListItem> HasBackupOptions { get; set; }
        public IList<SelectListItem> HasDomiciledOperationOptions { get; set; }
        public IList<SelectListItem> HasActivityOptions { get; set; }
        public IList<SelectListItem> HasConfidencialOptions { get; set; }
        public IList<SelectListItem> HasBreachesOptions { get; set; }
        public IList<SelectListItem> HasKnowledgeOptions { get; set; }
        public IList<SelectListItem> HasOptionalCLEOptions { get; set; }
        public IList<SelectListItem> HasProceduresOptions { get; set; }
        public IList<SelectListItem> HasApprovedVendorsOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }
        public IList<SelectListItem> HasOptionalUltraOptions { get; set; }

        public IList<SelectListItem> HasLocationOptions { get; set; }

        public int CoverAmount { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }        
    }
    public class FAPViewModel
    {
        public FAPViewModel()
        {
            HasTraditionalLicenceOptions = GetSelectListOptions();
            HasAdvisersOptions = GetHasAdvisersOptions();
            HasApolloAdvisersOptions = GetHasApolloAdvisersOptions();
            HasAdditionalTraditionalLicenceOptions = GetAdditionalTraditionalLicenceSelectListOptions();
            CoverStartDate = "15/03/2021";
        }

        public string TransitionalLicenseNum { get; set; }
        public string TransitionalLicenseHolder { get; set; }
        public string CoverStartDate { get; set; }
        private IList<SelectListItem> GetHasAdvisersOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "I do not have any other advisers working under my license", Value = "1"
                },
                new SelectListItem
                {
                    Text = "I do have other advisers working under my license", Value = "2"
                }
            };
        }

        private IList<SelectListItem> GetHasApolloAdvisersOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "I do not have any other advisers working under my licence", Value = "1"
                },
                new SelectListItem
                {
                    Text = "I do have other Apollo advisers only working under my licence", Value = "2"
                },
                new SelectListItem
                {
                    Text = "I do have other Non-Apollo advisers working under my licence", Value = "3"
                }
            };
        }
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        private IList<SelectListItem> GetAdditionalTraditionalLicenceSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "I am taking my own Transitional Licence with no other advisers working under my license", Value = "1"
                },
                new SelectListItem
                {
                    Text = "I will be coming under someone else's Transitional Licence", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Undecided", Value = "3"
                }
            };
        }
        public IList<SelectListItem> HasAdvisersOptions { get; set; }
        public IList<SelectListItem> HasApolloAdvisersOptions { get; set; }
        public IList<SelectListItem> HasTraditionalLicenceOptions { get; set; }
        public IList<SelectListItem> HasAdditionalTraditionalLicenceOptions { get; set; }

    }
    public class PIViewModel
    {
        public PIViewModel()
        {
            HasCEASMembershipOptions = GetCEASMembershipSelectListOptions();
            HasIndustrialSoftwareOptions = GetSelectListOptions();
            HasEngageSoftwareOptions = GetSelectListOptions();
            HasSignificantShareholderOptions = GetSelectListOptions();
            HasEngageConsortium = GetSelectListOptions();
            HasAssociatedPracticeOptions = GetSelectListOptions();
            ContractingServicesOptions = GetContractingServicesOptions();
            HasStandardTermsOptions = GetSometimesSelectListOptions();
            HasNegotiateOptions = GetSometimesSelectListOptions();
            HasNoAgreementOptions = GetSometimesSelectListOptions();
            HasOwnPIOptions = GetSometimesSelectListOptions();
            HasBoundContractOptions = GetSometimesSelectListOptions();
            HasEngagementLetterOptions = GetSometimesSelectListOptions();
            HasRecordedOptions = GetSometimesSelectListOptions();
            HasDiaryRecordOptions = GetSometimesSelectListOptions();
            HasComplaintOptions = GetSometimesSelectListOptions();
            HasEngageOptions = GetSelectListOptions(); 
            HasDisciplinaryOptions = GetSelectListOptions();
            HasClaimsAgainstOptions = GetSelectListOptions();
            HasResponsibleOptions = GetSelectListOptions();
            HasClaimsAgainstOptions2 = GetSelectListOptions();
            HasRefundOptions = GetSelectListOptions();
            HasSuedOptions = GetSelectListOptions();
            HasDisputeOptions = GetSelectListOptions();
            HasDisputeOptions2 = GetSelectListOptions();
            HasPenaltyOptions = GetSelectListOptions();
            HasManagedProjectOptions = GetSelectListOptions();
            HasIncludedDesignOptions = GetSelectListOptions();
            HasEngineerOptions = GetSelectListOptions();
            HasAluminium = GetSelectListOptions();
            HasPracticeClaimOptions = GetSelectListOptions();
            HasThirdPartyOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
            HasDANZOptions = GetSelectListOptions();
            HasComplaintAlternativeOptions = GetAlternativeSelectListOptions();
            HasSalesRelateOptions = GetSelectListOptions();
            HasSubstantialChangeOptions = GetSelectListOptions();
            IsFormInPracticeOptions = GetSelectListOptions();
            HasStandardContractFormOptions = GetSelectListOptions();
            HasAnyOtherFormOptions = GetSelectListOptions();
            HasPersonnelDismissedOptions = GetSelectListOptions();
            HasReferencesObtainedOptions = GetSelectListOptions();
            HasLeakyBuildingCoverOptions = GetSelectListOptions();
            HasRiskManagementOptions = GetRiskManagementOptions();
            HasRetainedDocumentOptions = GetAlternativeSelectListOptions();
            HasCircumstanceAriseOptions = GetAlternativeSelectListOptions();
            HasChchRebuildOptions = GetSelectListOptions();
            HasConsultantantsResponsibilityOptions = GetSelectListOptions();
            HasComputerSoftwareOptions = GetSelectListOptions();
            HasJurisdictionOptions = GetSelectListOptions();
            HasDishonestyOptions = GetSelectListOptions();
            HasEmployeeFidelityOptions = GetSelectListOptions();
            HasLossDocumentsOptions = GetSelectListOptions();
            HasLegalCouncelOptions = GetSelectListOptions();
            HasFormalProceduresOptions = GetSelectListOptions();
            HasMortageOptions = GetSelectListOptions();
            HasBusinessChangesOptions = GetSelectListOptions();
            HasotherBusinessActivity = GetSelectListOptions();
            HasStandardTermsServicesOptions = GetSelectListOptions();
            HasHarmProvOptions = GetSelectListOptions();
            HasIndependentsPIOptions = GetSelectListOptions();
            HasIndependentsBoundOptions = GetSelectListOptions();
            HasMedicalServicesOptions = GetSelectListOptions();
            HasNavigationIndustryOptions = GetSelectListOptions();
            HasNetworkSecurityOptions = GetSelectListOptions();
            HasTradingIndustryOptions = GetSelectListOptions();
            HasPartyMembers = GetPartyMembersOptions();
            HasOtherPIinsurances = GetSelectListOptions();
            HasClassOfLicense = GetClassOfLicenseOptions();
            TrustAdvisorStatus = GetSelectListOptions();
            HaveAnyRunOffinsurance = GetSelectListOptions();
            YearCover = GetRunOffYearCoverSelectListOptions();
            HasInvActivity = GetSelectListOptions();
            YearCover1 = GetRunOffYearCoverSelectListOptions1();

        }

        private IList<SelectListItem> GetRunOffYearCoverSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "1 year", Value = "1"
                },
                new SelectListItem
                {
                    Text = "2 years", Value = "2"
                },
                new SelectListItem
                {
                    Text = "3 years", Value = "3"
                },
                new SelectListItem
                {
                    Text = "4 years", Value = "4"
                },
                new SelectListItem
                {
                    Text = "5 years", Value = "5"
                },
                new SelectListItem
                {
                    Text = "6 years", Value = "6"
                },
                new SelectListItem
                {
                    Text = "7 years", Value = "7"
                }

            };
        }

        private IList<SelectListItem> GetRunOffYearCoverSelectListOptions1()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "1 year", Value = "1"
                },
                new SelectListItem
                {
                    Text = "2 years", Value = "2"
                },
                new SelectListItem
                {
                    Text = "3 years", Value = "3"
                },
                new SelectListItem
                {
                    Text = "4 years", Value = "4"
                },
                new SelectListItem
                {
                    Text = "5 years", Value = "5"
                },
                new SelectListItem
                {
                    Text = "6 years", Value = "6"
                }

            };
        }

        private IList<SelectListItem> GetCEASMembershipSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "AGM Attendance Only", Value = "1"
                },
                new SelectListItem
                {
                    Text = "Standard Insurance Programme (SIP2)", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Incl. options on SIP1 for > deductibles (SIP2)", Value = "3"
                },
                new SelectListItem
                {
                    Text = "Incl. options on SIP2 for run-off (SIP3)", Value = "4"
                }
            };
        }

        private IList<SelectListItem> GetContractingServicesOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Network security", Value = "1"
                },
                new SelectListItem
                {
                    Text = "On-line stock trading", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Funds management / investment and financial advisingy", Value = "3"
                },
                new SelectListItem
                {
                    Text = "Manufacturing control processes", Value = "4"
                },
                new SelectListItem
                {
                    Text = "Oil & gas", Value = "5"
                },
                new SelectListItem
                {
                    Text = "Mining", Value = "6"
                },
                new SelectListItem
                {
                    Text = "Medical", Value = "7"
                },
                new SelectListItem
                {
                    Text = "Defence", Value = "8"
                },
                new SelectListItem
                {
                    Text = "None of the above", Value = "10"
                },
            };
        }
        private IList<SelectListItem> GetRiskManagementOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Always obtain clients instructions in writing ", Value = "1"
                },
                new SelectListItem
                {
                    Text = "If applicable, check rates notice to ensure the client owns the land", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Set out a budget and actively review it with your client as may be required ", Value = "3"
                },
                new SelectListItem
                {
                    Text = "Review contract conditions to ensure requirements are within your professional indemnity insurance conditions or policy limits", Value = "4"
                },
                new SelectListItem
                {
                    Text = "Have your client sign off each page of the contract documents", Value = "5"
                },
                new SelectListItem
                {
                    Text = "None of the above", Value = "10"
                },
            };
        }
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }

        private IList<SelectListItem> GetPartyMembersOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Steadfast", Value = "1"
                },
                new SelectListItem
                {
                    Text = "Advisor Net", Value = "2"
                }, 
                new SelectListItem
                {
                    Text = "NZ Broker", Value = "3"
                },
                 new SelectListItem
                {
                    Text = "2 or More of the Above", Value = "4"
                },
                 new SelectListItem
                {
                    Text = "None of the Above", Value = "5"
                }
            };
        }

        private IList<SelectListItem> GetClassOfLicenseOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Class 1 - Single Advisor Only", Value = "1"
                },
                new SelectListItem
                {
                    Text = " Class 2 - More than one advisor", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Class 3 - Any others", Value = "3"
                }
            };
        }

        private IList<SelectListItem> GetSometimesSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { 
                    Text = "No", Value = "2" 
                },
                new SelectListItem
                {
                    Text = "Sometimes", Value = "3"
                }
            };
        }
        private IList<SelectListItem> GetAlternativeSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes for all of the above", Value = "1"
                },
                new SelectListItem
                {
                    Text = "No", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Don't know", Value = "3"
                }
            };
        }
        public IList<SelectListItem> HasCEASMembershipOptions { get; set; }        
        public IList<SelectListItem> ContractingServicesOptions { get; set; }
        public IList<SelectListItem> HasMortageOptions { get; set; }
        public IList<SelectListItem> HasStandardTermsOptions { get; set; }
        public IList<SelectListItem> HasNegotiateOptions { get; set; }
        public IList<SelectListItem> HasNoAgreementOptions { get; set; }
        public IList<SelectListItem> HasOwnPIOptions { get; set; }
        public IList<SelectListItem> HasBoundContractOptions { get; set; }
        public IList<SelectListItem> HasEngagementLetterOptions { get; set; }
        public IList<SelectListItem> HasRecordedOptions { get; set; }
        public IList<SelectListItem> HasDiaryRecordOptions { get; set; }
        public IList<SelectListItem> HasComplaintOptions { get; set; }
        public IList<SelectListItem> HasEngageOptions { get; set; }
        public IList<SelectListItem> HasJurisdictionOptions { get; set; }
        public IList<SelectListItem> HasDishonestyOptions { get; set; }
        public IList<SelectListItem> HasEmployeeFidelityOptions { get; set; }
        public IList<SelectListItem> HasLossDocumentsOptions { get; set; }
        public IList<SelectListItem> HasLegalCouncelOptions { get; set; }        
        public IList<SelectListItem> HasDisciplinaryOptions { get; set; }
        public IList<SelectListItem> HasClaimsAgainstOptions { get; set; }
        public IList<SelectListItem> HasClaimsAgainstOptions2 { get; set; }
        public IList<SelectListItem> HasResponsibleOptions { get; set; }
        public IList<SelectListItem> HasRefundOptions { get; set; }
        public IList<SelectListItem> HasSuedOptions { get; set; }
        public IList<SelectListItem> HasDisputeOptions { get; set; }
        public IList<SelectListItem> HasDisputeOptions2 { get; set; }        
        public IList<SelectListItem> HasPenaltyOptions { get; set; }
        public IList<SelectListItem> HasManagedProjectOptions { get; set; }
        public IList<SelectListItem> HasIncludedDesignOptions { get; set; }
        public IList<SelectListItem> HasEngineerOptions { get; set; }
        public IList<SelectListItem> HasAluminium { get; set; }
        public IList<SelectListItem> HasPracticeClaimOptions { get; set; }
        public IList<SelectListItem> HasThirdPartyOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }
        public IList<SelectListItem> HasDANZOptions { get; set; }
        public IList<SelectListItem> HasSalesRelateOptions { get; set; }
        public IList<SelectListItem> HasSubstantialChangeOptions { get; set; }
        public IList<SelectListItem> HasStandardContractFormOptions { get; set; }
        public IList<SelectListItem> IsFormInPracticeOptions { get; set; }
        public IList<SelectListItem> HasAnyOtherFormOptions { get; set; }
        public IList<SelectListItem> HasPersonnelDismissedOptions { get; set; }
        public IList<SelectListItem> HasReferencesObtainedOptions { get; set; }
        public IList<SelectListItem> HasLeakyBuildingCoverOptions { get; set; }
        public IList<SelectListItem> HasRiskManagementOptions { get; set; }
        public IList<SelectListItem> HasRetainedDocumentOptions { get; set; }
        public IList<SelectListItem> HasComplaintAlternativeOptions { get; set; }
        public IList<SelectListItem> HasCircumstanceAriseOptions { get; set; }
        public IList<SelectListItem> HasChchRebuildOptions { get; set; }
        public IList<SelectListItem> HasConsultantantsResponsibilityOptions { get; set; }
        public IList<SelectListItem> HasAssociatedPracticeOptions { get; set; }
        public IList<SelectListItem> HasEngageConsortium { get; set; }
        public IList<SelectListItem> HasSignificantShareholderOptions { get; set; }
        public IList<SelectListItem> HasEngageSoftwareOptions { get; set; }
        public IList<SelectListItem> HasIndustrialSoftwareOptions { get; set; }
        public IList<SelectListItem> HasComputerSoftwareOptions { get; set; }
        public IList<SelectListItem> HasFormalProceduresOptions { get; set; }
        public IList<SelectListItem> HasBusinessChangesOptions { get; set; }
        public IList<SelectListItem> HasotherBusinessActivity { get; set; }
        public IList<SelectListItem> HasStandardTermsServicesOptions { get; set; }
        public IList<SelectListItem> HasHarmProvOptions { get; set; }
        public IList<SelectListItem> HasIndependentsPIOptions { get; set; }
        public IList<SelectListItem> HasIndependentsBoundOptions { get; set; }
        public IList<SelectListItem> HasMedicalServicesOptions { get; set; }
        public IList<SelectListItem> HasNavigationIndustryOptions { get; set; }
        public IList<SelectListItem> HasNetworkSecurityOptions { get; set; }
        public IList<SelectListItem> HasTradingIndustryOptions { get; set; }
        public IList<SelectListItem> HasPartyMembers { get; set; }
        public IList<SelectListItem> HasOtherPIinsurances { get; set; }
        public IList<SelectListItem> HasClassOfLicense { get; set; }
        public IList<SelectListItem> TrustAdvisorStatus { get; set; }
        public IList<SelectListItem> HaveAnyRunOffinsurance { get; set; }
        public IList<SelectListItem> YearCover { get; set; }
        public IList<SelectListItem> YearCover1 { get; set; }
        public IList<SelectListItem> HasInvActivity { get; set; }

        public string ProcedureManagedDetails { get; set; }
        public string BusinessChangesDetails { get; set; }        
        public string LegalCouncelDetails { get; set; }
        public string ComputerSoftwareActivityDetails { get; set; }
        public int DeductableAmount { get; set; }
        public string IndustrialSoftwareDetails { get; set; }
        public string EngagementSoftwareDetails { get; set; }
        public string EngageConsortiumDetails { get; set; }
        public string SignificantShareholderDetails { get; set; }        
        public string AssociatedPracticeDetails { get; set; }
        public string MortageDetails { get; set; }
        
        public string EngageDetails { get; set; }
        public string AluminiumDetails { get; set; }
        public string DisciplinaryDetails { get; set; }
        public string ClaimDetails { get; set; }
        public string ClaimDetails2 { get; set; }
        public string ResponsibleDetails { get; set; }
        public string RefundDetails { get; set; }
        public string SuedDetails { get; set; }
        public string DisputeDetails { get; set; }
        public string PenaltyDetails { get; set; }
        public string ManagedProjectDetails { get; set; }
        public string IncludedDesignDetails { get; set; }
        public string EngineerDetails { get; set; }
        public string ContractingServicesDetails { get; set; }
        public string MedicalServicesDetails { get; set; }
        public string NavigationIndustryDetails { get; set; }
        public string NetworkSecurityDetails { get; set; }
        public string TradingIndustryDetails { get; set; }
        public int CoverAmount { get; set; }
        public int PercentFees { get; set; }
        public decimal RevenueInsuredContractors { get; set; }
        public string ContrctAgreeHarmProvDetails { get; set; }
        public string PercentDetails { get; set; }
        public string PersonnelDismisedDetails { get; set; }
        public string FormInPracticeDetails { get; set; }
        public string UseInCircumstancesDetails { get; set; }
        public string ChchRebuildDetails { get; set; }        
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }
        public string SubstantialChangeDetails { get; set; }
        public string hasdirectagencies { get; set; }
        public string hasfsdr { get; set; }
    }
    public class DAOLIViewModel
    {
        public DAOLIViewModel()
        {
            HasDAOLIOptions = GetSelectListOptions();
            HasClaimOptions = GetSelectListOptions();
            HasCircumstanceOptions = GetSelectListOptions();
            HasInvestigationOptions = GetSelectListOptions();
            HasDeclinedOptions = GetSelectListOptions();
            HasReceivershipOptions = GetSelectListOptions();
            HasCriminalOptions = GetSelectListOptions();
            HasProcecutionOptions = GetSelectListOptions();
            HasObligationOptions = GetSelectListOptions();
            HasDebtsOptions = GetSelectListOptions();
            HasOtherDAOOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
            FormDate = DateTime.Now;
        }

        public IList<SelectListItem> HasDAOLIOptions { get; set; }
        public IList<SelectListItem> HasClaimOptions { get; set; }
        public IList<SelectListItem> HasCircumstanceOptions { get; set; }
        public IList<SelectListItem> HasInvestigationOptions { get; set; }
        public IList<SelectListItem> HasDeclinedOptions { get; set; }
        public IList<SelectListItem> HasReceivershipOptions { get; set; }
        public IList<SelectListItem> HasCriminalOptions { get; set; }
        public IList<SelectListItem> HasProcecutionOptions { get; set; }
        public IList<SelectListItem> HasObligationOptions { get; set; }
        public IList<SelectListItem> HasDebtsOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }
        public IList<SelectListItem> HasOtherDAOOptions { get; set; }

        

        public int ShareholderTotal { get; set; }
        public int AssetTotal { get; set; }
        public int LiabilityTotal { get; set; }
        public int AssetCurrent { get; set; }
        public int LiabilityCurrent{ get; set; }
        public int AfterTaxNumber { get; set; }
        public int DebtTotal { get; set; }
        public DateTime FormDate { get; set; }
        public string CompanyNameDetails { get; set; }
        public string CircumstanceDetails { get; set; }
        public string InvestigationDetails { get; set; }
        public string DeclinedDetails { get; set; }
        public string ReceivershipDetails { get; set; }
        public string CriminalDetails { get; set; }
        public string ProcecutionDetails { get; set; }
        public string ObligationDetails { get; set; }
        public int CoverAmount { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }      
    }

    public class OTViewModel
    {
        public OTViewModel()
        {
            HasOutsideTrusteesOptions = GetSelectListOptions();
            HasClaimQuestionsOptions = GetSelectListOptions();
            HasInvolvedTrusteeOptions = GetSelectListOptions();
            HasLessYearsOptions = GetSelectListOptions();
            HasFinancialObligationsOptions = GetSelectListOptions();
            FormDate = DateTime.Now;
        }

        public IList<SelectListItem> HasOutsideTrusteesOptions { get; set; }
        public IList<SelectListItem> HasClaimQuestionsOptions { get; set; }
        public IList<SelectListItem> HasInvolvedTrusteeOptions { get; set; }
        public IList<SelectListItem> HasLessYearsOptions { get; set; }
        public IList<SelectListItem> HasFinancialObligationsOptions { get; set; }
        public DateTime FormDate { get; set; }
        public string CompanyName { get; set; }
        public string OutsidePositions { get; set; }
        public string ClaimDetails { get; set; }
        public string InvolvedTrusteeDetails { get; set; }
        public string FinancialObligationsDetails { get; set; }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }


    }
    public class GLViewModel
    {
        public GLViewModel()
        {
            HasGLOptions = GetSelectListOptions();
            HasHigherGLOptions = GetSelectListOptions();
            HasExistingPolicyOptions = GetSelectListOptions();
            HasAssumeLiabilityOptions = GetSelectListOptions();
            HasClientFundsOptions = GetSelectListOptions();
        }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }
        public IList<SelectListItem> HasGLOptions { get; set; }
        public IList<SelectListItem> HasHigherGLOptions { get; set; }
        public IList<SelectListItem> HasExistingPolicyOptions { get; set; }
        public IList<SelectListItem> HasAssumeLiabilityOptions { get; set; }
        public IList<SelectListItem> HasClientFundsOptions { get; set; }
        public int CoverAmount { get; set; }
        public string VehicleDetails { get; set; }
        public string AssumeLiabilityDetails { get; set; }
        public string CarParkDetails { get; set; }
        public string EquipmentDetails { get; set; }
        public string DateLapsed { get; set; }
        public string RetroactiveDate { get; set; }
        public string InsurerName { get; set; }

    }
    public class ProjectViewModel
    {
        public ProjectViewModel(ClientInformationSheet clientInformationSheet)
        {
            BusinessContracts = GetBusinessContracts(clientInformationSheet);
            Territories = GetTerritories(clientInformationSheet);
            ContractTypeOptions = GetContractType();
            ResponsibilityOptions = GetResponsibilityOptions();
        }

        private IList<SelectListItem> GetTerritories(ClientInformationSheet clientInformationSheet)
        {
            Territories = new List<SelectListItem>();
            foreach(var territory in clientInformationSheet.Programme.BaseProgramme.TerritoryTemplates)
            {
                Territories.Add(new SelectListItem
                {
                    Text = territory.Location,
                    Value = territory.Location
                });
            }
            return Territories;
        }

        private IList<BusinessContract> GetBusinessContracts(ClientInformationSheet clientInformationSheet)
        {
            BusinessContracts = new List<BusinessContract>();
            foreach (var businessContract in clientInformationSheet.BusinessContracts)
            {
                BusinessContracts.Add(businessContract);
            }
            return BusinessContracts;
        }

        private IList<SelectListItem> GetContractType()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Residential", Value = "Residential"
                },
                new SelectListItem
                {
                    Text = "Commercial", Value = "Commercial"
                },
                new SelectListItem
                { 
                    Text = "New", Value = "New"
                },
                new SelectListItem
                {
                    Text = "Alteration", Value = "Alteration"
                },
                new SelectListItem
                {
                    Text = "Other", Value = "Other"
                }
            };
        }
        private IList<SelectListItem> GetResponsibilityOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "Proj. Director", Value = "ProjectViewModel.ProjectDirector"
                },
                new SelectListItem
                {
                    Text = "Proj. Manager", Value = "ProjectViewModel.ProjectManager"
                },
                new SelectListItem
                {
                    Text = "Proj. Coordinator/Administrator", Value = "ProjectViewModel.ProjectCoordinator"
                },
                new SelectListItem
                {
                    Text = "Proj. Engineers", Value = "ProjectViewModel.ProjectEngineer"
                }
            };
        }
        public IList<BusinessContract> BusinessContracts { get; set; }
        public IList<SelectListItem> Territories { get; set; }
        public IList<SelectListItem> ContractTypeOptions { get; set; }
        public IList<SelectListItem> ResponsibilityOptions { get; set; }
        public string MajorResponsibilities { get; set; }
        public string ProjectDescription { get; set; }
        public string Fees { get; set; }
        public string Year { get; set; }
        public string ContractTitle { get; set; }        
        public string ConstructionValue { get; set; }
        public string ProjectDuration { get; set; }
        
    }
    public class ResearchHouseViewModel
    {
        public ResearchHouseViewModel(ClientInformationSheet clientInformationSheet)
        {
            ResearchHouses = GetAAABusinessContracts(clientInformationSheet);
        }

      
        private IList<ResearchHouse> GetAAABusinessContracts(ClientInformationSheet clientInformationSheet)
        {
            ResearchHouses = new List<ResearchHouse>();
            foreach (var researchHouses in clientInformationSheet.ResearchHouses)
            {
                ResearchHouses.Add(researchHouses);
            }
            return ResearchHouses;
        }
       
        public IList<ResearchHouse> ResearchHouses { get; set; }
        //public string MajorResponsibilities { get; set; }
        public string Services { get; set; }
        //public string Fees { get; set; }
        public string Name { get; set; }
        public string ContractTitle { get; set; }
        public string ConstructionValue { get; set; }
        public string ProjectDuration { get; set; }

    }
    public class IPViewModel
    {
        public IPViewModel()
        {

            HasClientFundsOptions = GetSelectListOptions();
        }

        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Yes", Value = "1"
                },
                new SelectListItem
                { Text = "No", Value = "2" }
            };
        }

        public IList<SelectListItem> HasClientFundsOptions { get; set; }
        public string totalValue { get; set; }
        public string CashInvestments { get; set; }
        public string Bonds { get; set; }
        public string DirectInvestment { get; set; }
        public string PropertyTrust { get; set; }
        public string FinanceDebentures { get; set; }
        public string ManagedFunds { get; set; }
        public string Superannuation { get; set; }
        public string Kiwisaver { get; set; }
        public string OtherFunds { get; set; }
        public string OtherFundsAdditional { get; set; }
    }

    public class GeneralViewModel
    {
        public GeneralViewModel()
        {
            if(PolicyDate == DateTime.MinValue)
            {
                PolicyDate = DateTime.Now;
            }
            if (PolicyStartDate == DateTime.MinValue)
            {
                PolicyStartDate = DateTime.Now;
            }
            if (PolicyEndDate == DateTime.MinValue)
            {
                PolicyEndDate = DateTime.Now;
            }
        }
        [Display(Name = "Policy Date")]
        public DateTime PolicyDate { get; set; }

        [Display(Name = "Policy Start Date")]
        public DateTime PolicyStartDate { get; set; }

        [Display(Name = "Policy End Date")]
        public DateTime PolicyEndDate { get; set; }

    }
}
