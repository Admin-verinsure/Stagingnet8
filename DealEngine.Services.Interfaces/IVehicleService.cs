using System;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Services.Interfaces
{
	public interface IVehicleService
	{
		Vehicle GetValidatedVehicle (string plate);
        Task<Vehicle> GetVehicleById(Guid vehicleId);
		Vehicle CreateNewVehicle (User Creator, string Registration, string Make, string VehicleModel);
		Task<Vehicle> PostVehicle(IFormCollection collection, Vehicle organistaion);
	}
}

