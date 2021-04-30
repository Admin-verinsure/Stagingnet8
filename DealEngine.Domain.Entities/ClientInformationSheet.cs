using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities.Abstracts;
using System.Linq;
using Newtonsoft.Json;
using AutoMapper;
using AutoMapper.Configuration.Annotations;

namespace DealEngine.Domain.Entities
{
    [JsonObject]
    public class ClientInformationSheet : EntityBase, IAggregateRoot
    {
        public virtual Organisation Owner { get; set; }        
        public virtual ClientProgramme Programme { get; set; }
		[Obsolete ("No longer required with the new Programme implementation")]        
        public virtual ClientAgreement ClientAgreement { get; set; }       
        public virtual IList<ClientInformationAnswer> Answers { get; set; }
		public virtual IList<Vehicle> Vehicles { get; protected set; }
        public virtual IList<Building> Buildings { get; protected set; }
        public virtual IList<BusinessInterruption> BusinessInterruptions { get; set; }
        public virtual IList<MaterialDamage> MaterialDamages { get; set; }        
        public virtual IList<ClaimNotification> ClaimNotifications { get; protected set; }
        public virtual IList<Location> Locations { get; set; }
        public virtual IList<Job> Jobs { get; set; }
        public virtual IList<WaterLocation> WaterLocations { get; set; }
        public virtual IList<Boat> Boats { get; protected set; }
        public virtual IList<Organisation> Organisation { get; set; }
		public virtual RoleData RoleData { get; set; }        
        public virtual IList<SubClientInformationSheet> SubClientInformationSheets { get; set; }
        public virtual RevenueData RevenueData { get; set; }
        //Not Started; Started; Submitted; Bound and pending payment; Bound and invoice pending; Bound and invoiced; Bound; Not Taken Up        
        public virtual string Status { get; set; }        
        public virtual string ReferenceId { get; set; }        
        public virtual ClientInformationSheet PreviousInformationSheet { get; set; }        
        public virtual ClientInformationSheet NextInformationSheet { get; set; }
		public virtual bool IsRenewawl { get; set; }
        public virtual bool IsChange { get; set; }
        public virtual string SheetReference { get; set; }
        public virtual DateTime SubmitDate { get; set; }
		public virtual User SubmittedBy { get; set; }
        public virtual DateTime UnlockDate { get; set; }
        public virtual User UnlockedBy { get; set; }
        public virtual ClientInformationSheet RenewFromInformationSheet { get; set; }
        
        [JsonIgnore]
        public virtual IList<AuditLog> ClientInformationSheetAuditLogs { get; set; }

        public virtual void submitted(User user)
        {
            Status = "Submitted";
            SubmitDate = DateTime.Now;
            SubmittedBy = user;
        }

        public virtual IList<BusinessContract> BusinessContracts { get; set; }
        public virtual IList<PreRenewOrRefData> PreRenewOrRefDatas { get; set; }
       
        protected ClientInformationSheet () : this (null) { }
        public virtual IList<ResearchHouse> ResearchHouses { get; set; }
        protected ClientInformationSheet (User createdBy)
			: base (createdBy)
		{
            SubClientInformationSheets = new List<SubClientInformationSheet>();
            Organisation = new List<Organisation>();
            Answers = new List<ClientInformationAnswer> ();
			Vehicles = new List<Vehicle> ();
			Locations = new List<Location> ();
            Jobs = new List<Job>();
            WaterLocations = new List<WaterLocation>();
            Buildings = new List<Building>();
            Boats = new List<Boat>();
            ClaimNotifications = new List<ClaimNotification>();
            ClientInformationSheetAuditLogs = new List<AuditLog>();
            PreRenewOrRefDatas = new List<PreRenewOrRefData>();
            BusinessContracts = new List<BusinessContract>();
            Status = "Not Started";
            RevenueData = new RevenueData(null, createdBy);
            RoleData = new RoleData(null, createdBy);
        }

		public ClientInformationSheet (User createdBy, Organisation createdFor, InformationTemplate informationTemplate)
			: this (createdBy)
		{
			//OwnerId = user.GetPersonalOrganisation ().Id;
			Owner = createdFor;	// For now
			//InformationTemplate = informationTemplate;
		}

        public ClientInformationSheet(User createdBy, Organisation createdFor, InformationTemplate informationTemplate, string referenceId)
            : this(createdBy)
        {
            //OwnerId = user.GetPersonalOrganisation ().Id;
            Owner = createdFor; // For now
                                //InformationTemplate = informationTemplate;
            ReferenceId = referenceId;
        }

        public ClientInformationSheet (User createdBy, ClientInformationSheet originalSheet, bool renewal)
			: this (createdBy)
		{
			Owner = originalSheet.Owner;
			//InformationTemplate = originalSheet.InformationTemplate;
			PreviousInformationSheet = originalSheet;
			IsRenewawl = renewal;
		}       

        public virtual void AddAnswer(string itemName, string value)
		{
			// no value entered for that question? we won't bother saving it
			if (string.IsNullOrWhiteSpace (value))
				return;

			ClientInformationAnswer answer = Answers.FirstOrDefault (a => a.ItemName == itemName);
			if (answer != null)
				answer.Value = value;
			else
				Answers.Add (new ClientInformationAnswer (CreatedBy, itemName, value));
		}


        public virtual void AddVehicle(Vehicle vehicle)
        {
            Vehicles.Add(vehicle);
        }

        public virtual void AddBoat(Boat boat)
        {
            Boats.Add(boat);
        }

        public virtual void AddClaim(ClaimNotification claim)
        {
            ClaimNotifications.Add(claim);
        }

        public virtual void AddBuilding(Building building)
        {
            Buildings.Add(building);
        }

        public virtual void AddWaterLocation(WaterLocation waterLocation)
        {
            WaterLocations.Add(waterLocation);
        }

        public virtual void AddLocation(Location location)
        {
            Locations.Add(location);
        }

        public virtual void AddJob(Job Job)
        {
            Jobs.Add(Job);
        }



        public virtual ClientInformationSheet CloneForUpdate (User cloningUser, IMapper mapper)
		{
			ClientInformationSheet newSheet = new ClientInformationSheet (cloningUser, Owner, null);
            try {
                newSheet = mapper.Map<ClientInformationSheet>(this);

                //newSheet = mapper.Map<ClientInformationSheet>(this);
                newSheet.PreviousInformationSheet = this;
                NextInformationSheet = newSheet;
                //newSheet.Product = Product;


                //foreach (ClientInformationAnswer answer in Answers)
                //    newSheet.Answers.Add(answer.CloneForNewSheet(newSheet));

                //foreach (Location location in Locations)
                //    newSheet.AddLocation(location.CloneForNewSheet(newSheet));

                //foreach (Building building in Buildings.Where(bui => !bui.Removed && bui.DateDeleted == null))
                //    newSheet.AddBuilding(building.CloneForNewSheet(newSheet));

                //foreach (Vehicle vehicle in Vehicles.Where(v => !v.Removed && v.DateDeleted == null))
                //    newSheet.AddVehicle(vehicle.CloneForNewSheet(newSheet));

                //foreach (Boat boat in Boats.Where(b => !b.Removed && b.DateDeleted == null))
                //    newSheet.AddBoat(boat.CloneForNewSheet(newSheet));

                //foreach (ClaimNotification claim in ClaimNotifications.Where(cl => !cl.Removed && cl.DateDeleted == null))
                //    newSheet.AddClaim(claim.CloneForNewSheet(newSheet));

                //foreach (Organisation org in Organisation.Where(cl => !cl.Removed && cl.DateDeleted == null))
                //    newSheet.Organisation.Add(org);

                //// foreach (RoleData role in Organisation.Where(cl => !cl.Removed && cl.DateDeleted == null))
                //newSheet.RoleData = RoleData;
                //newSheet.RevenueData = RevenueData;

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return newSheet;
		}

		public virtual ClientInformationSheet CloneForRenewal (User renewingUser, IMapper mapper)
		{
			ClientInformationSheet sheet = CloneForUpdate(renewingUser, mapper);
			sheet.IsRenewawl = true;            
			return sheet;
		}

		public virtual Product Product { get; set; }
	}

    public class RevenueData : EntityBase
    {
        protected RevenueData() 
            : this (null)
        {
            Territories = new List<Territory>();
            Activities = new List<BusinessActivity>();
            AdditionalActivityInformation = new AdditionalActivityInformation(null);
        }

        public RevenueData(ClientInformationSheet sheet = null, User user = null) 
            : base(user) 
        {
            Territories = new List<Territory>();
            Activities = new List<BusinessActivity>();
            AdditionalActivityInformation = new AdditionalActivityInformation(user);

            if (sheet != null)
            {
                Territories = CreateTerritories(sheet);
                Activities = CreateActivities(sheet);
            }                       
        }

        private IList<Territory> CreateTerritories(ClientInformationSheet sheet)
        {
            if(sheet.RevenueData != null)
            {
                foreach (var territory in sheet.RevenueData.Territories)
                {
                    Territories.Add(new Territory(null)
                    {
                        TemplateId = territory.TemplateId,
                        Location = territory.Location,
                        Percentage = 0,
                        Selected = false
                        //Percentage = territory.Percentage,
                        //Selected = territory.Selected                    
                    });
                }
            }
            if(Territories.Count != sheet.Programme.BaseProgramme.TerritoryTemplates.Count)
            {
                foreach (var template in sheet.Programme.BaseProgramme.TerritoryTemplates)
                {
                    var containsTerritory = Territories.Where(t => t.TemplateId == template.Id).ToList();
                    if (containsTerritory.Count == 0)
                    {
                        Territories.Add(new Territory(null)
                        {
                            TemplateId = template.Id,
                            Location = template.Location,
                            Percentage = 0,
                            Selected = false
                        });
                    }
                }
            }
            
            return Territories;
        }
        private IList<BusinessActivity> CreateActivities(ClientInformationSheet sheet)
        {
            if (sheet.RevenueData != null)
            {
                foreach (var activity in sheet.RevenueData.Activities)
                {
                    Activities.Add(new BusinessActivity(null)
                    {
                        Description = activity.Description,
                        AnzsciCode = activity.AnzsciCode,
                        Percentage = 0,
                        Selected = false
                        //Selected = activity.Selected,
                        //Percentage = activity.Percentage,                        
                    });
                }
            }
            if (Activities.Count != sheet.Programme.BaseProgramme.BusinessActivityTemplates.Count)
            {
                foreach (var template in sheet.Programme.BaseProgramme.BusinessActivityTemplates)
                {
                    var containsTerritory = Activities.Where(t => t.AnzsciCode == template.AnzsciCode).ToList();
                    if (containsTerritory.Count == 0)
                    {
                        Activities.Add(new BusinessActivity(null)
                        {
                            Description = template.Description,
                            AnzsciCode = template.AnzsciCode,
                            Selected = false,
                            Percentage = 0
                        });
                    }
                }
            }
            return Activities;
        }

        public virtual IList<Territory> Territories { get; set; }
        public virtual IList<BusinessActivity> Activities { get; set; }
        public virtual decimal NextFinancialYearTotal { get; set; }
        public virtual decimal CurrentYearTotal { get; set; }
        public virtual decimal LastFinancialYearTotal { get; set; }
        public virtual AdditionalActivityInformation AdditionalActivityInformation { get; set; }

        public virtual RevenueData CloneForNewSheet(ClientInformationSheet newSheet)
        {
            RevenueData newRevenueData = new RevenueData();
            newRevenueData.Territories = Territories.ToList();
            newRevenueData.Activities = Activities.ToList();
            newRevenueData.NextFinancialYearTotal = NextFinancialYearTotal;
            newRevenueData.CurrentYearTotal = CurrentYearTotal;
            newRevenueData.LastFinancialYearTotal = LastFinancialYearTotal;
            newRevenueData.AdditionalActivityInformation = AdditionalActivityInformation;
            return newRevenueData;
        }
    }

    public class AdditionalActivityInformation : EntityBase
    {
        protected AdditionalActivityInformation() : this(null) { }
        public AdditionalActivityInformation(User createdBy = null) : base(createdBy)
        {
        }

        public virtual string HasInspectionReportOptions { get; set; }
        public virtual string HasDisclaimerReportsOptions { get; set; }
        public virtual string HasObservationServicesOptions { get; set; }
        public virtual string HasRecommendedCladdingOptions { get; set; }
        public virtual string HasStateSchoolOptions { get; set; }
        public virtual string HasIssuedCertificatesOptions { get; set; }
        public virtual string QualificationDetails { get; set; }
        public virtual string ValuationDetails { get; set; }
        public virtual string OtherDetails { get; set; }
        public virtual string OtherServices { get; set; }        
        public virtual string RebuildDetails { get; set; }
        public virtual string InspectionReportDetails { get; set; }
        public virtual string OtherProjectManagementDetails { get; set; }
        public virtual string NonProjectManagementDetails { get; set; }
        public virtual decimal ConstructionCommercialDetails { get; set; }
        public virtual decimal ConstructionDwellingDetails { get; set; }
        public virtual decimal ConstructionIndustrialDetails { get; set; }
        public virtual decimal ConstructionInfrastructureDetails { get; set; }
        public virtual decimal ConstructionSchoolDetails { get; set; }
        public virtual string ConstructionEngineerDetails { get; set; }
    }

    public class SubClientInformationSheet : ClientInformationSheet
    {
        public virtual ClientInformationSheet BaseClientInformationSheet { get; set; }
        public SubClientInformationSheet() { }

        public virtual SubClientInformationSheet CloneForNewSheet(ClientInformationSheet newSheet)
        {
            SubClientInformationSheet newSubClientInformationSheet = new SubClientInformationSheet();
            newSubClientInformationSheet.Answers = Answers.ToList();
            newSubClientInformationSheet.BaseClientInformationSheet = newSheet;
            newSubClientInformationSheet.Boats = Boats.ToList();
            newSubClientInformationSheet.Buildings = Buildings.ToList();
            newSubClientInformationSheet.BusinessContracts = BusinessContracts.ToList();
            newSubClientInformationSheet.BusinessInterruptions = BusinessInterruptions.ToList();
            newSubClientInformationSheet.ClaimNotifications = ClaimNotifications.ToList();
            newSubClientInformationSheet.ClientInformationSheetAuditLogs = ClientInformationSheetAuditLogs.ToList();
            newSubClientInformationSheet.CreatedBy = newSheet.CreatedBy;
            newSubClientInformationSheet.DateCreated = DateTime.Now;
            newSubClientInformationSheet.IsChange = IsChange;
            newSubClientInformationSheet.IsRenewawl = IsRenewawl;
            newSubClientInformationSheet.Locations = Locations.ToList();
            newSubClientInformationSheet.MaterialDamages = MaterialDamages.ToList();
            newSubClientInformationSheet.Organisation = Organisation.ToList();
            newSubClientInformationSheet.Owner = Owner;
            newSubClientInformationSheet.PreRenewOrRefDatas = PreRenewOrRefDatas.ToList();
            newSubClientInformationSheet.PreviousInformationSheet = newSheet.PreviousInformationSheet;
            newSubClientInformationSheet.Product = Product;
            newSubClientInformationSheet.Programme = Programme;
            newSubClientInformationSheet.ReferenceId = ReferenceId;
            newSubClientInformationSheet.ResearchHouses = ResearchHouses.ToList();
            newSubClientInformationSheet.RevenueData = RevenueData;
            newSubClientInformationSheet.RoleData = RoleData;
            newSubClientInformationSheet.SheetReference = SheetReference;
            newSubClientInformationSheet.Status = Status;
            newSubClientInformationSheet.Vehicles = Vehicles.ToList();
            return newSubClientInformationSheet;
        }


    }
}

