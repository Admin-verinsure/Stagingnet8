using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;
using Newtonsoft.Json;

namespace DealEngine.Domain.Entities
{
	public class Vehicle : EntityBase, IAggregateRoot
	{
		public Vehicle () : base (null) { }

		public Vehicle (User createdBy, string registration, string make, string model)
			: base (createdBy)
		{
			Registration = registration;
			Make = make;
			Model = model;
		}

		public virtual Vehicle OriginalVehicle {
			get;
			set;
		}
		[JsonIgnore]
		public virtual ClientInformationSheet ClientInformationSheet {
			get;
			set;
		}

		public virtual string Registration {
			get;
			set;
		}

		public virtual string Make {
			get;
			set;
		}

		public virtual string Model {
			get;
			set;
		}

		public virtual string Year {
			get;
			set;
		}

		public virtual string VIN {
			get;
			set;
		}

		public virtual string ChassisNumber {
			get;
			set;
		}

		public virtual string EngineNumber {
			get;
			set;
		}

		public virtual int GroupSumInsured {
			get;
			set;
		}

		public virtual string FleetNumber {
			get;
			set;
		}

		public virtual string SerialNumber {
			get;
			set;
		}

		// change to string?
		public virtual int GrossVehicleMass {
			get;
			set;
		}

		public virtual string CCGVM {
			get;
			set;
		}

		public virtual int AreaOfOperation {
			get;
			set;
		}
		//MeiS
		public virtual string AreaOperation
		{
			get;
			set;
		}
		public virtual int VehicleType {
			get;
			set;
		}
		/// <summary>
		/// meis
		/// </summary>
		public virtual string TypeOfVehicle
		{
			get;
			set;
		}
		public virtual int UseType {
			get;
			set;
		}

		public virtual int SubUseType {
			get;
			set;
		}

		public virtual IList<Organisation> InterestedParties {
			get;
			set;
		}

		public virtual string Notes {
			get;
			set;
		}

		public virtual bool Validated {
			get;
			set;
		}

		public virtual Location GarageLocation {
			get;
			set;
		}

		public virtual bool Removed {
			get;
			set;
		}

		public virtual string VehicleCategory {
			get;
			set;
		}

		public virtual DateTime VehicleEffectiveDate {
			get;
			set;
		}

		public virtual DateTime VehicleCeaseDate {
			get;
			set;
		}

		public virtual int VehicleCeaseReason {
			get;
			set;
		}

		public virtual DateTime VehicleInceptionDate {
			get;
			set;
		}

		public virtual DateTime VehicleExpireDate {
			get;
			set;
		}

		public virtual string TypeOfCover
		{
			get;
			set;
		}



		#region Unused
		public virtual bool DetailsUnknown {
			get;
			protected set;
		}

		public virtual string BodyStyle {
			get;
			protected set;
		}

		public virtual string BodyType {
			get;
			protected set;
		}

		public virtual int NumberOfAxles {
			get;
			protected set;
		}

		public virtual string VehicleTypeAsString {
			get;
			protected set;
		}

		public virtual string Grade {
			get;
			protected set;
		}

		//public virtual string TypeOfCover {
		//	get;
		//	protected set;
		//}
		
		public virtual string hasdirectagencies
		{
			get;
			protected set;
		}
		public virtual bool LossOfUseRentalCosts {
			get;
			protected set;
		}

		public virtual string LossOfUseRentalCostsMaximumDayWeekLimit {
			get;
			protected set;
		}

		public virtual string LOURentalCostsMaxPeriodofVehicleLossInWeeks {
			get;
			protected set;
		}

		public virtual string LOURentalCostsDeductible {
			get;
			protected set;
		}


		#endregion

		public virtual Vehicle CloneForNewSheet (ClientInformationSheet newSheet)
		{		
			Vehicle newVehicle = new Vehicle (newSheet.CreatedBy, Registration, Make, Model);
			newVehicle.OriginalVehicle = this;
			newVehicle.Year = Year;
			newVehicle.VIN = VIN;
			newVehicle.ChassisNumber = ChassisNumber;
			newVehicle.EngineNumber = EngineNumber;
			newVehicle.GroupSumInsured = GroupSumInsured;
			newVehicle.FleetNumber = FleetNumber;
			newVehicle.SerialNumber = SerialNumber;
			newVehicle.GrossVehicleMass = GrossVehicleMass;
			newVehicle.CCGVM = CCGVM;
			newVehicle.AreaOfOperation = AreaOfOperation;
			newVehicle.VehicleType = VehicleType;
			newVehicle.UseType = UseType;
			newVehicle.SubUseType = SubUseType;
			newVehicle.InterestedParties = InterestedParties.ToList();
			newVehicle.Notes = Notes;
			newVehicle.Validated = Validated;
			newVehicle.GarageLocation = GarageLocation;
			if (VehicleEffectiveDate > DateTime.MinValue)
				newVehicle.VehicleEffectiveDate = VehicleEffectiveDate;
			if (VehicleCeaseDate > DateTime.MinValue)
				newVehicle.VehicleCeaseDate = VehicleCeaseDate;
			newVehicle.VehicleCeaseReason = VehicleCeaseReason;
			if (VehicleInceptionDate > DateTime.MinValue)
				newVehicle.VehicleInceptionDate = VehicleInceptionDate;
			if (VehicleExpireDate > DateTime.MinValue)
				newVehicle.VehicleExpireDate = VehicleExpireDate;
			return newVehicle;
		}
	}
}
