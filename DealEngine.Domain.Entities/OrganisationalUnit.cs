using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using DealEngine.Domain.Entities.Abstracts;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Domain.Entities
{
    public class OrganisationalUnit : EntityBase, IAggregateRoot
    {
        public OrganisationalUnit() : base(null) { }

        public OrganisationalUnit(User createdBy, string name)
            : base(createdBy)
        {
            Name = name;
        }

        public OrganisationalUnit(User createdBy, string name, string type, IFormCollection collection)
            : base(createdBy)
        {
            if (collection != null)
            {
                PopulateEntity(collection);
            }
            Name = name;
            Type = type;
        }

        public virtual string Type { get; set; }
        public virtual string Name { get; set; }
        public virtual string EserviceProducerCode { get; set; }
        public virtual string EbixDepartmentCode { get; set; }

        public virtual string HPFBranchCode { get; set; }

        //public virtual Guid organisation_id { get; set; }
    }

    public class AdvisorUnit : OrganisationalUnit
    {
        public AdvisorUnit() { }
        public AdvisorUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {
        }

        [Display(Name = "List any industry qualifications you have. (If none please put nil)")]
        public virtual string Qualifications { get; set; }
        [Display(Name = "Insurance experience and positions held")]
        public virtual string Experience { get; set; }
        [Display(Name = "Is this person retired or deceased?")]
        public virtual bool IsRetiredorDeceased { get; set; }
        [Display(Name = "Select to confirm you have received approval from the TripleA Advisers Association for this adviser to be included under the TripleA PI scheme")]
        public virtual bool IsTripleAApproval { get; set; }
        [Display(Name = "Registered Status AFA, RFA or N/A")]
        public virtual string RegisteredStatus { get; set; }
        [Display(Name = "Duration Of Time as Adviser")]
        public virtual string Duration { get; set; }
        [Display(Name = "Is this individual the FAP’s Principal Adviser?")]
        public virtual bool IsPrincipalAdvisor { get; set; }
        [Display(Name = "Date of Retirement(Please Enter either Date of Retirement or Date of Deceased)")]
        public virtual DateTime? DateofRetirement { get; set; }
        [Display(Name = "Date of Deceased (Please Enter either Date of Retirement or Date of Deceased)")]
        public virtual DateTime? DateofDeceased { get; set; }
        [Display(Name = "Date of Commencement")]
        public virtual DateTime? DateofCommencement { get; set; }
        public virtual string MyCRMId { get; set; }
        public virtual string PIRetroactivedate { get; set; }
        public virtual string DORetroactivedate { get; set; }
        [Display(Name = "Please select the Professional Bodies or Associations that the company or individual is a member of:")]
        public virtual IList<string> Associations { get; set; }
        [Display(Name = "If Other is specified, please describe")]
        public virtual string OtherInformation { get; set; }
        // Craig asked for this, but we never finished what its to be used for
        public virtual string FSP { get; set; }

        [Display(Name = "FAP License number")]
        public virtual string FAPLicenseNumber { get; set; }

        [Display(Name = "Is this person, entity or company the FAP licence holder for this policy?")]
        public virtual bool isTheFAP { get; set; }

        [Display(Name = "Are you intending to apply for interposed person arrangements?")]
        public virtual bool isInterposedPerson { get; set; }

    }

    public class EBaristerUnit : OrganisationalUnit
    {
        public EBaristerUnit() { }
        public EBaristerUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {
        }
        public virtual string OtherInformation { get; set; }
        [Display(Name = "Title")]
        public virtual string EInitial { get; set; }
        [Display(Name = "Honorific")]
        public virtual string Ehonorific { get; set; }

    }

    public class JBaristerUnit : OrganisationalUnit
    {
        public JBaristerUnit() { }
        public JBaristerUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {
        }
        public virtual string OtherInformation { get; set; }
        [Display(Name = "Title")]

        public virtual string JInitial { get; set; }
        [Display(Name = "Honorific")]
        public virtual string Jhonorific { get; set; }
    }

    public class BarristerUnit : OrganisationalUnit
    {
        public BarristerUnit() { }
        public BarristerUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {
        }
        public virtual string OtherInformation { get; set; }
        [Display(Name = "Is this individual the Principal Barrister?")]
        public virtual bool IsPrincipalBarrister { get; set; }
        [Display(Name = "Title")]
        public virtual string Initial { get; set; }
        [Display(Name = "Honorific")]
        public virtual string honorific { get; set; }
    }

    public class PersonnelUnit : OrganisationalUnit
    {
        public PersonnelUnit() { }
        public PersonnelUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {

        }
        [Display(Name = "Is this personnel registered as a licensed building practioner")]
        public virtual bool IsRegisteredLicensed { get; set; }
        [Display(Name = "Qualifications")]
        public virtual string Qualifications { get; set; }
        [Display(Name = "Relation to insured entity")]
        public virtual string InsuredEntityRelation { get; set; }
        [Display(Name = "Date Qualified")]
        public virtual DateTime? DateQualified { get; set; }
        [Display(Name = "Design Licensed")]
        public virtual string DesignLicensed { get; set; }
        [Display(Name = "Site Licensed")]
        public virtual string SiteLicensed { get; set; }
        [Display(Name = "Does this person have an association membership")]
        public virtual bool IsCurrentMembership { get; set; }
        [Display(Name = "Association")]
        public virtual string OtherCompanyName { get; set; }
        [Display(Name = "Years as a Member")]
        public virtual string YearOfPractice { get; set; }
        ///pminz
        [Display(Name = "Job title")]
        public virtual string JobTitle { get; set; }
        [Display(Name = "Professional Affiliations")]
        public virtual string ProfAffiliation { get; set; }
        [Display(Name = "Does this contractor require to be insured under this policy")]
        public virtual bool IsInsuredRequired { get; set; }
        [Display(Name = "Does this contractor carry their own insurance")]
        public virtual bool IsContractorInsured { get; set; }
        [Display(Name = "Do you have a current PMI membership (i.e. not expired)")]
        public virtual bool IsCurrentMembershipPMINZ { get; set; }
        [Display(Name = "PMI Certification Type")]
        public virtual string CertType { get; set; }
        [Display(Name = "PMI membership No")]
        public virtual string CurrentMembershipNo { get; set; }
        [Display(Name = "Does this party own 10% or more of the insured's issued shares")]
        public virtual bool MajorShareHolder { get; set; }
    }

    public class PrincipalUnit : OrganisationalUnit
    {
        public PrincipalUnit() { }
        public PrincipalUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {

        }
        [Display(Name = "Date of Retirement (Please Enter either Date of Retirement or Date of Deceased)")]
        public virtual DateTime? DateofRetirement { get; set; }
        [Display(Name = "Is this person retired or deceased?")]
        public virtual bool IsRetiredorDeceased { get; set; }
        [Display(Name = "Qualifications")]
        public virtual string Qualifications { get; set; }
        [Display(Name = "Engineer Member")]
        public virtual string IsIPENZmember { get; set; }
        [Display(Name = "CPEng Qualified")]
        public virtual string CPEngQualified { get; set; }
        [Display(Name = "How long as a Principal of this practice")]
        public virtual string YearOfPracticeCEAS { get; set; }
        [Display(Name = "How long as a Principal of any practice")]
        public virtual string PrevPracticeCEAS { get; set; }
        [Display(Name = "Are you a member of NZIA")]
        public virtual bool IsNZIAmember { get; set; }
        [Display(Name = "Are you a member of ADNZ")]
        public virtual bool IsADNZmember { get; set; }
        [Display(Name = "State the type of NZIA membership held")]
        public virtual string NZIAmembership { get; set; }
        [Display(Name = "Do you hold LPB Category 3 classification")]
        public virtual bool IsLPBCategory3 { get; set; }
        [Display(Name = "How long have you been at this practice")]
        public virtual string YearOfPracticeNZACS { get; set; }
        [Display(Name = "Name of previous practice")]
        public virtual string PrevPracticeNZACS { get; set; }
        [Display(Name = "Do you require coverage for other directorship appointments held by you on behalf of your practice")]
        public virtual bool IsOtherdirectorship { get; set; }
        [Display(Name = "Please list all Name of company(s)")]
        public virtual string TradingName { get; set; }
    }

    public class PlannerUnit : OrganisationalUnit
    {
        public PlannerUnit() { }
        public PlannerUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {

        }

        [Display(Name = "Are you a member of NZPI")]
        public virtual bool IsNZPIAMember { get; set; }

        [Display(Name = "Qualifications")]
        public virtual string Qualifications { get; set; }
        [Display(Name = "Date of Retirement (Please Enter either Date of Retirement or Date of Deceased)")]
        public virtual DateTime? DateQualified { get; set; }
        [Display(Name = "Years at firm?")]
        public virtual int YearsAtFirm { get; set; }
        [Display(Name = "Solely contracted to the Insured?")]
        public virtual bool ContractedInsured { get; set; }
        [Display(Name = "Years working for Insured?")]
        public virtual int YearsAtInsured { get; set; }
        [Display(Name = "Principal Planner?")]
        public virtual bool IsPrincipalPlanner { get; set; }
        [Display(Name = "Enter fees received from the Insured in the last 12 months?")]
        public virtual int FeesIn12Months { get; set; }



    }

    public class InterestedPartyUnit : OrganisationalUnit
    {
        public InterestedPartyUnit() { }
        public InterestedPartyUnit(User User, string Name, string Type, IFormCollection Collection)
        : base(User, Name, Type, Collection)
        {

        }

        [Display(Name = "Type of Interested Party?")]
        public virtual string PartyType { get; set; }
        [Display(Name = "Type of Interested Party?")]
        public virtual Location Location { get; set; }
    }

    public class MarinaUnit : OrganisationalUnit
    {
        public MarinaUnit() { }
        public MarinaUnit(User User, string Name, string Type, IFormCollection Collection)
        : base(User, Name, Type, Collection)
        {

        }

        public virtual WaterLocation WaterLocation { get; set; }
    }

    public class IndividualInsuredUnit : OrganisationalUnit
    {
        public IndividualInsuredUnit() { }

        public IndividualInsuredUnit(User User, string Name, string Type, IFormCollection Collection)
        : base(User, Name, Type, Collection)
        {

        }
        [Display(Name = "Phone")]
        public virtual string Phone { get; set; }

    }


    public class AdministratorUnit : OrganisationalUnit
    {
        public AdministratorUnit() { }
        public AdministratorUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {

        }


        [Display(Name = "Street")]
        public virtual string Street { get; set; }

        [Display(Name = "Suburb")]
        public virtual string Suburb { get; set; }

        [Display(Name = "PostCode")]
        public virtual string PostCode { get; set; }

        [Display(Name = "Country")]
        public virtual string Country { get; set; }

        [Display(Name = "Phone Number")]
        public virtual string PhoneNumber { get; set; }

        [Display(Name = "FAP License number")]
        public virtual string FAPAdministratorLicenseNumber { get; set; }

        [Display(Name = "Is FAP?")]
        public virtual bool isAdministratorTheFAP { get; set; }

        [Display(Name = "Are you intending to apply for interposed person arrangements?")]
        public virtual bool isAdministratorInterposedPerson { get; set; }

    }

    public class DirectorUnit : OrganisationalUnit
    {
        public DirectorUnit() { }
        public DirectorUnit(User User, string Name, string Type, IFormCollection Collection)
            : base(User, Name, Type, Collection)
        {
        }

        [Display(Name = "Is this person retired or deceased?")]
        public virtual bool IsRetiredorDeceased { get; set; }
        [Display(Name = "Select to confirm you have received approval from the TripleA directors Association for this director to be included under the TripleA PI scheme")]
     
        public virtual DateTime? DateofRetirement { get; set; }
        [Display(Name = "Date of Deceased (Please Enter either Date of Retirement or Date of Deceased)")]
        public virtual DateTime? DateofDeceased { get; set; }
    }
}



