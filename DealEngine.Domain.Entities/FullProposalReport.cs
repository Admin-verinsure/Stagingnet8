using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
    [JsonObject]
    public class FullProposalReport : EntityBase, IAggregateRoot
    {

        protected FullProposalReport() : base(null) { }

        public virtual string DeclaringOfficer { get; set; }
        public virtual string Organisations { get; set; }

        public virtual string Locations { get; set; }
        public virtual string TerritoriesPercentage { get; set; }      
        public virtual decimal ActualCommissionLastYear { get; set; }
        public virtual decimal EstimatedCommissionCurrentYear { get; set; }
        public virtual decimal EstimatedCommissionNextYear { get; set; }
        public virtual string ActivitiesPercentageCurrentYear { get; set; }
        public virtual string CoverClubAssets { get; set; }
        public virtual string AnyClubAssetOver15000 { get; set; }
        public virtual List<string> ScheduledAssetsOver15000 { get; set; }
        public virtual string ClubRealEstate { get; set; }
        public virtual string ClubMotorVehicle { get; set; }
        public virtual string lossExperience5000 { get; set; }
        public virtual string ClaimWithdrawn { get; set; }
        public virtual string InsuranceIssues { get; set; }
        public virtual string CriminalOffense { get; set; }
        public virtual string Bankruptcy { get; set; }
        public virtual List<string> Buildings { get; set; }
        public virtual string GeneralLiability { get; set; }
        public virtual string Youthprogrammedetails { get; set; }
        public virtual string Documents { get; set; }


        public virtual string Declaration { get; set; }



        public static FullProposalReport Create(
           string declaringOfficer,
           string organisations,
           string locations,
           string territoriesPercentage,
           decimal actualCommissionLastYear,
           decimal estimatedCommissionCurrentYear,
           decimal estimatedCommissionNextYear,
           string activitiesPercentageCurrentYear,
           string coverClubAssets,
           string anyClubAssetOver15000,
           List<string> scheduledAssetsOver15000,
           string clubRealEstate,
           string clubMotorVehicle,
           string lossExperience5000,
           string claimWithdrawn,
           string insuranceIssues,
           string criminalOffense,
           string bankruptcy,
           List<string> buildings,
           string generalLiability,
           string documents,
           string declaration)
        {
            return new FullProposalReport
            {
                DeclaringOfficer = declaringOfficer,
                Organisations = organisations,
                Locations = locations,
                TerritoriesPercentage = territoriesPercentage,
                ActualCommissionLastYear = actualCommissionLastYear,
                EstimatedCommissionCurrentYear = estimatedCommissionCurrentYear,
                EstimatedCommissionNextYear = estimatedCommissionNextYear,
                ActivitiesPercentageCurrentYear = activitiesPercentageCurrentYear,
                CoverClubAssets = coverClubAssets,
                AnyClubAssetOver15000 = anyClubAssetOver15000,
                ScheduledAssetsOver15000 = scheduledAssetsOver15000,
                ClubRealEstate = clubRealEstate,
                ClubMotorVehicle = clubMotorVehicle,
                lossExperience5000 = lossExperience5000,
                ClaimWithdrawn = claimWithdrawn,
                InsuranceIssues = insuranceIssues,
                CriminalOffense = criminalOffense,
                Bankruptcy = bankruptcy,
                Buildings = buildings,
                GeneralLiability = generalLiability,
                Documents= documents,
                Declaration = declaration
            };
        }








    }
}

