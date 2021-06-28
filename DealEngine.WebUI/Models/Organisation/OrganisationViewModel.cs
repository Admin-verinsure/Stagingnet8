using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Linq;

namespace DealEngine.WebUI.Models
{
    public class OrganisationViewModel : BaseViewModel
    {
        public OrganisationViewModel() { }
        public OrganisationViewModel(ClientInformationSheet ClientInformationSheet, User OrgUser)
        {
            User = new User(null, Guid.NewGuid());
            Organisations = new List<Domain.Entities.Organisation>();
            PublicOrganisations = new List<Domain.Entities.Organisation>();
            OrganisationTypes = GetOrganisationTypes();
            if (ClientInformationSheet != null)
            {
                Programme = ClientInformationSheet.Programme.BaseProgramme;
                if(Programme.NamedPartyUnitName == "NZFSG Programme" || Programme.NamedPartyUnitName == "TripleA Programme" || Programme.NamedPartyUnitName == "Apollo Programme" || 
                    Programme.NamedPartyUnitName == "Abbott Financial Advisor Liability Programme" || 
                    Programme.NamedPartyUnitName == "Financial Advice NZ Financial Advice Provider Liability Programme")
                {
                    AdvisorUnit = new AdvisorUnit(null, null, null, null);//organisation.FirstOrDefault(o=>o.OrganisationalUnits.Any(o=>o.Type == "Advisor"));
                    AdministratorUnit = new AdministratorUnit(null, null, null, null);//organisation.FirstOrDefault(o=>o.OrganisationalUnits.Any(o=>o.Type == "Advisor"));

                    if (Programme.NamedPartyUnitName == "NZFSG Programme") 
                    { 
                        InsuranceAttributes = GetAdvisorTypes1(); 
                    } 
                    else if(Programme.NamedPartyUnitName == "TripleA Programme")
                    {
                        InsuranceAttributes = GetAdvisorTypes2();
                    }
                    else if (Programme.NamedPartyUnitName == "Apollo Programme" || Programme.NamedPartyUnitName == "Abbott Financial Advisor Liability Programme" || 
                        Programme.NamedPartyUnitName == "Financial Advice NZ Financial Advice Provider Liability Programme")
                    {
                        InsuranceAttributes = GetAdvisorTypes3();
                        if(Programme.NamedPartyUnitName == "Apollo Programme") 
                        { 
                            HasAssociationOptions = GetAssociationOptions1(); 
                        } else if (Programme.NamedPartyUnitName == "Abbott Financial Advisor Liability Programme")
                        { 
                            HasAssociationOptions = GetAssociationOptions2(); 
                        } else
                        {
                            HasAssociationOptions = GetAssociationOptions3();
                        }
                    }
                    
                    HasRetiredorDeceasedOptions = GetStandardSelectOptions();
                    HasRegisteredOptions = GetHasRegisteredOptions();
                    OrganisationTypes = GetOrganisationTypes();
                    HasPrincipalOptions = GetBooleanSelectOptions();
                    HasIsTripleAApprovalOptions = GetBooleanSelectOptions();
                    HasIsFAPOptions = GetBooleanSelectOptions();
                    HasInterposedPersonOptions = GetBooleanSelectOptions();



                }
                if (Programme.NamedPartyUnitName == "DANZ Programme" || Programme.NamedPartyUnitName == "PMINZ Programme")
                {
                    InsuranceAttributes = GetPersonnelTypes();
                    PersonnelUnit = new PersonnelUnit(null, null, null, null); //(PersonnelUnit)organisation.OrganisationalUnits.FirstOrDefault(o => o.Type == "Personnel");
                    InsuredEntityRelationOptions = GetInsuredEntityRelationOptions();
                    HasRegisteredLicensedOptions = GetBooleanSelectOptions();
                    HasDesignLicencedOptions = GetLicencedOptions();
                    HasSiteLicensedOptions = GetLicencedOptions();
                    HasCurrentMembershipOptions = GetBooleanSelectOptions();
                    InsuredEntityRelationOptions = GetInsuredEntityRelationOptions();
                    HasContractorInsuredOptions = GetBooleanSelectOptions();
                    HasInsuredRequiredOptions = GetStandardSelectOptions();
                    CertTypes = GetCertTypes();
                    HasMajorShareHolder = GetBooleanSelectOptions();
                }

                if (Programme.NamedPartyUnitName == "NZFSG ML Programme" || Programme.NamedPartyUnitName == "Financial Advice NZ Financial Advice Provider Liability ML Programme")
                {
                    DirectorUnit = new DirectorUnit(null, null, null, null);//organisation.FirstOrDefault(o=>o.OrganisationalUnits.Any(o=>o.Type == "Advisor"));

                    InsuranceAttributes = GetAdvisorTypes4();
                    HasRetiredorDeceasedOptions = GetStandardSelectOptions();
                    OrganisationTypes = GetOrganisationTypes();

                }
                if (Programme.NamedPartyUnitName == "Technology Contractors Liability Programme")
                {
                    InsuranceAttributes = GetIndividualInsured();
                    IndividualInsuredUnit = new IndividualInsuredUnit(null, null, null, null);
                }
                if (Programme.NamedPartyUnitName == "CEAS Programme" || Programme.NamedPartyUnitName == "NZACS Programme")
                {
                    InsuranceAttributes = GetPrincipalTypes();
                    PrincipalUnit = new PrincipalUnit(null, null, null, null); //(PrincipalUnit)organisation.OrganisationalUnits.FirstOrDefault(o => o.Type == "Principal");
                    HasRetiredorDeceasedOptions = GetStandardSelectOptions();
                    HasIsIPENZmemberOptions = GetStandardSelectOptions();
                    HasCPEngQualifiedOptions = GetStandardSelectOptions();
                    HasIsNZIAmemberOptions = GetStandardSelectOptions();
                    HasIsADNZmemberOptions = GetStandardSelectOptions();
                    HasIsOtherdirectorshipOptions = GetStandardSelectOptions();
                }
                
                if (Programme.NamedPartyUnitName == "NZPI Programme")
                {
                    InsuranceAttributes = GetContractorTypes();
                    PlannerUnit = new PlannerUnit(null, null, null, null);
                    HasNZPIAMemberOptions = GetBooleanSelectOptions();
                    HasContractedInsuredOptions = GetBooleanSelectOptions();
                    HasPrincipalOptions = GetBooleanSelectOptions();
                }
                if(Programme.NamedPartyUnitName == "First Mate Cover")
                {
                    InterestedPartyUnit = new InterestedPartyUnit(null, null, null, null);
                    InsuranceAttributes = GetMarshTypes();
                    InterestedPartyOptions = GetInterestedPartyOptions();
                    OwnershipOptions = GetOwnershipOptions();
                }

                Organisation = ClientInformationSheet.Owner;
                //if (Organisations.Any(o => o.Id != (ClientInformationSheet.Owner.Id)))
                Organisations.Add(ClientInformationSheet.Owner);
                foreach(var sheetOrg in ClientInformationSheet.Organisation)
                {
                    if(Organisations.FirstOrDefault(o=>o.Id == sheetOrg.Id) == null)
                        Organisations.Add(sheetOrg);                                        
                }
                
            }
            if(OrgUser != null)
            {
                User = OrgUser;
            }
        }

        private IList<SelectListItem> GetAssociationOptions3()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "IFA",
                        Value = "IFA"
                    },
                    new SelectListItem
                    {
                        Text = "LBA/TNP",
                        Value = "LBA/TNP"
                    },
                    new SelectListItem
                    {
                        Text = "SIFA",
                        Value = "SIFA"
                    },
                    new SelectListItem
                    {
                        Text = "NZMBA",
                        Value = "NZMBA"
                    },
                    new SelectListItem
                    {
                        Text = "IBANZ",
                        Value = "IBANZ"
                    },
                    new SelectListItem
                    {
                        Text = "CFA",
                        Value = "CFA"
                    },
                    new SelectListItem
                    {
                        Text = "BIG",
                        Value = "BIG"
                    },
                    new SelectListItem
                    {
                        Text = "None of the above",
                        Value = "None of the above"
                    },
                    new SelectListItem
                    {
                        Text = "Other",
                        Value = "Other"
                    }
                };
            return _Types;
        }

        private IList<SelectListItem> GetAssociationOptions2()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "IFA",
                        Value = "IFA"
                    },
                    new SelectListItem
                    {
                        Text = "LBA/TNP",
                        Value = "LBA/TNP"
                    },
                    new SelectListItem
                    {
                        Text = "SIFA",
                        Value = "SIFA"
                    },
                    new SelectListItem
                    {
                        Text = "NZMBA",
                        Value = "NZMBA"
                    },
                    new SelectListItem
                    {
                        Text = "IBANZ",
                        Value = "IBANZ"
                    },
                    new SelectListItem
                    {
                        Text = "CFA",
                        Value = "CFA"
                    },
                    new SelectListItem
                    {
                        Text = "BIG",
                        Value = "BIG"
                    },
                    new SelectListItem
                    {
                        Text = "None of the above - Abbott adviser only",
                        Value = "None of the above - Abbott adviser only"
                    },
                    new SelectListItem
                    {
                        Text = "Other",
                        Value = "Other"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetAssociationOptions1()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "IFA",
                        Value = "IFA"
                    },
                    new SelectListItem
                    {
                        Text = "LBA/TNP",
                        Value = "LBA/TNP"
                    },
                    new SelectListItem
                    {
                        Text = "SIFA",
                        Value = "SIFA"
                    },
                    new SelectListItem
                    {
                        Text = "NZMBA",
                        Value = "NZMBA"
                    },
                    new SelectListItem
                    {
                        Text = "IBANZ",
                        Value = "IBANZ"
                    },
                    new SelectListItem
                    {
                        Text = "CFA",
                        Value = "CFA"
                    },
                    new SelectListItem
                    {
                        Text = "BIG",
                        Value = "BIG"
                    },
                    new SelectListItem
                    {
                        Text = "None of the above - Apollo adviser only",
                        Value = "None of the above - Apollo adviser only"
                    },
                    new SelectListItem
                    {
                        Text = "Other",
                        Value = "Other"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetOwnershipOptions()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Financial",
                        Value = "Financial"
                    },
                    new SelectListItem
                    {
                        Text = "Interested Party",
                        Value = "Interested Party"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetInterestedPartyOptions()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Financial",
                        Value = "Financial"
                    },
                    new SelectListItem
                    {
                        Text = "Interested Party",
                        Value = "Interested Party"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetMarshTypes()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Skipper",
                        Value = "Skipper"
                    },
                    new SelectListItem
                    {
                        Text = "Financial",
                        Value = "Financial"
                    },
                new SelectListItem
                    {
                        Text = "Co Owner",
                        Value = "Co Owner"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetContractorTypes()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Planner",
                        Value = "Planner"
                    },
                    new SelectListItem
                    {
                        Text = "Contractor",
                        Value = "Contractor"
                    }
                };
            return _Types;
        }

        private IList<SelectListItem> GetPrincipalTypes()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Principal",
                        Value = "Principal"
                    },
                    new SelectListItem
                    {
                        Text = "Subsidiary",
                        Value = "Subsidiary"
                    },
                    new SelectListItem
                    {
                        Text = "Previous Consulting Business",
                        Value = "Previous Consulting Business"
                    },
                    new SelectListItem
                    {
                        Text = "Mergers",
                        Value = "Mergers"
                    },
                    new SelectListItem
                    {
                        Text = "Joint Venture",
                        Value = "Joint Venture"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetInsuredEntityRelationOptions()
        {
            var _Types = new List<SelectListItem>()
            {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Director",
                        Value = "Director"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Employee",
                        Value = "Employee"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Contractor",
                        Value = "Contractor"
                    }
            };
            return _Types;
        }
        private List<SelectListItem> GetCertTypes()
        {
            var _Types = new List<SelectListItem>()
            {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Ordinary/Non Member",
                        Value = "Ordinary"
                    }
                ,
                new SelectListItem
                    {
                        Text = "PMP",
                        Value = "PMP"
                    }
                ,
                new SelectListItem
                    {
                        Text = "CAPM",
                        Value = "CAPM"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Project Director",
                        Value = "Project Director"
                    }
            };
            return _Types;
        }
        private IList<SelectListItem> GetPersonnelTypes()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "Personnel",
                        Value = "Personnel"
                    },
                    new SelectListItem
                    {
                        Text = "Subsidiary",
                        Value = "Subsidiary"
                    },
                    new SelectListItem
                    {
                        Text = "Previous Consulting Business",
                        Value = "Previous Consulting Business"
                    }
                    ,
                    new SelectListItem
                    {
                        Text = "Mergers",
                        Value = "Mergers"
                    }
                    ,
                    new SelectListItem
                    {
                        Text = "Joint Venture",
                        Value = "Joint Venture"
                    }
                    ,
                    new SelectListItem
                    {
                        Text = "Major Share Holder (Not being a PM)",
                        Value = "Major Share Holder"
                    }
                    
                };
            return _Types;
        }      
        private IList<SelectListItem> GetLicencedOptions()
        {
            var _Types = new List<SelectListItem>()
            {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    }
                ,
                new SelectListItem
                    {
                        Text = "None",
                        Value = "None"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Category 1",
                        Value = "Category 1"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Category 2",
                        Value = "Category 2"
                    }
                ,
                new SelectListItem
                    {
                        Text = "Category 3",
                        Value = "Category 3"
                    }
            };
            return _Types;
        }
        private IList<SelectListItem> GetBooleanSelectOptions()
        {
            var _Types = new List<SelectListItem>()
            {
                new SelectListItem
                    {
                        Text = "No",
                        Value = "false"
                    },
                new SelectListItem
                    {
                        Text = "Yes",
                        Value = "true"
                    }
            };
            return _Types;
        }
        private IList<SelectListItem> GetStandardSelectOptions()
        {
            var _Types = new List<SelectListItem>()
            {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                new SelectListItem
                    {
                        Text = "No",
                        Value = "false"
                    },
                new SelectListItem
                    {
                        Text = "Yes",
                        Value = "true"
                    }
            };
            return _Types;
        }
        private IList<SelectListItem> GetOrganisationTypes()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                    new SelectListItem
                    {
                        Text = "Company",
                        Value = "Corporation – Limited liability"
                    },
                    new SelectListItem
                    {
                        Text = "Private",
                        Value = "Person - Individual"
                    },
                    new SelectListItem
                    {
                        Text = "Trust",
                        Value = "Trust"
                    },
                    new SelectListItem
                    {
                        Text = "Partnership",
                        Value = "Partnership"
                    },
                    new SelectListItem
                    {
                        Text = "Incorporated Society",
                        Value = "Incorporated Society"
                    }
                    ,
                    new SelectListItem
                    {
                        Text = "Government",
                        Value = "Government"
                    },
                    new SelectListItem
                    {
                        Text = "Financial Institution",
                        Value = "Financial Institution"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetHasRegisteredOptions()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {                            
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                    new SelectListItem
                    {
                        Text = "AFA",
                        Value = "AFA"
                    },
                    new SelectListItem
                    {
                        Text = "RFA",
                        Value = "RFA"
                    },
                    new SelectListItem
                    {
                        Text = "N/A",
                        Value = "N/A"
                    }
                };
            return _Types;
        }
        private IList<SelectListItem> GetAdvisorTypes4()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                new SelectListItem
                    {
                        Text = "Director",
                        Value = "Director"
                    }
            };
            return _Types;

        }
        private IList<SelectListItem> GetAdvisorTypes1()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                new SelectListItem
                    {
                        Text = "Advisor",
                        Value = "Advisor"
                    },
                new SelectListItem
                {
                    Text = "Nominated Representative",
                    Value = "Nominated Representative"
                },
                new SelectListItem
                    {
                        Text = "Administrator",
                        Value = "Administrator"
                    },
                new SelectListItem
                {
                    Text = "Other Consulting Business",
                    Value = "Other Consulting Business"
                }
            };
            return _Types;

        }
        private IList<SelectListItem> GetAdvisorTypes2()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                new SelectListItem
                    {
                        Text = "-- Select --",
                        Value = "0"
                    },
                new SelectListItem
                    {
                        Text = "Advisor",
                        Value = "Advisor"
                  },
                new SelectListItem
                    {
                        Text = "Administrator",
                        Value = "Administrator"
                    },
                new SelectListItem
                {
                    Text = "Nominated Representative",
                    Value = "Nominated Representative"
                },
                new SelectListItem
                {
                    Text = "Other Consulting Business",
                    Value = "Other Consulting Business"
                }
            };
            return _Types;

        }
        private IList<SelectListItem> GetAdvisorTypes3()
        {
            
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                new SelectListItem
                {
                    Text = "-- Select --",
                    Value = "0"
                },
                new SelectListItem
                {
                    Text = "Advisor",
                    Value = "Advisor"
                },
                new SelectListItem
                {
                    Text = "Mentored Advisor",
                    Value = "Mentored Advisor"
                },
                new SelectListItem
                {
                        Text = "Administrator",
                        Value = "Administrator"
                },
                new SelectListItem
                {
                    Text = "Other Consulting Business",
                    Value = "Other Consulting Business"
                }
            };
            return _Types;

        }
        private IList<SelectListItem> GetIndividualInsured()
        {
            var _Types = new List<SelectListItem>();
            _Types = new List<SelectListItem>() {
                new SelectListItem
                {
                    Text = "-- Select --",
                    Value = "0"
                },
                new SelectListItem
                {
                    Text = "Individual",
                    Value = "Individual"
                }
            };
            return _Types;
        }

        [JsonIgnore]
        public Domain.Entities.Programme Programme { get; set; }
        public Guid ID { get; set; }
        public Guid ProgrammeId { get; set; }
        public Domain.Entities.Organisation Organisation { get; set; }
        public User User { get; set; }
        [Display(Name ="Type")]
        [JsonIgnore]
        public IList<SelectListItem> InsuranceAttributes { get; set; }
        [Display(Name = "Organisation Type")]
        [JsonIgnore]
        public IList<SelectListItem> Individuals { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> OrganisationTypes { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasRetiredorDeceasedOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasRegisteredOptions { get; set; }
        [JsonIgnore]
        public IList<Domain.Entities.Organisation> Organisations { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasPrincipalOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasAssociationOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasRegisteredLicensedOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasDesignLicencedOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasSiteLicensedOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasCurrentMembershipOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasContractorInsuredOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasInsuredRequiredOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> InsuredEntityRelationOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> CertTypes { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasIsTripleAApprovalOptions { get; set; }        
        [JsonIgnore]
        public IList<SelectListItem> HasMajorShareHolder { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasIsIPENZmemberOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasCPEngQualifiedOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasIsNZIAmemberOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasIsADNZmemberOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasIsOtherdirectorshipOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasNZPIAMemberOptions { get; set; }
        [JsonIgnore]
        public IList<SelectListItem> HasContractedInsuredOptions { get; set; }
        public IList<SelectListItem> InterestedPartyOptions { get; set; }
        public IList<SelectListItem> OwnershipOptions { get; set; }
        public AdvisorUnit AdvisorUnit { get; set; }
        public PersonnelUnit PersonnelUnit { get; set; }
        public PrincipalUnit PrincipalUnit { get; set; }
        public InterestedPartyUnit InterestedPartyUnit { get; set; }
        public PlannerUnit PlannerUnit { get; set; }
        public MarinaUnit MarinaUnit { get; set; }
        public IndividualInsuredUnit IndividualInsuredUnit { get; set; }
        public IList<Domain.Entities.Organisation> PublicOrganisations { get; set; }
        public IList<SelectListItem> HasIsFAPOptions { get; set; }
        public IList<SelectListItem> HasInterposedPersonOptions { get; set; }
        public AdministratorUnit AdministratorUnit { get; set; }

        public DirectorUnit DirectorUnit { get; set; }

    }
}


