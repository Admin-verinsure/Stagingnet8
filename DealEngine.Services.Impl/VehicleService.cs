using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;
using ServiceStack;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using AutoMapper;

namespace DealEngine.Services.Impl
{
	public class VehicleService : IVehicleService
	{

        string _apiEndpoint;
		string _apiKey;
        IAppSettingService _appSettingService;
		IMapperSession<Vehicle> _vehicleRepository;
		ISerializerationService _serializerationService;
		IMapper _mapper;

		public VehicleService (IAppSettingService appSettingService, IMapper mapper, ISerializerationService serializerationService, IMapperSession<Vehicle> vehicleRepository)
		{
			_vehicleRepository = vehicleRepository;
			_appSettingService = appSettingService;
            _apiEndpoint = _appSettingService.CarJamEndpoint;
            _apiKey = _appSettingService.CarJamApiKey;
			_serializerationService = serializerationService;
			_mapper = mapper;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

        [Obsolete]
        public Vehicle GetValidatedVehicle (string plate)
		{
			if (string.IsNullOrWhiteSpace (_apiEndpoint) || string.IsNullOrWhiteSpace (_apiKey))
				throw new Exception (string.Format ("{0} and/or {1} are not set for CarJam API", nameof(_apiEndpoint), nameof(_apiKey)));

			Vehicle vehicle = null;

			ServicePointManager.ServerCertificateValidationCallback += MyRemoteCertificateValidationCallback;
			try
			{

			XmlServiceClient client = new XmlServiceClient (_apiEndpoint);
			var http = client.Get ("car/?plate=" + plate + "&key=" + _apiKey + "&translate=1");
			// load the xml into a XDocument for reading
			XDocument doc = XDocument.Load (http.GetResponseStream ());
			if (doc != null) {
				if (doc.Root.Name == "message") {
					XElement details = doc.Root.Element ("idh").Element ("vehicle");
					if (details != null) {
						vehicle = new Vehicle (null, plate, details.Element ("make").Value, details.Element ("model").Value);
						vehicle.Validated = true;
						vehicle.Year = details.Element ("year_of_manufacture").Value;
						vehicle.VIN = details.Element ("vin").Value;
						vehicle.ChassisNumber = details.Element ("chassis").Value;
						vehicle.EngineNumber = details.Element ("engine_no").Value;
                        vehicle.GrossVehicleMass = Convert.ToInt32(details.Element("gross_vehicle_mass").Value);
					}
				}
				else if (doc.Root.Name == "error")
					throw new Exception (doc.Root.Element ("message").Value);
			}
			ServicePointManager.ServerCertificateValidationCallback -= MyRemoteCertificateValidationCallback;

			}
			catch (Exception ex)
			{

			}
			return vehicle;
		}

		public async Task<Vehicle> GetVehicleById(Guid vehicleId)
		{
			return await _vehicleRepository.GetByIdAsync(vehicleId);
		}

		public Vehicle CreateNewVehicle(User Creator,  string Registration, string Make, string VehicleModel)
		{
			Vehicle vehicle = new Vehicle(Creator, Registration, Make, VehicleModel);
			return vehicle;
		}

		public async Task<Vehicle> PostVehicle(IFormCollection collection, Vehicle vehicle)
        {
			 vehicle = await UpdateVehicle(collection, vehicle);
			 return vehicle;
		}

		public  async Task<Vehicle> UpdateVehicle(IFormCollection collection, Vehicle vehicle)
        {
			var jsonVehicle = (Vehicle) await _serializerationService.GetDeserializedObject(typeof(Vehicle), collection);
			vehicle = _mapper.Map(jsonVehicle, vehicle);
			return vehicle;

		}



		public bool MyRemoteCertificateValidationCallback (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			// from http://stackoverflow.com/questions/4926676/mono-https-webrequest-fails-with-the-authentication-or-decryption-has-failed
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None) {
				for (int i = 0; i < chain.ChainStatus.Length; i++) {
					if (chain.ChainStatus [i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build ((X509Certificate2)certificate);
						if (!chainIsValid) {
							isOk = false;
						}
					}
				}
			}
			return isOk;
		}
	}
}

