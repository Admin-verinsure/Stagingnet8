

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    [JsonObject]
    public class Organisation : EntityBase, IAggregateRoot
    {
        #region Constructors
        public Organisation() : base(null) { }

        protected Organisation(User createdBy)
            : base(createdBy)
        {
            OrganisationalUnits = new List<OrganisationalUnit>();
            InsuranceAttributes = new List<InsuranceAttribute>();
        }

        public Organisation(User createdBy, Guid id)
            : base(createdBy)
        {
            Id = id;
            OrganisationalUnits = new List<OrganisationalUnit>();
            InsuranceAttributes = new List<InsuranceAttribute>();
        }

        public Organisation(User createdBy, string organisationName)
            : base(createdBy)
        {
            if (string.IsNullOrWhiteSpace(organisationName))
                throw new ArgumentNullException(nameof(organisationName), "Not allowed to create an organisation with no name.");

            Name = organisationName;
            OrganisationalUnits = new List<OrganisationalUnit>();
            InsuranceAttributes = new List<InsuranceAttribute>();
        }

        public Organisation(User createdBy, Guid id, string organisationName, OrganisationType organisationType)
            : this(createdBy, organisationName, organisationType)
        {
            Id = id;
        }

        public Organisation(User createdBy, Guid id, string organisationName)
            : this(createdBy, organisationName)
        {
            Id = id;
        }

        public Organisation(User createdBy, string organisationName, OrganisationType organisationType)
            : this(createdBy, organisationName)
        {
            if (organisationType == null)
                throw new ArgumentNullException(nameof(organisationType), "Not allowed to create an organisation without specifying a type.");

            Name = organisationName;
            OrganisationType = organisationType;
        }

        public Organisation(User createdBy, string organisationName, OrganisationType organisationType, string email, string phone)
           : this(createdBy, organisationName)
        {
            if (organisationType == null)
                throw new ArgumentNullException(nameof(organisationType), "Not allowed to create an organisation without specifying a type.");

            Name = organisationName;
            Email = email;
            Phone = phone;
            OrganisationType = organisationType;
        }

        public Organisation(User createdBy, Guid id, string organisationName, OrganisationType organisationType, string email)
          : this(createdBy, organisationName, organisationType)
        {
            if (organisationType == null)
                throw new ArgumentNullException(nameof(organisationType), "Not allowed to create an organisation without specifying a type.");
            Id = id;
            Email = email;
        }

        public Organisation(User creator, Guid id, string organisationName, OrganisationType organisationType, List<OrganisationalUnit> organisationalUnits, InsuranceAttribute insuranceAttribute, string email)
            : this(creator, organisationName, organisationType)
        {
            Name = organisationName;
            Id = id;
            Email = email;
            OrganisationalUnits = organisationalUnits;
            InsuranceAttributes.Add(insuranceAttribute);
        }

        #endregion

        #region Getters
        public virtual string Name { get; set; }
        [Display(Name = "Trading Name")]
        public virtual string TradingName { get; set; }


        [Display(Name = "FAP License number")]
        public virtual string OrganisationFAPLicenseNumber { get; set; }

        [Display(Name = "Is this person, entity or company the FAP licence holder for this policy?")]
        public virtual bool isOrganisationTheFAP { get; set; }

        [Display(Name = "Are you intending to apply for interposed person arrangements?")]
        public virtual bool isOrganisationInterposedPerson { get; set; }

        public virtual OrganisationType OrganisationType { get; set; }
        public virtual bool Removed { get; set; }
        // public virtual string Othercompanyname { get; set; }
        public virtual string Description { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Domain { get; set; }
        public virtual string Email { get; set; }
        public virtual bool IsBroker { get; set; }
        public virtual bool IsInsurer { get; set; }
        public virtual bool IsTC { get; set; }
        public virtual bool IsProgrammeManager { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual string Clientmembership { get; set; }
        public virtual bool OrgBeenMoved { get; set; }
        public virtual IList<InsuranceAttribute> InsuranceAttributes { get; set; }
        [Display(Name = "Initial")]
      
        //public virtual Organisation CloneForNewSheet(ClientInformationSheet newSheet)
        //{
        //    Organisation newOrganisation = new Organisation();
        //    newOrganisation.Clientmembership = Clientmembership;
        //    newOrganisation.CreatedBy = newSheet.CreatedBy;
        //    newOrganisation.DateCreated = DateTime.Now;
        //    newOrganisation.DateofIncorportation = DateofIncorportation;
        //    newOrganisation.Description = Description;
        //    newOrganisation.Domain = Domain;
        //    newOrganisation.DORetroactivedate = DORetroactivedate;
        //    newOrganisation.Email = Email;
        //    newOrganisation.Id = Guid.NewGuid();
        //    newOrganisation.InsuranceAttributes = InsuranceAttributes;
        //    newOrganisation.IsApproved = IsApproved;
        //    newOrganisation.IsBroker = IsBroker;
        //    newOrganisation.IsInsurer = IsInsurer;
        //    newOrganisation.IsTC = IsTC;
        //    newOrganisation.Name = Name;
        //    newOrganisation.OrganisationalUnits = OrganisationalUnits;
        //    newOrganisation.OrganisationType = OrganisationType;
        //    newOrganisation.Phone = Phone;
        //    newOrganisation.PIRetroactivedate = PIRetroactivedate;
        //    newOrganisation.TradingName = TradingName;
        //    return newOrganisation;
        //}

        #endregion

        #region OldGetters

        //public virtual string SkipperExp
        //{
        //    get;
        //    set;
        //}


        //public virtual string Website
        //{
        //    get;
        //    set;
        //}
        //public virtual DateTime DateofRetirement
        //{
        //    get;
        //    set;
        //}

        //public virtual bool IsRetiredorDecieved
        //{
        //    get;
        //    set;
        //}
        //public virtual string Activities
        //{
        //    get;
        //    set;
        //}


        //public virtual DateTime DateofDeceased
        //{
        //    get;
        //    set;
        //}

        //public virtual DateTime DateofBirth
        //{
        //    get;
        //    set;
        //}

        //public virtual IList<Boat> Boat
        //{
        //    get;
        //    set;
        //}
        //public virtual bool IsCurrentMembership { get; set; }
        //// public virtual string Othercompanyname { get; set; }
        //public virtual string DateQualified { get; set; }
        //public virtual string DesignLicensed { get; set; }
        //public virtual string SiteLicensed { get; set; }
        //public virtual bool IsRegisteredLicensed { get; set; }
        //public virtual Location Location { get; set; }
        //public virtual string Qualifications { get; set; }
        //public virtual bool IsNZIAmember { get; set; }
        //public virtual string NZIAmembership { get; set; }
        //public virtual string CPEngQualified { get; set; }
        //public virtual bool IsADNZmember { get; set; }
        //public virtual string IsIPENZmember { get; set; }
        //public virtual string YearofPractice { get; set; }
        //public virtual string PrevPractice { get; set; }
        //public virtual string Type { get; set; }
        //public virtual string PMICert { get; set; }
        //public virtual string CertType { get; set; }
        //public virtual bool IsLPBCategory3 { get; set; }
        //public virtual bool MajorShareHolder { get; set; }
        //public virtual bool IsContractorInsured { get; set; }
        //public virtual bool IsInsuredRequired { get; set; }
        //public virtual bool IsOtherdirectorship { get; set; }
        //public virtual string InsuredEntityRelation { get; set; }
        //public virtual string OtherCompanyname { get; set; }
        //public virtual bool IsAffiliation { get; set; }
        //public virtual string AffiliationDetails { get; set; }
        //public virtual string ProfAffiliation { get; set; }
        //public virtual string JobTitle { get; set; }
        //public virtual string PartyName { get; set; }
        //public virtual string CurrentMembershipNo { get; set; }
        //public virtual string RegisteredStatus { get; set; }
        //public virtual bool ConfirmAAA { get; set; }
        //public virtual string Duration { get; set; }
        //public virtual IList<Programme> Programmes { get; set; }
        //public virtual bool IsPrincipalAdvisor { get; set; }
        //public virtual string OfcPhoneno { get; set; }
        //public virtual string MyCRMId { get; set; }
        public virtual string PIRetroactivedate { get; set; }
        public virtual string DORetroactivedate { get; set; }
        //public virtual IList<AuditHistory> AuditHistory { get; set; }
        [Display(Name = "Date of Company Incorporation")]
        public virtual DateTime? DateofIncorportation { get; set; }
        public virtual IList<Programme> Programmes { get; set; }

        #endregion

        #region Opperations

        public virtual void ChangeOrganisationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }
        #endregion

        public virtual IList<OrganisationalUnit> OrganisationalUnits { get; set; }

        public static Organisation CreateDefaultOrganisation(User creatingUser, User owner, OrganisationType organisationType)
        {
            return new Organisation(creatingUser, Guid.NewGuid(), "Default user organisation for " + owner.FullName, organisationType)
            {
                Domain = "#",
                Email = owner.Email
            };
        }
    }
}