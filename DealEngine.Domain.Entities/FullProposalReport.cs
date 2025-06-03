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
        public virtual decimal NameofApplicant { get; set; }
        public virtual decimal SubsidiaryCompanies { get; set; }
        public virtual decimal PhoneNumber { get; set; }
        public virtual string Email { get; set; }
        public virtual string PostalAddress { get; set; }

        public virtual string NumberofYearsBusiness { get; set; }
        public virtual string PeriodofInsuranceFrom { get; set; }
        public virtual decimal PeriodofInsuranceTo { get; set; }
        public virtual decimal GeneralLiabilityLimit { get; set; }
        public virtual decimal StatutoryLiabilityLimit { get; set; }
        public virtual string EmployersLiabilityLimit { get; set; }
        public virtual string RetroactiveDate { get; set; }

        public virtual string BusinessoutsideofNewZealand { get; set; }

        public virtual string NumberEmployeesinNZ { get; set; }
        public virtual decimal NumberlocationsNZ { get; set; }
        public virtual decimal DescriptionActivity { get; set; }
        public virtual decimal NameInsured { get; set; }
        public virtual string CurrentYearTurnover { get; set; }
        public virtual string NextYearTurnover { get; set; }

        public virtual string jointventureactivites { get; set; }
        public virtual string contructionactivites { get; set; }

        public virtual string ApplyInsolvencyExclusion { get; set; }
        public virtual string Businessisabletopay { get; set; }


        public virtual decimal CurrentAssets { get; set; }
        public virtual decimal TotalAssets { get; set; }
        public virtual decimal CurrentLiabilities { get; set; }
        public virtual decimal TotalLiabilities { get; set; }


        public virtual decimal anydirectorhanydirector { get; set; }
        public virtual decimal anydirectorhanydirector5yr { get; set; }
        public virtual decimal Declinedrenewpolicy  { get; set; }
		public virtual decimal withdrawnaclaim { get; set; }
        public virtual string declinedanyclaim { get; set; }
        public virtual string declaredbankrupt { get; set; }
        public virtual string anycriminaloffence { get; set; }


        public virtual string Declaration { get; set; }



        public static FullProposalReport Create(
           string submissionReference,
    string eglobalClientNumber,
    string eglobalBranch,
    decimal nameofApplicant,
    decimal subsidiaryCompanies,
    decimal phoneNumber,
    string email,
    string postalAddress,
    string numberOfYearsBusiness,
    string periodOfInsuranceFrom,
    decimal periodOfInsuranceTo,
    decimal generalLiabilityLimit,
    decimal statutoryLiabilityLimit,
    string employersLiabilityLimit,
    string retroactiveDate,
    string businessoutsideofNewZealand,
    string numberEmployeesinNZ,
    decimal numberlocationsNZ,
    decimal descriptionActivity,
    decimal nameInsured,
    string currentYearTurnover,
    string nextYearTurnover,
    string jointventureactivites,
    string contructionactivites,
    string applyInsolvencyExclusion,
    string businessisabletopay,
    decimal currentAssets,
    decimal totalAssets,
    decimal currentLiabilities,
    decimal totalLiabilities,
    decimal anydirectorhanydirector,
    decimal anydirectorhanydirector5yr,
    decimal declinedrenewpolicy,
    decimal withdrawnaclaim,
    string declinedanyclaim,
    string declaredbankrupt,
    string anycriminaloffence)
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
        anydirectorhanydirector         = anydirectorhanydirector,
        anydirectorhanydirector5yr      = anydirectorhanydirector5yr,
        Declinedrenewpolicy             = declinedrenewpolicy,
        withdrawnaclaim                 = withdrawnaclaim,
        declinedanyclaim                = declinedanyclaim,
        declaredbankrupt                = declaredbankrupt,
        anycriminaloffence              = anycriminaloffence
    };
        }








    }
}

