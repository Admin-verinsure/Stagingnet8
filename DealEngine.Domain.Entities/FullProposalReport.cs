using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;
using static NHibernate.Engine.Query.CallableParser;

namespace DealEngine.Domain.Entities
{
    [JsonObject]
    public class FullProposalReport : EntityBase, IAggregateRoot
    {

        protected FullProposalReport() : base(null) { }

        public virtual string SubmissionReference { get; set; }


        public virtual string EglobalClientNumber { get; set; }
        public virtual string EglobalBranch { get; set; }
        public virtual string NameofApplicant { get; set; }
        public virtual string SubsidiaryCompanies { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Email { get; set; }
        public virtual string PostalAddress { get; set; }

        public virtual string NumberofYearsBusiness { get; set; }
        public virtual string PeriodofInsuranceFrom { get; set; }
        public virtual string PeriodofInsuranceTo { get; set; }
        public virtual double GeneralLiabilityLimit { get; set; }
        public virtual double StatutoryLiabilityLimit { get; set; }
        public virtual double EmployersLiabilityLimit { get; set; }
        public virtual string RetroactiveDate { get; set; }

        public virtual string BusinessoutsideofNewZealand { get; set; }

        public virtual string NumberEmployeesinNZ { get; set; }
        public virtual string NumberlocationsNZ { get; set; }
        public virtual string DescriptionActivity { get; set; }
        public virtual string NameInsured { get; set; }
        public virtual decimal CurrentYearTurnover { get; set; }
        public virtual decimal NextYearTurnover { get; set; }

        public virtual string jointventureactivites { get; set; }
        public virtual string contructionactivites { get; set; }

        public virtual string ApplyInsolvencyExclusion { get; set; }
        public virtual string Businessisabletopay { get; set; }


        public virtual string CurrentAssets { get; set; }
        public virtual string TotalAssets { get; set; }
        public virtual string CurrentLiabilities { get; set; }
        public virtual string TotalLiabilities { get; set; }


        public virtual string Anydirectorhanydirector { get; set; }
        public virtual string Anydirectorclaim5yr { get; set; }
        public virtual string Declinedrenewpolicy  { get; set; }
		public virtual decimal withdrawnaclaim { get; set; }
        public virtual string declinedanyclaim { get; set; }
        public virtual string declaredbankrupt { get; set; }
        public virtual string anycriminaloffence { get; set; }


        public virtual string Declaration { get; set; }



        public static FullProposalReport Create(
           string submissionReference,
    string eglobalClientNumber,
    string eglobalBranch,
    string nameofApplicant,
    string subsidiaryCompanies,
    string phoneNumber,
    string email,
    string postalAddress,
    string numberOfYearsBusiness,
    string periodOfInsuranceFrom,
    string periodOfInsuranceTo,
    double generalLiabilityLimit,
    double statutoryLiabilityLimit,
    double employersLiabilityLimit,
    string retroactiveDate,
    string businessoutsideofNewZealand,
    string numberEmployeesinNZ,
    string numberlocationsNZ,
    string descriptionActivity,
    string nameInsured,
    decimal currentYearTurnover,
    decimal nextYearTurnover,
    string jointventureactivites,
    string contructionactivites,
    string applyInsolvencyExclusion,
    string businessisabletopay,
    string currentAssets,
    string totalAssets,
    string currentLiabilities,
    string totalLiabilities,
    string anydirectorproceedings,
    string anydirectorclaim5yr,
    string declinedrenewpolicy
   // decimal withdrawnaclaim,
  //  string declinedanyclaim,
  //  string declaredbankrupt,
   // string anycriminaloffence
    )
        {
            return new FullProposalReport
    {
        SubmissionReference             = submissionReference,
        EglobalClientNumber             = eglobalClientNumber,
        EglobalBranch                   = eglobalBranch,
        NameofApplicant                 = nameofApplicant,
        SubsidiaryCompanies             = subsidiaryCompanies,
        PhoneNumber                     = phoneNumber,
        Email                           = email,
        PostalAddress                   = postalAddress,
        NumberofYearsBusiness           = numberOfYearsBusiness,
        PeriodofInsuranceFrom           = periodOfInsuranceFrom,
        PeriodofInsuranceTo             = periodOfInsuranceTo,
        GeneralLiabilityLimit           = generalLiabilityLimit,
        StatutoryLiabilityLimit         = statutoryLiabilityLimit,
        EmployersLiabilityLimit         = employersLiabilityLimit,
        RetroactiveDate                 = retroactiveDate,
        BusinessoutsideofNewZealand     = businessoutsideofNewZealand,
        NumberEmployeesinNZ             = numberEmployeesinNZ,
        NumberlocationsNZ               = numberlocationsNZ,
        DescriptionActivity             = descriptionActivity,
        NameInsured                     = nameInsured,
        CurrentYearTurnover             = currentYearTurnover,
        NextYearTurnover                = nextYearTurnover,
        jointventureactivites           = jointventureactivites,
        contructionactivites            = contructionactivites,
        ApplyInsolvencyExclusion        = applyInsolvencyExclusion,
        Businessisabletopay             = businessisabletopay,
        CurrentAssets                   = currentAssets,
        TotalAssets                     = totalAssets,
        CurrentLiabilities              = currentLiabilities,
        TotalLiabilities                = totalLiabilities,
        Anydirectorhanydirector         = anydirectorproceedings,
        Anydirectorclaim5yr = anydirectorclaim5yr,
        Declinedrenewpolicy             = declinedrenewpolicy
       // withdrawnaclaim                 = withdrawnaclaim,
        //declinedanyclaim                = declinedanyclaim,
       // declaredbankrupt                = declaredbankrupt,
        //anycriminaloffence              = anycriminaloffence
    };
        }








    }
}

