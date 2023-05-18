using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models
{
    public class BuildingViewModel
    {
        public BuildingViewModel()
        {
            ExteriorWallTypeOptions = GetSelectExteriorWallOptions();
            ConstructionYearOptions = GetSelectConstructionYearOptions();
            FloorAreaOptions = GetSelectListOptions();
            hasEPS = GetSelectListOptions();
            hasAutomaticSprinklers = GetSelectListOptions();
            hasWaterSupply = GetSelectListOptions();
            hasUnoccupied = GetSelectListOptions();
            BuildingUses = GetBuildingUsesOptions();
            IsBuildingOccupied = GetBuildingOwners();
            IndustryOptions = GetIndustryOptions();
            OccupationOptions = GetOccupationOptions();
        }
        public Guid AnswerSheetId { get; set; }

        public Guid BuildingId { get; set; }

        public string BuildingName { get; set; }

        public string ConstructionType { get; set; }

        public int ConstructionYear { get; set; }

        public int NumOfUnits { get; set; }

        public int NumberOfStoreys { get; set; }

        public string IsBuilding3OrMoreStoreysHigh { get; set; }

        public string HasInsulatedSandwichPanels { get; set; }

        public int PercentOfInsuSandwichPanels { get; set; }

        public string StructuralFraming { get; set; }

        public string HasSprinklers { get; set; }

        public string NZS4541Compliant { get; set; }

        public string IsHalfOrMoreUnOccupied { get; set; }

        public string IsTownWaterSupplied { get; set; }

        public string HasAlarm { get; set; }

        public string BuildingLastValuationDate { get; set; }

        public string BuildingBasisofSettlement { get; set; }

        public int BuildingRVValue { get; set; }

        public int BuildingRIVValue { get; set; }

        public int BuildingDValue { get; set; }

        public int BuildingIVValue { get; set; }

        public int BuildingIIVValue { get; set; }

        public string BuildingBasisofSettlementND { get; set; }

        public string ContentsBasisofSettlement { get; set; }

        public int ContentsRVValue { get; set; }

        public int ContentsIVValue { get; set; }

        public string ContentsBasisofSettlementND { get; set; }

        public string StockBasisofSettlement { get; set; }

        public int StockRVValue { get; set; }

        public int StockIVValue { get; set; }

        public string StockBasisofSettlementND { get; set; }

        public string HasMDND { get; set; }

        public string BuildingNotes { get; set; }

        public string ResidentialProportion { get; set; }

        public string OwnerOrTenant { get; set; }

        public string HasHoseReels { get; set; }

        public string HasFireExtinguishers { get; set; }

        public string HasSecurityGuard { get; set; }

        public string HasFlammableLiquidsOrGases { get; set; }

        public string FlammableLiquidsOrGasesDesc { get; set; }

        public string HasSafe { get; set; }

        public string HasSafeAlarm { get; set; }

        public string HasSafeBolted { get; set; }

        public int ConstructionLimit { get; set; }

        public int CapitalAdditions { get; set; }

        public int CreditConstruction { get; set; }

        public int CreditCapAdds { get; set; }

        public int DomesticUnits { get; set; }

        public int BuildingFRVValue { get; set; }

        public int BuildingFRIVValue { get; set; }

        public string HasNBSExceed { get; set; }

        public IList<SelectListItem> OrganisationalUnits { get; set; }

        public Guid[] InterestedParties { get; set; }

        public Guid BuildingLocation { get; set; }

        public decimal BuildingLatitude { get; set; }

        public decimal BuildingLongitude { get; set; }

        public string BuildingApproved { get; set; }

        public string BuildingCategory { get; set; }
        public string LocationStreet { get; set; }
        public IList<SelectListItem> ExteriorWallTypeOptions { get; set; }
        public IList<SelectListItem> ConstructionYearOptions { get; set; }
        public IList<SelectListItem> FloorAreaOptions { get; set; }
        public string ResidentialUnits { get; set; }
        public string StoreysNumber { get; set; }
        public string TenantName { get; set; }

        public IList<SelectListItem> hasEPS { get; set; }
        public IList<SelectListItem> hasAutomaticSprinklers { get; set; }
        public IList<SelectListItem> hasWaterSupply { get; set; }
        public IList<SelectListItem> hasUnoccupied { get; set; }
        public List<Location> Locations { get; set; }
        public IList<SelectListItem> BuildingUses { get; set; }
        public IList<SelectListItem> IsBuildingOccupied { get; set; }
        public IList<SelectListItem> IndustryOptions { get; set; }
        public IList<SelectListItem> OccupationOptions { get; set; }

        

        private IList<SelectListItem> GetOccupationOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = ""
                },
                new SelectListItem
                {
                    Text = "Occupation1", Value = "Occupation1"
                }
            };
        }
        
          private IList<SelectListItem> GetIndustryOptions()
          {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = ""
                },
                new SelectListItem
                {
                    Text = "Accomodation & Food Services", Value = "7"
                },
                new SelectListItem
                {
                    Text = "Agriculture / Forestry", Value = "2"
                },
                new SelectListItem
                {
                    Text = "Healthcare", Value = "8"
                },
                new SelectListItem
                {
                    Text = "Manufacturing", Value = "4"
                },
                new SelectListItem
                {
                    Text = "Professional / Office", Value = "1"
                },
                new SelectListItem
                {
                    Text = "Retail", Value = "5"
                },
                new SelectListItem
                {
                    Text = "Services", Value = "11"
                },
                new SelectListItem
                {
                    Text = "Trades / Construction", Value = "3"
                },
                new SelectListItem
                {
                    Text = "Wholesale", Value = "6"
                },
                new SelectListItem
                {
                    Text = "Miscellaneous", Value = "9"
                },
                new SelectListItem
                {
                    Text = "Unknown", Value = "12"
                }
            };
          }


        private IList<SelectListItem> GetBuildingOwners()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = ""
                },
                new SelectListItem
                {
                    Text = "Owner", Value = "Owner"
                },
                new SelectListItem
                {
                    Text = "Tenant", Value = "Tenant"
                },
                new SelectListItem
                {
                    Text = "Both", Value = "Both"
                }
            };
        }

        private IList<SelectListItem> GetSelectExteriorWallOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = ""
                },
                new SelectListItem
                {
                    Text = "Brick/concrete", Value = "TC_BrickOrConcrete"
                },
                new SelectListItem
                {
                    Text = "Mixed", Value = "TC_Mixed"
                },
                new SelectListItem
                {
                    Text = "Expanded Polystyrene Panels", Value = "TC_InsulatedSandwichPanels"
                },
                new SelectListItem
                {
                    Text = "Timber", Value = "TC_Other1"
                },
                new SelectListItem
                { Text = "Other", Value = "TC_Other" }
            };
        }

        private IList<SelectListItem> GetSelectConstructionYearOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = ""
                },
                new SelectListItem
                {
                    Text = "1992+", Value = "1993"
                },
                new SelectListItem
                {
                    Text = "1976-91", Value = "1967"
                },
                new SelectListItem
                {
                    Text = "1966-75", Value = "1967"
                },
                new SelectListItem
                {
                    Text = "1936-65", Value = "1937"
                },
                new SelectListItem
                { Text = "Pre 1936", Value = "1900" }
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

        private IList<SelectListItem> GetBuildingUsesOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "Factory", Value = "factory"
                },
                new SelectListItem
                { Text = "Store", Value = "store" }
            };
        }


        

        public Building ToEntity(User creatingUser)
        {
            Building building = new Building(creatingUser);
            UpdateEntity(building);
            return building;
        }

        public Building UpdateEntity(Building building)
        {
            building.BuildingName = BuildingName;
            building.OwnerOrTenant = OwnerOrTenant;
            if (!string.IsNullOrEmpty(BuildingLastValuationDate))
            {
                building.BuildingLastValuationDate = DateTime.Parse(BuildingLastValuationDate, System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
            }
            else
            {
                building.BuildingLastValuationDate = DateTime.MinValue;
            }
            building.BuildingRVValue = BuildingRVValue;
            building.BuildingRIVValue = BuildingRIVValue;
            building.BuildingDValue = BuildingDValue;
            building.BuildingIVValue = BuildingIVValue;
            building.BuildingIIVValue = BuildingIIVValue;
            building.ConstructionLimit = ConstructionLimit;
            building.CapitalAdditions = CapitalAdditions;
            building.CreditConstruction = CreditConstruction;
            building.CreditCapAdds = CreditCapAdds;
            building.DomesticUnits = DomesticUnits;
            building.BuildingFRVValue = BuildingFRVValue;
            building.BuildingFRIVValue = BuildingFRIVValue;
            building.ConstructionType = ConstructionType;
            building.ConstructionYear = ConstructionYear;
            building.ResidentialProportion = ResidentialProportion;
            building.NumOfUnits = NumOfUnits;
            building.NumberOfStoreys = NumberOfStoreys;
            building.HasSprinklers = HasSprinklers;
            building.NZS4541Compliant = NZS4541Compliant;
            building.HasHoseReels = HasHoseReels;
            building.HasFireExtinguishers = HasFireExtinguishers;
            building.IsTownWaterSupplied = IsTownWaterSupplied;
            building.IsHalfOrMoreUnOccupied = IsHalfOrMoreUnOccupied;
            building.HasAlarm = HasAlarm;
            building.HasSecurityGuard = HasSecurityGuard;
            building.HasFlammableLiquidsOrGases = HasFlammableLiquidsOrGases;
            building.FlammableLiquidsOrGasesDesc = FlammableLiquidsOrGasesDesc;
            building.HasNBSExceed = HasNBSExceed;
            building.HasSafe = HasSafe;
            building.HasSafeAlarm = HasSafeAlarm;
            building.HasSafeBolted = HasSafeBolted;
            building.BuildingNotes = BuildingNotes;
            building.BuildingLatitude = BuildingLatitude;
            building.BuildingLongitude = BuildingLongitude;
            building.BuildingApproved = BuildingApproved;
            building.BuildingCategory = BuildingCategory;
            return building;
        }

        public static BuildingViewModel FromEntity(Building building)
        {
            BuildingViewModel model = new BuildingViewModel
            {
                BuildingId = building.Id,
                BuildingName = building.BuildingName,
                OwnerOrTenant = building.OwnerOrTenant,
                BuildingLastValuationDate = (building.BuildingLastValuationDate > DateTime.MinValue) ? building.BuildingLastValuationDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")) : "",
                BuildingRVValue = building.BuildingRVValue,
                BuildingRIVValue = building.BuildingRIVValue,
                BuildingDValue = building.BuildingDValue,
                BuildingIVValue = building.BuildingIVValue,
                BuildingIIVValue = building.BuildingIIVValue,
                ConstructionLimit = building.ConstructionLimit,
                CapitalAdditions = building.CapitalAdditions,
                CreditConstruction = building.CreditConstruction,
                CreditCapAdds = building.CreditCapAdds,
                DomesticUnits = building.DomesticUnits,
                BuildingFRVValue = building.BuildingFRVValue,
                BuildingFRIVValue = building.BuildingFRIVValue,
                ConstructionType = building.ConstructionType,
                ConstructionYear = building.ConstructionYear,
                ResidentialProportion = building.ResidentialProportion,
                NumOfUnits = building.NumOfUnits,
                NumberOfStoreys = building.NumberOfStoreys,
                HasSprinklers = building.HasSprinklers,
                NZS4541Compliant = building.NZS4541Compliant,
                HasHoseReels = building.HasHoseReels,
                HasFireExtinguishers = building.HasFireExtinguishers,
                IsTownWaterSupplied = building.IsTownWaterSupplied,
                IsHalfOrMoreUnOccupied = building.IsHalfOrMoreUnOccupied,
                HasAlarm = building.HasAlarm,
                HasSecurityGuard = building.HasSecurityGuard,
                HasFlammableLiquidsOrGases = building.HasFlammableLiquidsOrGases,
                FlammableLiquidsOrGasesDesc = building.FlammableLiquidsOrGasesDesc,
                HasNBSExceed = building.HasNBSExceed,
                HasSafe = building.HasSafe,
                HasSafeAlarm = building.HasSafeAlarm,
                HasSafeBolted = building.HasSafeBolted,
                BuildingNotes = building.BuildingNotes,
                BuildingLatitude = building.BuildingLatitude,
                BuildingLongitude = building.BuildingLongitude,
                BuildingApproved = building.BuildingApproved,
                BuildingCategory = building.BuildingCategory,
              //  Locationst = building.Location,
                
        };
            if (building.Location != null)
            {
                model.BuildingLocation = building.Location.Id;
                model.LocationStreet = building.Location.Street;
            }

            return model;
        }
    }

}
