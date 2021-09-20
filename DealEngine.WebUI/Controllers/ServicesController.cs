

using System;
using System.Collections.Generic;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.ControlModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceStack;
using System.Threading;
using System.Threading.Tasks;
using DealEngine.WebUI.Helpers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Dynamic;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using DealEngine.Infrastructure.Ldap.Interfaces;
using System.Text.Json;

namespace DealEngine.WebUI.Controllers
{
    //[Authorize]
    public class ServicesController : BaseController
    {
        IAuthenticationService _authenticationService;
        IClientInformationService _clientInformationService;
        IClientAgreementService _clientAgreementService;
        IOrganisationalUnitService _organisationalUnitService;
        ILocationService _locationService;
        IJobService _jobService;
        IMapperSession<WaterLocation> _waterLocationRepository;
        IMapperSession<Boat> _boatRepository;
        IMapperSession<Vehicle> _vehicleRepository;

        IBoatUseService _boatUseService;
        IVehicleService _vehicleService;
        IOrganisationService _organisationService;
        IMapperSession<Building> _buildingRepository;
        IMapperSession<BusinessInterruption> _businessInterruptionRepository;
        IMapperSession<MaterialDamage> _materialDamageRepository;
        IClaimNotificationService _claimNotificationService;
        IProductService _productService;
        ISerializerationService _serializerationService;
        IProgrammeService _programmeService;
        IOrganisationTypeService _organisationTypeService;
        IUnitOfWork _unitOfWork;
        IReferenceService _referenceService;
        IEmailService _emailService;
        IAppSettingService _appSettingService;
        IBusinessContractService _businessContractService;
        IResearchHouseService _researchHouseService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<ServicesController> _logger;
        IMapper _mapper;
        Infrastructure.Ldap.Interfaces.ILdapService _ldapService;


        public ServicesController(
            ILdapService ldapService,
            IAuthenticationService authenticationService,
            IMapper mapper,
            ILogger<ServicesController> logger,
            ISerializerationService serializerationService,
            IApplicationLoggingService applicationLoggingService,
            IBusinessContractService businessContractService,
            IUserService userService,
            IClientAgreementService clientAgreementService,
            IAppSettingService appSettingService,
            IClientInformationService clientInformationService,
            IOrganisationalUnitService organisationalUnitService,
            ILocationService locationService,
            IJobService jobService,
            IMapperSession<WaterLocation> waterLocationRepository,
            IMapperSession<Vehicle> vehicleRepository,
            IMapperSession<Building> buildingRepository,
            IMapperSession<BusinessInterruption> businessInterruptionRepository,
            IMapperSession<MaterialDamage> materialDamageRepository,
            IClaimNotificationService claimNotificationService,
            IProductService productService,
            IVehicleService vehicleService,
            IMapperSession<Boat> boatRepository,
            IOrganisationService organisationService,
            IBoatUseService boatUseService,
            IProgrammeService programeService,
            IOrganisationTypeService organisationTypeService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IReferenceService referenceService,
            IResearchHouseService researchHouseService
            )

            : base(userService)
        {
            _ldapService = ldapService;
            _serializerationService = serializerationService;
            _authenticationService = authenticationService;
            _mapper = mapper;
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;
            _clientAgreementService = clientAgreementService;
            _appSettingService = appSettingService;
            _clientInformationService = clientInformationService;
            _organisationalUnitService = organisationalUnitService;
            _vehicleService = vehicleService;
            _locationService = locationService;
            _jobService = jobService;
            _waterLocationRepository = waterLocationRepository;
            _boatRepository = boatRepository;
            _organisationService = organisationService;
            _boatUseService = boatUseService;
            _buildingRepository = buildingRepository;
            _businessInterruptionRepository = businessInterruptionRepository;
            _materialDamageRepository = materialDamageRepository;
            _claimNotificationService = claimNotificationService;
            _productService = productService;
            _programmeService = programeService;
            _organisationTypeService = organisationTypeService;
            _unitOfWork = unitOfWork;
            _referenceService = referenceService;
            _emailService = emailService;
            _businessContractService = businessContractService;
            _researchHouseService = researchHouseService;
            _vehicleRepository = vehicleRepository;
        }

        #region Vehicle

        [HttpPost]
        public async Task<IActionResult> GetClient1(IFormCollection Collection , Guid ProgId)
        {
            User user = null;
            try
            {
                Guid.TryParse(Collection["Id"], out Guid OrganisationId);
                Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
                //EditClientsViewModel EditClientsViewModel = new EditClientsViewModel();
                Programme programme = await _programmeService.GetProgrammeById(ProgId);
                EditClientsViewModel model = new EditClientsViewModel(programme);

                if (organisation != null)
                {
                    var ClientProgrammes = await _programmeService.GetClientProgrammesByOwnerByProgramme(organisation.Id, ProgId);
                    if (ClientProgrammes.Any())
                    {
                        model.ClientProgramme = ClientProgrammes.FirstOrDefault();
                        model.Organisation = organisation;

                    }
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetClient(IFormCollection Collection)
        {
            User user = null;
            Guid.TryParse(Collection["Id"], out Guid OrganisationId);
            Guid.TryParse(Collection["ProgId"], out Guid ProgId);
            Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            try
            {
                
                if (organisation != null)
                {
                    var ClientProgrammes = await _programmeService.GetClientProgrammesByOwnerByProgramme(OrganisationId, ProgId);
                    if (ClientProgrammes.Any())
                    {
                        JsonObjects.Add("Organisation", organisation);
                        JsonObjects.Add("ClientProgramme", ClientProgrammes.OrderByDescending(cpobdc => cpobdc.DateCreated).FirstOrDefault());
                        var jsonObj = await _serializerationService.GetSerializedObject(JsonObjects);
                        //var jsonObj = JsonSerializer.Serialize(JsonObjects);
                        return Json(jsonObj);
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostClient(IFormCollection Collection)
        {
            User user = null;
            try
            {
                Guid.TryParse(Collection["ClientProgramme.Id"], out Guid ClientId);
                ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(ClientId);
                if(clientProgramme != null)
                {
                    clientProgramme.EGlobalClientNumber = Collection["ClientProgramme.EGlobalClientNumber"];
                    clientProgramme.Tier = Collection["ClientProgramme.Tier"];
                    await _programmeService.Update(clientProgramme);
                    User OwnerUser = await _userService.GetUserPrimaryOrganisationOrEmail(clientProgramme.Owner);
                    if(OwnerUser != null)
                    {

                        OwnerUser.Email = Collection["Organisation.Email"];
                        OwnerUser.PrimaryOrganisation.Email = Collection["Organisation.Email"];
                        OwnerUser.PrimaryOrganisation.Name = Collection["Organisation.Name"];
                        await _userService.Update(OwnerUser);
                    }
                }
                return Redirect("../Home/EditClients?ProgrammeId=" + clientProgramme.BaseProgramme.Id.ToString());
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchVehicle(string registration)
        {
            VehicleViewModel model = new VehicleViewModel();
            User user = null;
            try
            {
                user = await CurrentUser();
                model.Registration = registration;
                model.Make = "Not Found";
                model.VehicleModel = "Not Found";
                model.SumInsured = 5000;


                Vehicle vehicle = _vehicleService.GetValidatedVehicle(registration);
                if (vehicle != null)
                {
                    model.Validated = vehicle.Validated;
                    model.Year = vehicle.Year;
                    model.Make = vehicle.Make;
                    model.VehicleModel = vehicle.Model;
                    model.VIN = vehicle.VIN;
                    model.ChassisNumber = vehicle.ChassisNumber;
                    model.EngineNumber = vehicle.EngineNumber;
                    model.GrossVehicleMass = vehicle.GrossVehicleMass.ToString();
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle(VehicleViewModel model)
        {
            User user = null;
            try
            {
                user = await CurrentUser();

                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                {
                    throw new Exception("Unable to save Vehicle - No Client information for " + model.AnswerSheetId);
                }

                // get existing vehicle (if any)
                Vehicle vehicle = await _vehicleService.GetVehicleById(model.VehicleId);
                // no vehicle, so create new
                if (vehicle == null)
                {
                    vehicle = model.ToEntity(user);          
                }
                model.UpdateEntity(vehicle);           
                if (model.VehicleLocation != Guid.Empty)
                {
                    vehicle.GarageLocation = await _locationService.GetLocationById(model.VehicleLocation);
                }
                vehicle.ClientInformationSheet = sheet;
                await _vehicleRepository.AddAsync(vehicle);
                sheet.Vehicles.Add(vehicle);
                model.VehicleId = vehicle.Id;
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetVehicle(Guid answerSheetId, Guid vehicleId)
        {
            VehicleViewModel model = new VehicleViewModel();
            User user = null;
            try
            {
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                Vehicle vehicle = sheet.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
                if (vehicle != null)
                {
                    model = VehicleViewModel.FromEntity(vehicle);
                    model.AnswerSheetId = answerSheetId;

                    if (vehicle.GarageLocation != null)
                        model.VehicleLocation = vehicle.GarageLocation.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles(Guid informationId, bool validated, bool removed, bool transfered, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;
            var vehicles = new List<Vehicle>();

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                if (transfered)
                {
                    vehicles = sheet.Vehicles.Where(v => v.Validated == validated && v.Removed == removed && v.DateDeleted == null && v.VehicleCeaseDate > DateTime.MinValue && v.VehicleCeaseReason == 4).ToList();
                }
                else
                {
                    vehicles = sheet.Vehicles.Where(v => v.Validated == validated && v.Removed == removed && v.DateDeleted == null).ToList();
                }

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            vehicles = vehicles.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            vehicles = vehicles.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            vehicles = vehicles.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //vehicles = vehicles.OrderBy(sidx + " " + sord).ToList();
                vehicles = vehicles.ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = vehicles.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    Vehicle vehicle = vehicles[i];
                    JqGridRow row = new JqGridRow(vehicle.Id);
                    if (vehicle.Validated)
                    {
                        row.AddValues(vehicle.Id, vehicle.Year, vehicle.Registration, vehicle.Make, vehicle.Model, vehicle.GroupSumInsured.ToString("C", UserCulture), vehicle.Id);
                    }
                    else
                    {
                        if (sheet.Programme.BaseProgramme.Products.First().Id == new Guid("e2eae6d8-d68e-4a40-b50a-f200f393777a")) // Marsh Coastguard
                        {
                            row.AddValues(vehicle.Id, vehicle.Year, vehicle.Make, vehicle.Model, vehicle.GroupSumInsured.ToString("C", UserCulture), vehicle.Id);
                        }
                        else
                        {
                            row.AddValues(vehicle.Id, vehicle.Year, vehicle.FleetNumber, vehicle.Make, vehicle.Model, vehicle.GroupSumInsured.ToString("C", UserCulture), vehicle.Id);
                        }

                    }


                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        //[HttpGet]
        //public async Task<IActionResult> GetPrincipalPartners(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
        //                                 string searchField, string searchString, string searchOper, string filters)
        //{
        //    User user = null;
        //    XDocument document = null;
        //    JqGridViewModel model = new JqGridViewModel();

        //    try
        //    {
        //        user = await CurrentUser();
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);

        //        if (sheet == null)
        //            throw new Exception("No valid information for id " + informationId);

        //        var organisations = await _organisationService.GetOrganisationPrincipals(sheet);

        //        if (_search)
        //        {
        //            switch (searchOper)
        //            {
        //                case "eq":
        //                    organisations = organisations.Where(searchField + " = \"" + searchString + "\"").ToList();
        //                    break;
        //                case "bw":
        //                    organisations = organisations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
        //                    break;
        //                case "cn":
        //                    organisations = organisations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
        //                    break;
        //            }
        //        }
        //        //organisations = organisations.OrderBy(sidx + " " + sord).ToList();
        //        model.Page = page;
        //        model.TotalRecords = organisations.Count;
        //        model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;
        //        JqGridRow row1 = new JqGridRow(sheet.Owner.Id);
        //        row1.AddValues(sheet.Owner.Id, sheet.Owner.Name, "Owner");
        //        model.AddRow(row1);
        //        int offset = rows * (page - 1);
        //        for (int i = offset; i < offset + rows; i++)
        //        {
        //            if (i == model.TotalRecords)
        //                break;
        //            Organisation organisation = organisations[i];
        //            JqGridRow row = new JqGridRow(organisation.Id);

        //            for (int x = 0; x < organisation.InsuranceAttributes.Count; x++)
        //            {
        //                row.AddValues(organisation.Id, organisation.Name, organisation.InsuranceAttributes[x].InsuranceAttributeName, organisation.Id);
        //            }
        //            model.AddRow(row);
        //        }


        //        //// convert model to XDocument for rendering.
        //        document = model.ToXml();
        //        return Xml(document);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}


        //[HttpGet]
        //public async Task<IActionResult> GetCommonPrincipalPartners(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
        //                                 string searchField, string searchString, string searchOper, string filters)
        //{
        //    User user = null;
        //    XDocument document = null;
        //    JqGridViewModel model = new JqGridViewModel();

        //    try
        //    {
        //        user = await CurrentUser();
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
        //        var progname = sheet.Programme.BaseProgramme.Name;

               

        //        if (sheet == null)
        //            throw new Exception("No valid information for id " + informationId);

        //        var organisations = await _organisationService.GetOrganisationPrincipals(sheet);

        //        if (_search)
        //        {
        //            switch (searchOper)
        //            {
        //                case "eq":
        //                    organisations = organisations.Where(searchField + " = \"" + searchString + "\"").ToList();
        //                    break;
        //                case "bw":
        //                    organisations = organisations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
        //                    break;
        //                case "cn":
        //                    organisations = organisations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
        //                    break;
        //            }
        //        }
        //        organisations = organisations.OrderByDescending(cp => cp.IsPrincipalAdvisor).ToList();
        //        model.Page = page;
        //        model.TotalRecords = organisations.Count;
        //        model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;
        //        JqGridRow row1 = new JqGridRow(sheet.Owner.Id);
        //        if (progname == "NZFSG Programme"){
        //            row1.AddValues(sheet.Owner.Id, sheet.Owner.Name, "Owner", "false",sheet.Owner.TradingName == null ? " ": sheet.Owner.TradingName);
        //        }else{
        //            row1.AddValues(sheet.Owner.Id, sheet.Owner.Name, "Owner", "false","NonTrading");
        //        }
        //        model.AddRow(row1);
        //        int offset = rows * (page - 1);
        //        for (int i = offset; i < offset + rows; i++)
        //        {
        //            if (i == model.TotalRecords)
        //                break;
        //            Organisation organisation = organisations[i];
        //            JqGridRow row = new JqGridRow(organisation.Id);

        //            if (organisation.InsuranceAttributes.Any())
        //            {
        //                for (int x = 0; x < organisation.InsuranceAttributes.Count; x++)
        //                {
        //                    row.AddValues(organisation.Id, organisation.Name, organisation.InsuranceAttributes[x].InsuranceAttributeName, organisation.IsPrincipalAdvisor, "", organisation.Id);
        //                }
        //            }
        //            else
        //            {
        //                //if(organisation.Type == "Advisor")
        //                //{
        //                //    row.AddValues(organisation.Id, organisation.Name, organisation.Type, organisation.IsPrincipalAdvisor, "", organisation.Id);
        //                //}
        //            }
        //            model.AddRow(row);
        //        }


        //        //// convert model to XDocument for rendering.
        //        document = model.ToXml();
        //        return Xml(document);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetDeletedPartners(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
        //                               string searchField, string searchString, string searchOper, string filters)
        //{
        //    User user = null;

        //    try
        //    {
        //        XDocument document = null;
        //        JqGridViewModel model = new JqGridViewModel();
        //        user = await CurrentUser();
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);

        //        var userList = await _userService.GetAllUsers();
        //        User userdb = userList.FirstOrDefault(user => user.PrimaryOrganisation == sheet.Owner);
        //        if (sheet == null)
        //            throw new Exception("No valid information for id " + informationId);

        //        var organisations = new List<Organisation>();
        //        foreach (var org in sheet.Organisation.Where(o => o.Removed == true))
        //        {
        //            organisations.Add(org);
        //        }

        //        if (_search)
        //        {
        //            switch (searchOper)
        //            {
        //                case "eq":
        //                    organisations = organisations.Where(searchField + " = \"" + searchString + "\"").ToList();
        //                    break;
        //                case "bw":
        //                    organisations = organisations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
        //                    break;
        //                case "cn":
        //                    organisations = organisations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
        //                    break;
        //            }
        //        }
        //        //organisations = organisations.OrderBy(sidx + " " + sord).ToList();
        //        model.Page = page;
        //        model.TotalRecords = organisations.Count;
        //        model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;
        //        int offset = rows * (page - 1);
        //        for (int i = offset; i < offset + rows; i++)
        //        {
        //            if (i == model.TotalRecords)
        //                break;
        //            Organisation organisation = organisations[i];
        //            JqGridRow row = new JqGridRow(organisation.Id);

        //            for (int x = 0; x < organisation.InsuranceAttributes.Count; x++)
        //            {
        //                row.AddValues(organisation.Id, organisation.Name, organisation.InsuranceAttributes[x].InsuranceAttributeName, organisation.Id);
        //            }
        //            model.AddRow(row);
        //        }


        //        //// convert model to XDocument for rendering.
        //        document = model.ToXml();
        //        return Xml(document);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}



        [HttpGet]
        public async Task<IActionResult> GetPminzNamedParties(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;
            XDocument document = null;
            JqGridViewModel model = new JqGridViewModel();

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var organisations = new List<Organisation>();
                foreach (var org in sheet.Organisation.Where(o => o.Removed == removed))
                {
                    organisations.Add(org);
                }


                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            organisations = organisations.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            organisations = organisations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            organisations = organisations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //organisations = organisations.OrderBy(sidx + " " + sord).ToList();
                model.Page = page;
                model.TotalRecords = organisations.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;
                JqGridRow row1 = new JqGridRow(sheet.Owner.Id);
                row1.AddValues(sheet.Owner.Id, sheet.Owner.Name, "Owner");

                model.AddRow(row1);
                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;
                    Organisation organisation = organisations[i];
                    JqGridRow row = new JqGridRow(organisation.Id);

                    for (int x = 0; x < organisation.InsuranceAttributes.Count; x++)
                    {
                        row.AddValues(organisation.Id, organisation.Name, organisation.InsuranceAttributes[x].InsuranceAttributeName, organisation.Id);
                    }
                    model.AddRow(row);
                }

                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetNamedParties(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;
            XDocument document = null;
            JqGridViewModel model = new JqGridViewModel();

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var organisations = new List<Organisation>();
                for (var i = 0; i < sheet.Organisation.Count; i++)
                {
                    organisations.Add(sheet.Organisation.ElementAtOrDefault(i));
                }

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            organisations = organisations.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            organisations = organisations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            organisations = organisations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //organisations = organisations.OrderBy(sidx + " " + sord).ToList();
                model.Page = page;
                model.TotalRecords = organisations.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;
                JqGridRow row1 = new JqGridRow(sheet.Owner.Id);
                row1.AddValues(sheet.Owner.Id, sheet.Owner.Name, "Owner");

                model.AddRow(row1);
                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;
                    Organisation organisation = organisations[i];
                    JqGridRow row = new JqGridRow(organisation.Id);

                    for (int x = 0; x < organisation.InsuranceAttributes.Count; x++)
                    {
                        row.AddValues(organisation.Id, organisation.Name, organisation.InsuranceAttributes[x].InsuranceAttributeName, organisation.Id);
                    }
                    model.AddRow(row);
                }

                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetVehicleRemovedStatus(Guid vehicleId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Vehicle vehicle = await _vehicleService.GetVehicleById(vehicleId);
                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    vehicle.Removed = status;
                    await uow.Commit();
                }
                //return new JsonResult(true);
                return new JsonResult(new { status = true, id = vehicleId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetVehicleCeasedStatus(Guid vehicleId, bool status, DateTime ceaseDate, int ceaseReason)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Vehicle vehicle = await _vehicleService.GetVehicleById(vehicleId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    vehicle.VehicleCeaseDate = DateTime.Parse(LocalizeTime(ceaseDate, "d"));
                    vehicle.VehicleCeaseReason = ceaseReason;
                    await uow.Commit().ConfigureAwait(false);
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetVehicleTransferedStatus(Guid vehicleId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Vehicle vehicle = await _vehicleService.GetVehicleById(vehicleId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    vehicle.VehicleCeaseDate = DateTime.MinValue;
                    vehicle.VehicleCeaseReason = '0';
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RevalidateVehicle(Guid vehicleId)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Vehicle vehicle = await _vehicleService.GetVehicleById(vehicleId);
                if (vehicle == null)
                    throw new Exception("Vehicle is null");
                if (vehicle.Validated == false)
                {
                    if (string.IsNullOrWhiteSpace(vehicle.Registration))
                        throw new Exception("Unable to revalidate a non registered & validated vehicle");
                }

                return null;
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Organisational Units

        [HttpPost]
        public async Task<IActionResult> SearchOrganisationalUnit(Guid answerSheetId, string name)
        {
            OrganisationalUnitViewModel model = new OrganisationalUnitViewModel();
            User user = null;
            try
            {
                user = await CurrentUser();
                model.Name = name;
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                OrganisationalUnit unit = sheet.Owner.OrganisationalUnits.FirstOrDefault(ou => ou.Name == name);
                if (unit != null)
                {
                    model.Name = unit.Name;
                    model.OrganisationId = sheet.Owner.Id;
                    model.OrganisationalUnitId = unit.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganisationalUnit(OrganisationalUnitViewModel model)
        {
            User user = null;

            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Organisational Unit - No Client information for " + model.AnswerSheetId);

                OrganisationalUnit ou = await _organisationalUnitService.GetOrganisationalUnit(model.OrganisationalUnitId);
                if (ou == null)
                    ou = new OrganisationalUnit(user, model.Name);
                ou.Name = model.Name;

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.Owner.OrganisationalUnits.Add(ou);
                    await uow.Commit();
                }
                model.OrganisationalUnitId = ou.Id;

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetOrganisationalUnit(Guid answerSheetId, Guid unitId)
        {
            OrganisationalUnitViewModel model = new OrganisationalUnitViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                OrganisationalUnit unit = sheet.Owner.OrganisationalUnits.FirstOrDefault(ou => ou.Id == unitId);
                if (unit != null)
                {
                    model.Name = unit.Name;
                    model.OrganisationId = sheet.Owner.Id;
                    model.OrganisationalUnitId = unit.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganisationalUnits(Guid informationId, bool _search, string nd, int rows, int page, string sidx, string sord,
                                                     string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var organisationalUnits = sheet.Owner.OrganisationalUnits;

                if (_search)
                {

                    switch (searchOper)
                    {
                        case "eq":
                            organisationalUnits = organisationalUnits.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            organisationalUnits = organisationalUnits.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            organisationalUnits = organisationalUnits.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = 1;
                model.TotalRecords = organisationalUnits.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    OrganisationalUnit ou = organisationalUnits[i];
                    JqGridRow row = new JqGridRow(ou.Id);
                    row.AddValues(ou.Id, ou.Name);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganisationalUnitName(string term)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                var organisationalUnitNameList = await _organisationalUnitService.GetAllOrganisationalUnitsName();
                var results = organisationalUnitNameList.Where(n => n.ToLower().Contains(term.ToLower()));

                return new JsonResult(results.ToArray());
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Locations
        [HttpPost]
        public async Task<IActionResult> LocationTableRefreshAPI(IFormCollection collection)
        {
            User user = null;
            
            var dictionary = new Dictionary<string, string>();
            ClientInformationSheet sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));
            foreach(var location in sheet.Locations)
            {

                //string editbutton = "<button class=\"btn btn-success btn - sm\" onclick=\"EditLocation('"+location.Id+"', '"+location.LocationType+"', '"+ location.CommonName+ "', '"+location.Street+"', '"+ location.City + "', '"+ location.Country + "', '"+ location.Suburb + "', '"+location.Postcode+"')\"><i class=\"fa fa-edit\"></i> Edit</button>";
                
                //string removebutton = 
                
                //dictionary.Add("editButton", editbutton);
                //if (!dictionary.ContainsKey("deleteButton"))
                //    dictionary.Add("deleteButton", removebutton);

                //dictionary["deleteButton"].a(removebutton);
                
                //if (!dictionary.ContainsKey("editButton"))
                //    dictionary.Add("editButton", removebutton);

                //dictionary["editButton"].Add(removebutton);
            }
            return Json(sheet.Locations);
        }

        [HttpPost]
        public async Task<IActionResult> AddLocation(IFormCollection collection)
        {
            User user = null;
            try
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));
                user = await CurrentUser();
                Location location = null;
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));
                var locationForm = collection.Keys.Where(s => s.StartsWith("LocationViewModel", StringComparison.CurrentCulture));
                var id = collection["LocationViewModel.LocationId"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    location = new Location(user);
                }
                else
                {
                    location = await _locationService.GetLocationById(Guid.Parse(id));
                }
                var type = location.GetType();
                foreach (var keyField in locationForm)
                {
                    if (keyField != "LocationViewModel.LocationId")
                    {
                        var propertyName = keyField.Split('.').ToList();
                        var property = type.GetProperty(propertyName.LastOrDefault());
                        property.SetValue(location, collection[keyField].ToString());
                    }
                }

                if (sheet.Locations.Contains(location))
                {
                    await _locationService.UpdateLocation(location);
                }
                else
                {
                    sheet.Locations.Add(location);
                    await _clientInformationService.UpdateInformation(sheet);
                }

                return new JsonResult(location.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveLocation(string locationId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                Location location = await _locationService.GetLocationById(Guid.Parse(locationId));
                location.Removed = true;
                await _locationService.UpdateLocation(location);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> RestoreLocation(string locationId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                Location location = await _locationService.GetLocationById(Guid.Parse(locationId));
                location.Removed = false;
                await _locationService.UpdateLocation(location);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddJob(IFormCollection collection)
        {
            User user = null;
            try
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));
                user = await CurrentUser();
                Job job = null;
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));
                var jobForm = collection.Keys.Where(s => s.StartsWith("JobViewModel", StringComparison.CurrentCulture));
                var id = collection["JobViewModel.JobId"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    job = new Job(user);
                }
                else
                {
                    job = await _jobService.GetJobById(Guid.Parse(id));
                }
                var type = job.GetType();
                foreach (var keyField in jobForm)
                {
                    if (keyField != "JobViewModel.JobId")
                    {
                        var propertyName = keyField.Split('.').ToList();
                        var property = type.GetProperty(propertyName.LastOrDefault());
                        if (propertyName[1] == "IssueDate" ||
                            propertyName[1] == "StartDate" ||
                            propertyName[1] == "EndDate" || 
                            propertyName[1] == "CertRequiredBy")
                        {
                            property.SetValue(job, DateTime.Parse(collection[keyField].ToString(),System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")));
                        }
                        else
                        {
                            property.SetValue(job, collection[keyField].ToString());
                        }
                        
                    }
                }

                if (sheet.Jobs.Contains(job))
                {
                    await _jobService.UpdateJob(job);
                }
                else
                {
                    sheet.Jobs.Add(job);
                    await _clientInformationService.UpdateInformation(sheet);
                }

                return new JsonResult(job.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveJob(string jobId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                Job job = await _jobService.GetJobById(Guid.Parse(jobId));
                job.Removed = true;
                await _jobService.UpdateJob(job);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> SetPrincipalRemovedStatus(Guid answersheetId, Guid principalId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Organisation org = await _organisationService.GetOrganisation(principalId);
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answersheetId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    org.Removed = status;

                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Buildings

        [HttpPost]
        public async Task<IActionResult> AddBuilding(BuildingViewModel model)
        {
            User user = null;

            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Building - No Client information for " + model.AnswerSheetId);

                Building building = _buildingRepository.FindAll().FirstOrDefault(bui => bui.Id == model.BuildingId);
                if (building == null)
                    building = model.ToEntity(user);
                model.UpdateEntity(building);

                if (model.BuildingLocation != null)
                    building.Location = await _locationService.GetLocationById(model.BuildingLocation);

                if (model.InterestedParties != null)
                {
                    var getAllOrganisations = await _organisationService.GetAllOrganisations();
                    building.InterestedParties = getAllOrganisations.Where(org => model.InterestedParties.Contains(org.Id)).ToList();
                }

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.Buildings.Add(building);
                    await uow.Commit();
                }

                model.LocationStreet = building.Location.Street;
                model.BuildingId = building.Id;


                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBuilding(Guid answerSheetId, Guid buildingId)
        {
            BuildingViewModel model = new BuildingViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                Building building = sheet.Buildings.FirstOrDefault(b => b.Id == buildingId);
                if (building != null)
                {
                    model = BuildingViewModel.FromEntity(building);
                    model.AnswerSheetId = answerSheetId;

                    if (building.Location != null)
                        model.BuildingLocation = building.Location.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBuildings(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var buildings = new List<Building>();

                buildings = sheet.Buildings.Where(b => b.Removed == removed && b.DateDeleted == null).ToList();

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            buildings = buildings.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            buildings = buildings.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            buildings = buildings.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //buildings = buildings.OrderBy(sidx + " " + sord).ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = buildings.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    Building building = buildings[i];
                    JqGridRow row = new JqGridRow(building.Id);
                    row.AddValues(building.Id, building.BuildingName, building.BuildingCategory, building.Location.Street, building.Location.Suburb, building.Location.City, building.Id, building.BuildingCategory);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBuildingRemovedStatus(Guid buildingId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Building building = await _buildingRepository.GetByIdAsync(buildingId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    building.Removed = status;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        #endregion

        #region WaterLocations

        //[HttpPost]
        //public async Task<IActionResult> SearchWaterLocationName(Guid answerSheetId, string waterLocationName)
        //{
        //    WaterLocationViewModel model = new WaterLocationViewModel();
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        model.WaterLocationName = waterLocationName;
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
        //        WaterLocation waterLocation = _waterLocationRepository.FindAll().FirstOrDefault(wl => wl.WaterLocationName == waterLocationName);
        //        if (waterLocation != null)
        //        {
        //            model = WaterLocationViewModel.FromEntity(waterLocation);
        //        }
        //        return Json(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> AddWaterLocation(WaterLocationViewModel model)
        {
            User user = null;
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Water Location - No Client information for " + model.AnswerSheetId);

                WaterLocation waterLocation = _waterLocationRepository.FindAll().FirstOrDefault(wloc => wloc.Id == model.WaterLocationId);
                if (waterLocation == null)
                    waterLocation = model.ToEntity(user);
                model.UpdateEntity(waterLocation);

                // waterLocation.OrganisationalUnit = OrganisationalUnit;

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.WaterLocations.Add(waterLocation);
                    await uow.Commit();
                }

                model.WaterLocationId = waterLocation.Id;
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> GetWaterLocation(Guid answerSheetId, Guid waterLocationId)
        //{
        //    WaterLocationViewModel model = new WaterLocationViewModel();
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
        //        WaterLocation waterLocation = sheet.WaterLocations.FirstOrDefault(wloc => wloc.Id == waterLocationId);
        //        if (waterLocation != null)
        //        {
        //            model = WaterLocationViewModel.FromEntity(waterLocation);
        //            model.AnswerSheetId = answerSheetId;

        //            if (waterLocation.WaterLocationLocation != null)
        //                model.WaterLocationLocation = waterLocation.WaterLocationLocation.Id;
        //            if (waterLocation.WaterLocationMarinaLocation != null)
        //                model.WaterLocationMarinaLocation = waterLocation.WaterLocationMarinaLocation.Id;
        //            if (waterLocation.OrganisationalUnit != null)
        //                model.OrganisationalUnit = waterLocation.OrganisationalUnit.Id;


        //        }

        //        Organisation organisation = null;

        //        organisation = await _organisationService.GetOrganisation(waterLocation.WaterLocationMarinaLocation.Id);
        //        var organisationalUnits = new List<OrganisationalUnitViewModel>();

        //        foreach (OrganisationalUnit ou in organisation.OrganisationalUnits)
        //        {
        //            organisationalUnits.Add(new OrganisationalUnitViewModel
        //            {
        //                OrganisationalUnitId = ou.Id,
        //                Name = ou.Name
        //            });
        //        }

        //        model.LOrganisationalUnits = organisationalUnits;

        //        var Locations = new List<LocationViewModel>();

        //        foreach (Location loc in waterLocation.OrganisationalUnit.Locations)
        //        {
        //            model.Locations.Add(loc);
        //        }

        //        return Json(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }

        //}

        //removing this through new bootstrap
        //[HttpGet]
        //public async Task<IActionResult> GetWaterLocations(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
        //                                  string searchField, string searchString, string searchOper, string filters)
        //{
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
        //        if (sheet == null)
        //            throw new Exception("No valid information for id " + informationId);

        //        var waterLocations = new List<WaterLocation>();

        //        waterLocations = sheet.WaterLocations.Where(wl => wl.Removed == removed && wl.DateDeleted == null).ToList();

        //        if (_search)
        //        {
        //            switch (searchOper)
        //            {
        //                case "eq":
        //                    waterLocations = waterLocations.Where(searchField + " = \"" + searchString + "\"").ToList();
        //                    break;
        //                case "bw":
        //                    waterLocations = waterLocations.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
        //                    break;
        //                case "cn":
        //                    waterLocations = waterLocations.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
        //                    break;
        //            }
        //        }
        //        //waterLocations = waterLocations.OrderBy(sidx + " " + sord).ToList();

        //        XDocument document = null;
        //        JqGridViewModel model = new JqGridViewModel();
        //        model.Page = page;
        //        model.TotalRecords = waterLocations.Count;
        //        model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

        //        int offset = rows * (page - 1);
        //        for (int i = offset; i < offset + rows; i++)
        //        {
        //            if (i == model.TotalRecords)
        //                break;

        //            WaterLocation waterLocation = waterLocations[i];
        //            JqGridRow row = new JqGridRow(waterLocation.Id);
        //            if (waterLocation.WaterLocationMarinaLocation != null)
        //            {
        //                row.AddValues(waterLocation.Id, waterLocation.WaterLocationName, waterLocation.WaterLocationMarinaLocation.Name, waterLocation.WaterLocationMooringType, waterLocation.Id);
        //            }
        //            else
        //            {
        //                row.AddValues(waterLocation.Id, waterLocation.WaterLocationName, " ", waterLocation.WaterLocationMooringType, waterLocation.Id);

        //            }
        //            model.AddRow(row);
        //        }

        //        // convert model to XDocument for rendering.
        //        document = model.ToXml();
        //        return Xml(document);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetWaterLocationName(string term)
        //{
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        var waterLocationNameList = _waterLocationRepository.FindAll().Select(wl => wl.WaterLocationName);
        //        var results = waterLocationNameList.Where(n => n.ToLower().Contains(term.ToLower()));
        //        return new JsonResult(results.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> GetWaterLocationList(Guid answerSheetId)
        {
            List<WaterLocationViewModel> models = new List<WaterLocationViewModel>();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                foreach (var waterLocation in sheet.WaterLocations)
                    models.Add(WaterLocationViewModel.FromEntity(waterLocation));

                return new JsonResult(models.ToArray());
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetWaterLocationRemovedStatus(Guid waterLocationId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                WaterLocation waterLocation = await _waterLocationRepository.GetByIdAsync(waterLocationId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    waterLocation.Removed = status;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region BusinessInterruption

        [HttpPost]
        public async Task<IActionResult> AddBusinessInterruption(BusinessInterruptionViewModel model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save BusinessInterruption - No Client information for " + model.AnswerSheetId);

                BusinessInterruption businessInterruption = _businessInterruptionRepository.FindAll().FirstOrDefault(bi => bi.Id == model.BusinessInterruptionId);
                if (businessInterruption == null)
                    businessInterruption = model.ToEntity(user);
                model.UpdateEntity(businessInterruption);

                if (model.BusinessInterruptionLocation != null)
                    businessInterruption.Location = await _locationService.GetLocationById(model.BusinessInterruptionLocation);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.BusinessInterruptions.Add(businessInterruption);
                    await uow.Commit();
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBusinessInterruption(Guid answerSheetId, Guid businessInterruptionId)
        {
            BusinessInterruptionViewModel model = new BusinessInterruptionViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                BusinessInterruption businessInterruption = sheet.BusinessInterruptions.FirstOrDefault(bi => bi.Id == businessInterruptionId);
                if (businessInterruption != null)
                {
                    model = BusinessInterruptionViewModel.FromEntity(businessInterruption);
                    model.AnswerSheetId = answerSheetId;

                    if (businessInterruption.Location != null)
                        model.BusinessInterruptionLocation = businessInterruption.Location.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBusinessInterruptions(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var businessInterruptions = new List<BusinessInterruption>();

                businessInterruptions = sheet.BusinessInterruptions.Where(bi => bi.Removed == removed && bi.DateDeleted == null).ToList();

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            businessInterruptions = businessInterruptions.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            businessInterruptions = businessInterruptions.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            businessInterruptions = businessInterruptions.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //businessInterruptions = businessInterruptions.OrderBy(sidx + " " + sord).ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = businessInterruptions.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    BusinessInterruption businessInterruption = businessInterruptions[i];
                    JqGridRow row = new JqGridRow(businessInterruption.Id);
                    row.AddValues(businessInterruption.IndemnityPeriod, businessInterruption.Location.CommonName, businessInterruption.Location.Street, businessInterruption.Location.Suburb, businessInterruption.Location.Postcode,
                        businessInterruption.Location.City, businessInterruption.Id);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBusinessInterruptionRemovedStatus(Guid businessInterruptionId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                BusinessInterruption businessInterruption = await _businessInterruptionRepository.GetByIdAsync(businessInterruptionId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    businessInterruption.Removed = status;
                    await uow.Commit();
                }


                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region MaterialDamage

        [HttpPost]
        public async Task<IActionResult> AddMaterialDamage(MaterialDamageViewModel model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save BusinessInterruption - No Client information for " + model.AnswerSheetId);

                MaterialDamage materialDamage = _materialDamageRepository.FindAll().FirstOrDefault(md => md.Id == model.MaterialDamageId);
                if (materialDamage == null)
                    materialDamage = model.ToEntity(user);
                model.UpdateEntity(materialDamage);

                if (model.MaterialDamageLocation != null)
                    materialDamage.Location = await _locationService.GetLocationById(model.MaterialDamageLocation);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.MaterialDamages.Add(materialDamage);
                    await uow.Commit();
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetMaterialDamage(Guid answerSheetId, Guid materialDamageId)
        {
            MaterialDamageViewModel model = new MaterialDamageViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                MaterialDamage materialDamage = sheet.MaterialDamages.FirstOrDefault(md => md.Id == materialDamageId);
                if (materialDamage != null)
                {
                    model = MaterialDamageViewModel.FromEntity(materialDamage);
                    model.AnswerSheetId = answerSheetId;

                    if (materialDamage.Location != null)
                        model.MaterialDamageLocation = materialDamage.Location.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialDamages(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var materialDamages = new List<MaterialDamage>();

                materialDamages = sheet.MaterialDamages.Where(md => md.Removed == removed && md.DateDeleted == null).ToList();

                if (_search)
                {

                    switch (searchOper)
                    {
                        case "eq":
                            materialDamages = materialDamages.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            materialDamages = materialDamages.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            materialDamages = materialDamages.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //materialDamages = materialDamages.OrderBy(sidx + " " + sord).ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = materialDamages.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    MaterialDamage materialDamage = materialDamages[i];
                    JqGridRow row = new JqGridRow(materialDamage.Id);
                    row.AddValues(materialDamage.NonHirePlant, materialDamage.Location.CommonName, materialDamage.Location.Street, materialDamage.Location.Suburb, materialDamage.Location.Postcode,
                        materialDamage.Location.City, materialDamage.Id);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetMaterialDamageRemovedStatus(Guid materialDamageId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                MaterialDamage materialDamage = await _materialDamageRepository.GetByIdAsync(materialDamageId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    materialDamage.Removed = status;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Boat

        [HttpPost]
        public async Task<IActionResult> AddBoat(BoatViewModel model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Boat - No Client information for " + model.AnswerSheetId);
                // get existing boat (if any)
                Boat boat = await _boatRepository.GetByIdAsync(model.BoatId);
                // no boat, so create new
                if (boat == null)
                    boat = model.ToEntity(user);
                model.UpdateEntity(boat);
                if (model.BoatLandLocation != Guid.Empty)
                    boat.BoatLandLocation = await _buildingRepository.GetByIdAsync(model.BoatLandLocation);

                if (model.BoatOperator != Guid.Empty)
                    boat.BoatOperator = await _organisationService.GetOrganisation(model.BoatOperator);
                boat.BoatWaterLocation = null;
                if (model.BoatTrailer != Guid.Empty)
                {
                    boat.BoatTrailers.Clear();
                    Vehicle trailer = await _vehicleService.GetVehicleById(model.BoatTrailer);
                    boat.BoatTrailers.Add(trailer);
                }
                if (model.SelectedBoatUse != Guid.Empty)
                {
                    var BoatUse = await _boatUseService.GetBoatUse(model.SelectedBoatUse);
                    boat.BoatUses.Clear();
                    boat.BoatUses.Add(BoatUse);                                        
                }
                boat.BoatOperator = await _organisationService.GetOrganisation(model.BoatOperator);

                if (model.BoatWaterLocation != Guid.Empty)
                {
                    var waterLocation = await _waterLocationRepository.GetByIdAsync(model.BoatWaterLocation);
                    boat.BoatWaterLocation = await _organisationService.GetMarina(waterLocation);
                }
                    
                if (model.OtherMarinaName != null)
                {
                    boat.OtherMarinaName = model.OtherMarinaName;
                    boat.OtherMarina = true;
                }
                else
                {
                    boat.OtherMarina = false;
                }

                if (model.SelectedInterestedParty != null)
                {

                    List<string> interestedpartylist = new List<string>();

                    boat.InterestedParties = new List<Organisation>();

                    //string strArray = model.SelectedBoatUse.Substring(0, model.SelectedBoatUse.Length - 1);
                    string[] interestedParty = model.SelectedInterestedParty.Split(',');

                    model.InterestedParties = new List<Organisation>();

                    foreach (var useid in interestedParty)
                    {
                        boat.InterestedParties.Add(await _organisationService.GetOrganisation(Guid.Parse(useid)));
                    }
                }

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.Boats.Add(boat);
                    await uow.Commit();
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetOriginalVehicle(Guid answerSheetId, Guid vehicleId)
        {
            VehicleViewModel model = new VehicleViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                Vehicle vehicle = sheet.Vehicles.FirstOrDefault(b => b.Id == vehicleId);
                if (vehicle != null)
                {
                    model.AnswerSheetId = answerSheetId;
                    if (vehicle.OriginalVehicle != null)
                        model.OriginalVehicleId = vehicle.OriginalVehicle.Id;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetOriginalBoat(Guid answerSheetId, Guid boatId)
        {
            BoatViewModel model = new BoatViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                Boat boat = sheet.Boats.FirstOrDefault(b => b.Id == boatId);
                if (boat != null)
                {
                    model.AnswerSheetId = answerSheetId;
                    if (boat.OriginalBoat != null)
                    {
                        model.OriginalBoatId = boat.OriginalBoat.Id;
                    }
                    else
                    {
                        model.OriginalBoatId = Guid.Parse("00000000-0000-0000-0000-000000000000");
                    }
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetBoat(Guid answerSheetId, Guid boatId)
        {
            BoatViewModel model = new BoatViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                Boat boat = sheet.Boats.FirstOrDefault(b => b.Id == boatId);
                if (boat != null)
                {
                    model = BoatViewModel.FromEntity(boat);
                    model.AnswerSheetId = answerSheetId;
                    if (boat.BoatLandLocation != null)
                        model.BoatLandLocation = boat.BoatLandLocation.Id;
                    if (boat.BoatWaterLocation != null)
                    {
                        var unit = (MarinaUnit)boat.BoatWaterLocation.OrganisationalUnits.FirstOrDefault();
                        model.BoatWaterLocation = unit.WaterLocation.Id;
                    }

                    // Workaround - if multiple trailers are added by the user, the wrong one could be selected on EDIT. Which one is the right one? Probably the last one added?
                    if (boat.BoatTrailers.Any())
                        model.BoatTrailer = boat.BoatTrailers.LastOrDefault().Id;
                        // model.BoatTrailer = sheet.Vehicles.FirstOrDefault().Id;
                    if (boat.OtherMarinaName != null)
                        model.OtherMarinaName = boat.OtherMarinaName;
                    if (boat.BoatUses != null)
                        model.BoatselectedVal = new List<String>();

                    model.BoatselectedText = new List<Guid>();

                    foreach (var boatuse in boat.BoatUses)
                    {
                        model.BoatselectedVal.Add(boatuse.BoatUseCategory);
                        model.BoatselectedText.Add(boatuse.Id);
                    }
                }

                model.BoatpartyVal = new List<string>();
                model.BoatpartyText = new List<Guid>();

                foreach (var boatparty in boat.InterestedParties)
                {
                    model.BoatpartyVal.Add(boatparty.Name);
                    model.BoatpartyText.Add(boatparty.Id);
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetBoats(Guid informationId, bool validated, bool removed, bool transfered, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                NumberFormatInfo currencyFormat = new CultureInfo(CultureInfo.CurrentCulture.ToString()).NumberFormat;

                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var boats = new List<Boat>();

                if (transfered)
                {
                    boats = sheet.Boats.Where(b => b.Removed == removed && b.DateDeleted == null && b.BoatCeaseDate > DateTime.MinValue && b.BoatCeaseReason == 4).ToList();
                }
                else
                {
                    boats = sheet.Boats.Where(b => b.Removed == removed && b.DateDeleted == null).ToList();
                }

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            boats = boats.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            boats = boats.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            boats = boats.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = boats.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    Boat boat = boats[i];
                    JqGridRow row = new JqGridRow(boat.Id);
                    row.AddValues(boat.Id, boat.BoatName, boat.YearOfManufacture, boat.MaxSumInsured.ToString("C", UserCulture), boat.Id);
                    model.AddRow(row);
                }

                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBoatRemovedStatus(Guid boatId, bool status)
        {
            User user = null;

            try
            {
                bool hasTrailer = false;
                user = await CurrentUser();
                Boat boat = await _boatRepository.GetByIdAsync(boatId);

                //if (boat.BoatTrailer != null)
                //{
                //    hasTrailer = true;
                //}

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    boat.Removed = status;
                    await uow.Commit();
                }
                return Json(new { HasTrailer = hasTrailer, Success = true });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> UndoBoatRemovedStatus(BoatViewModel removedboat)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Boat boat = await _boatRepository.GetByIdAsync(removedboat.BoatId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    boat.Removed = removedboat.Removed;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBoatCeasedStatus(Guid boatId, bool status, DateTime ceaseDate, int ceaseReason)
        {
            User user = null;
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone);
            try
            {
                user = await CurrentUser();
                Boat boat = await _boatRepository.GetByIdAsync(boatId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    
                    //boat.BoatCeaseDate = DateTime.Parse(LocalizeTime(ceaseDate, "d"));
                    boat.BoatCeaseDate = DateTime.Parse(ceaseDate.ToString(), System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")).ToUniversalTime(tzi);
                    boat.BoatCeaseReason = ceaseReason;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBoatTransferedStatus(Guid boatId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Boat boat = await _boatRepository.GetByIdAsync(boatId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    boat.BoatCeaseDate = DateTime.MinValue;
                    boat.BoatCeaseReason = '0';
                    await uow.Commit().ConfigureAwait(false);
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region BoatUse

        [HttpPost]
        public async Task<IActionResult> AddBoatUse(IFormCollection collection)
        {
            User user = null;
            BoatUse boatUse = null;
            try
            {
                user = await CurrentUser();
                if(Guid.TryParse(collection["AnswerSheetId"], out Guid InformationId))
                {
                    ClientInformationSheet sheet = await _clientInformationService.GetInformation(InformationId);
                    if (collection.ContainsKey("BoatUseId"))
                    {
                        if (Guid.TryParse(collection["BoatUseId"], out Guid BoatUseId))
                        {
                            if (BoatUseId != Guid.Empty)
                            {
                                boatUse = await _boatUseService.GetBoatUse(BoatUseId);
                            }                            
                        }
                    }
                    else
                    {
                        boatUse = new BoatUse(user, "");
                        boatUse.PopulateEntity(collection);
                        await _boatUseService.UpdateBoatUse(boatUse);
                    }                    
                }

                return Json(boatUse);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        //private void AddOrganisation(OrganisationViewModel model)
        //{
        //    throw new Exception("Unfinihed core update");
        //}

        //[HttpPost]
        //public async Task<IActionResult> AddOrganisation(IFormCollection collection)
        //{
        //    User currentUser = await CurrentUser();
        //    Guid Id = Guid.Parse(collection["ClientInformationSheet.Id"]);
        //    ClientInformationSheet Sheet = await _clientInformationService.GetInformation(Id);

        //    var jsonOrganisation = (Organisation)GetModelDeserializedModel(typeof(Organisation), collection);
        //    var jsonUser = (User)GetModelDeserializedModel(typeof(User), collection);

        //    string Email = jsonOrganisation.Email;
        //    string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
        //    string Name = jsonOrganisation.Name;
        //    string FirstName = jsonUser.FirstName;
        //    string LastName = jsonUser.LastName;
        //    string OrganisationTypeName = collection["OrganisationViewModel.OrganisationType"].ToString();
        //    Organisation organisation = await _organisationService.GetAnyRemovedAdvisor(Email);
        //    //condition for organisation exists
        //    try
        //    {
        //        if (organisation != null)
        //        {
        //            await _clientInformationService.RemoveOrganisationFromSheets(organisation);
        //            //await _organisationService.ChangeOwner(organisation, Sheet);
        //        }
        //        if (organisation == null)
        //        {
        //            organisation = await _organisationService.GetOrCreateOrganisation(Email, TypeName, Name, OrganisationTypeName, FirstName, LastName, currentUser, collection);
        //        }

        //        await _organisationService.UpdateOrganisation(collection);

        //        if (!Sheet.Organisation.Contains(organisation))
        //            Sheet.Organisation.Add(organisation);

        //        await _clientInformationService.UpdateInformation(Sheet);
        //        return Redirect("../Information/EditInformation?Id="+ Sheet.Programme.Id.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}        

        //[HttpPost]
        //public async Task<IActionResult> GetOrganisation(OrganisationViewModel model)
        //{
        //    User user = null;
        //    Guid OrganisationId = Guid.Parse(model.ID.ToString());//Guid.Parse(collection["OrganisationId"]);
        //    IList<object> JsonObjects = new List<object>();
        //    try
        //    {                   
                //Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
        //        User orgUser = await _userService.GetUserByEmail(organisation.Email);                
        //        JsonObjects.Add(orgUser);
        //        JsonObjects.Add(organisation);
        //        var jsonObj = GetSerializedModel(JsonObjects);

        //       return Json(jsonObj);
        //    }
        //    catch(Exception ex)
        //    {               
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return Json(ex.Message);
        //    }
        //}


        //[HttpPost]
        //public async Task<IActionResult> GetMooredType(Guid OrgID)
        //{
        //    Organisation organisation = null;
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        organisation = await _organisationService.GetOrganisation(OrgID);
        //        var organisationalUnits = new List<OrganisationalUnitViewModel>();
        //        List<SelectListItem> mooredtypes = new List<SelectListItem>();

        //        foreach (var mooredtype in organisation.OrganisationalUnits.First().Marinaorgmooredtype)
        //        {
        //            mooredtypes.Add(new SelectListItem
        //            {
        //                Selected = false,
        //                Value = mooredtype,
        //                Text = mooredtype
        //            });
        //        }

        //        return Json(mooredtypes);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }

        //}

        //[HttpPost]
        //public async Task<IActionResult> OUSelected(Guid OUselect)
        //{
        //    OrganisationalUnit orgunit = null;
        //    var location = new LocationViewModel();
        //    User user = null;

        //    try
        //    {
        //        user = await CurrentUser();
        //        orgunit = await _organisationalUnitService.GetOrganisationalUnit(OUselect);
        //        foreach (Location ou in orgunit.Locations)
        //        {
        //            location.Locations.Add(ou);
        //        }

        //        return Json(location);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> GetBoatUse(Guid answerSheetId, Guid boatUseId)
        {
            BoatUseViewModel model = new BoatUseViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                foreach(var boat in sheet.Boats.Where(b => b.BoatUses.Any()))
                {
                    foreach(var boatUse in boat.BoatUses)
                    {
                        model = BoatUseViewModel.FromEntity(boatUse);
                        model.AnswerSheetId = answerSheetId;
                    }
                    
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBoatUses(Guid informationId, bool removed, bool ceased, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var boats = new List<Boat>();
                var boatUses = new List<BoatUse>();

                /*
                ceased doesn't look to be stored in table????
                if (ceased)
                {
                    boats = sheet.Boats.Where(b => b.BoatUses.Any(bu => bu.Removed == removed && bu.DateDeleted == null && bu.BoatUseCeaseDate > DateTime.MinValue)).ToList();
                    //boatUses = sheet.BoatUses.Where(bu => bu.Removed == removed && bu.DateDeleted == null && bu.BoatUseCeaseDate > DateTime.MinValue).ToList();
                }
                else
                {
                    boats = sheet.Boats.Where(b => b.BoatUses.Any(bu => bu.Removed == removed && bu.DateDeleted == null && bu.BoatUseCeaseDate == DateTime.MinValue)).ToList();
                    //boatUses = sheet.BoatUses.Where(bu => bu.Removed == removed && bu.DateDeleted == null && bu.BoatUseCeaseDate == DateTime.MinValue).ToList();
                }*/


                // Get all the boat uses in the sheet that are removed or not removed THIS DOESNT WORK FAM as it just grabs the boat with them all anyways
                if (removed)
                {
                    boats = sheet.Boats.Where(b => b.BoatUses.Any(bu => bu.Removed == true)).ToList();                  
                }
                else
                {
                    boats = sheet.Boats.Where(b => b.BoatUses.Any(bu => bu.Removed == false)).ToList();
                }

                // For each of the boats that have been removed or not removed
                foreach (var boat in boats)
                {
                    // for the boat uses for that boat add to our boat uses list NO FILTERING 
                    foreach (var boatUse in boat.BoatUses)
                    {
                        if (removed)
                        {
                            // add only removed boats to the list
                            if (boatUse.Removed == true)
                            {
                                boatUses.Add(boatUse);
                            }
                        }
                        else
                        {
                            if (boatUse.Removed == false)
                            {
                                boatUses.Add(boatUse);
                            }
                        }
                    }
                }

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            boatUses = boatUses.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            boatUses = boatUses.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            boatUses = boatUses.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //boatUses = boatUses.OrderBy(sidx + " " + sord).ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = boatUses.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    BoatUse boatUse = boatUses[i];
                    JqGridRow row = new JqGridRow(boatUse.Id);
                    row.AddValues(boatUse.Id, boatUse.BoatUseCategory, boatUse.Id);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> SetBoatUseRemovedStatus(Guid boatUseId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                BoatUse boatUse = await _boatUseService.GetBoatUse(boatUseId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    boatUse.Removed = status;
                    await uow.Commit();
                }
                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetBoatUseCeasedStatus(Guid boatUseId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                BoatUse boatUse = await _boatUseService.GetBoatUse(boatUseId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    boatUse.BoatUseCeaseDate = DateTime.MinValue;
                    boatUse.BoatUseCeaseReason = '0';
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Claim

        [HttpPost]
        public async Task<IActionResult> AddClaim(ClaimViewModel model)
        {
            User user = null;

            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();

                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Claim - No Client information for " + model.AnswerSheetId);

                ClaimNotification claimNotification = await _claimNotificationService.GetClaimNotificationById(model.ClaimId);
                // no claim, so create new
                if (claimNotification == null)
                    claimNotification = model.ToEntity(user);
                model.UpdateEntity(claimNotification);

                if (model.OrganisationId != Guid.Empty)
                {
                    Organisation org = await _organisationService.GetOrganisation(model.OrganisationId);
                    claimNotification.Organisation = org;
                }

                if (model.ClaimProducts != null)
                {
                    var productList = await _productService.GetAllProducts();
                    claimNotification.ClaimProducts = productList.Where(pro => model.ClaimProducts.Contains(pro.Id)).ToList();
                }

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.ClaimNotifications.Add(claimNotification);
                    await uow.Commit();
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetClaim(Guid answerSheetId, Guid claimId)
        {
            ClaimViewModel model = new ClaimViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                ClientProgramme clientProgramme = sheet.Programme;
                ClaimNotification claim = sheet.ClaimNotifications.FirstOrDefault(c => c.Id == claimId);
                if (claim != null)
                {
                    model = ClaimViewModel.FromEntity(claim);
                    model.AnswerSheetId = answerSheetId;
                }
                var claimProducts = new List<Product>();
                List<SelectListItem> ClaimProducts = new List<SelectListItem>();

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaims(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                         string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var claims = new List<ClaimNotification>();
                claims = sheet.ClaimNotifications.Where(c => c.Removed == removed && c.DateDeleted == null).ToList();

                if (_search)
                {

                    switch (searchOper)
                    {
                        case "eq":
                            claims = claims.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            claims = claims.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            claims = claims.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                //claims = claims.OrderBy(sidx + " " + sord).ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = page;
                model.TotalRecords = claims.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    ClaimNotification claim = claims[i];
                    JqGridRow row = new JqGridRow(claim.Id);
                    if (claim.ClaimStatus != "Precautionary notification only")
                    {
                        row.AddValues(claim.Id, claim.ClaimTitle, claim.ClaimDescription, claim.ClaimReference, claim.Claimant);
                    }
                    else
                    {
                        row.AddValues(claim.Id, claim.ClaimTitle, claim.ClaimDescription, claim.ClaimReference, claim.Claimant, claim.Id);
                    }
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering.
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetClaimRemovedStatus(Guid claimId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClaimNotification claim = await _claimNotificationService.GetClaimNotificationById(claimId);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    claim.Removed = status;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Operators

        


        #endregion

        #region BusinessContracts
        [HttpGet]
        public async Task<IActionResult> GetBusinessContracts(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                          string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var businessContracts = sheet.BusinessContracts.Where(bc => bc.Removed == removed && bc.DateDeleted == null).ToList();

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            businessContracts = businessContracts.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            businessContracts = businessContracts.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            businessContracts = businessContracts.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                businessContracts = businessContracts.ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = 1;
                model.TotalRecords = businessContracts.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    BusinessContract businessContract = businessContracts[i];
                    JqGridRow row = new JqGridRow(businessContract.Id);
                    row.AddValues(businessContract.Id, businessContract.Year, businessContract.ContractTitle, businessContract.ConstructionValue, businessContract.Fees, businessContract.ContractType, businessContract.Id);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProject(IFormCollection collection)
        {
            User user = null;
            try
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));
                user = await CurrentUser();
                BusinessContract businessContract = null;
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));
                var projectForm = collection.Keys.Where(s => s.StartsWith("ProjectViewModel", StringComparison.CurrentCulture));
                var id = collection["ProjectViewModel.ProjectId"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    businessContract = new BusinessContract(user);
                }
                else
                {
                    businessContract = await _businessContractService.GetBusinessContractById(Guid.Parse(id));
                    businessContract.ProjectDirector = false;
                    businessContract.ProjectEngineer = false;
                    businessContract.ProjectManager = false;
                    businessContract.ProjectCoordinator = false;
                }
                var type = businessContract.GetType();
                foreach (var keyField in projectForm)
                {
                    if (keyField != "ProjectViewModel.ProjectId")
                    {
                        var propertyName = keyField.Split('.').ToList();
                        var property = type.GetProperty(propertyName.LastOrDefault());
                        if (typeof(string) == property.PropertyType)
                        {
                            property.SetValue(businessContract, collection[keyField].ToString());
                        }
                        if (typeof(bool) == property.PropertyType)
                        {
                            property.SetValue(businessContract, bool.Parse(collection[keyField].ToString()));
                        }
                    }
                }

                if (sheet.BusinessContracts.Contains(businessContract))
                {
                    await _businessContractService.Update(businessContract);
                }
                else
                {
                    sheet.BusinessContracts.Add(businessContract);
                    await _clientInformationService.UpdateInformation(sheet);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProject(string projectId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                BusinessContract businessContract = await _businessContractService.GetBusinessContractById(Guid.Parse(projectId));
                businessContract.Removed = true;
                await _businessContractService.Update(businessContract);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RestoreProject(string projectId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                BusinessContract businessContract = await _businessContractService.GetBusinessContractById(Guid.Parse(projectId));
                businessContract.Removed = false;
                await _businessContractService.Update(businessContract);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> RestoreResearchHouse(string researchHouseId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ResearchHouse researchHouse = await _researchHouseService.GetResearchHouseById(Guid.Parse(researchHouseId));
                researchHouse.Removed = false;
                await _researchHouseService.Update(researchHouse);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddResearchHouse(IFormCollection collection)
        {
            User user = null;
            try
            {
                if (collection == null)
                    throw new ArgumentNullException(nameof(collection));
                user = await CurrentUser();
                ResearchHouse researchHouse = null;
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));
                var projectForm = collection.Keys.Where(s => s.StartsWith("ResearchHouseViewModel", StringComparison.CurrentCulture));
                var id = collection["ResearchHouseViewModel.ProjectId"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    researchHouse = new ResearchHouse(user);
                }
                else
                {
                    researchHouse = await _researchHouseService.GetResearchHouseById(Guid.Parse(id));                   
                }
                var type = researchHouse.GetType();
                foreach (var keyField in projectForm)
                {
                    if (keyField != "ResearchHouseViewModel.ProjectId")
                    {
                        var propertyName = keyField.Split('.').ToList();
                        var property = type.GetProperty(propertyName.LastOrDefault());
                        if (typeof(string) == property.PropertyType)
                        {
                            property.SetValue(researchHouse, collection[keyField].ToString());
                        }
                        if (typeof(bool) == property.PropertyType)
                        {
                            property.SetValue(researchHouse, bool.Parse(collection[keyField].ToString()));
                        }
                    }
                }

                if (sheet.ResearchHouses.Contains(researchHouse))
                {
                    await _researchHouseService.Update(researchHouse);
                }
                else
                {
                    sheet.ResearchHouses.Add(researchHouse);
                    await _clientInformationService.UpdateInformation(sheet);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SetBusinessContractRemovedStatus(Guid businessContractId, bool status)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                BusinessContract businessContract = await _businessContractService.GetBusinessContractById(businessContractId);
                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    businessContract.Removed = status;
                    await uow.Commit();
                }

                return new JsonResult(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCEASProject(BusinessContractViewModel model)
        {
            User user = null;

            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Location - No Client information for " + model.AnswerSheetId);

                BusinessContract businessContract = await _businessContractService.GetBusinessContractById(model.BusinessContractId);
                if (businessContract == null)
                    businessContract = model.ToEntity(user);
                model.UpdateEntity(businessContract);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    sheet.BusinessContracts.Add(businessContract);
                    await uow.Commit();
                }

                model.BusinessContractId = businessContract.Id;

                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditCEASProject(BusinessContractViewModel model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(model.AnswerSheetId);
                if (sheet == null)
                    throw new Exception("Unable to save Location - No Client information for " + model.AnswerSheetId);

                BusinessContract businessContract = await _businessContractService.GetBusinessContractById(model.BusinessContractId);
                if (businessContract == null)
                    businessContract = model.ToEntity(user);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    model.UpdateEntity(businessContract);
                    await uow.Commit();
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetCEASProject(Guid answerSheetId, Guid CEASProjectId)
        {
            BusinessContractViewModel model = new BusinessContractViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                BusinessContract businessContract = sheet.BusinessContracts.FirstOrDefault(bc => bc.Id == CEASProjectId);
                if (businessContract != null)
                {
                    model = BusinessContractViewModel.FromEntity(businessContract);
                    model.AnswerSheetId = answerSheetId;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetBusinessContract(Guid answerSheetId, Guid businessContractId)
        {
            BusinessContractViewModel model = new BusinessContractViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(answerSheetId);
                BusinessContract businessContract = sheet.BusinessContracts.FirstOrDefault(bc => bc.Id == businessContractId);
                if (businessContract != null)
                {
                    model = BusinessContractViewModel.FromEntity(businessContract);
                    model.AnswerSheetId = answerSheetId;
                }
                return Json(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Getceasprojects(Guid informationId, bool removed, bool _search, string nd, int rows, int page, string sidx, string sord,
                                          string searchField, string searchString, string searchOper, string filters)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(informationId);
                if (sheet == null)
                    throw new Exception("No valid information for id " + informationId);

                var businessContracts = sheet.BusinessContracts.Where(bc => bc.Removed == removed && bc.DateDeleted == null).ToList();

                if (_search)
                {
                    switch (searchOper)
                    {
                        case "eq":
                            businessContracts = businessContracts.Where(searchField + " = \"" + searchString + "\"").ToList();
                            break;
                        case "bw":
                            businessContracts = businessContracts.Where(searchField + ".StartsWith(\"" + searchString + "\")").ToList();
                            break;
                        case "cn":
                            businessContracts = businessContracts.Where(searchField + ".Contains(\"" + searchString + "\")").ToList();
                            break;
                    }
                }
                businessContracts = businessContracts.ToList();

                XDocument document = null;
                JqGridViewModel model = new JqGridViewModel();
                model.Page = 1;
                model.TotalRecords = businessContracts.Count;
                model.TotalPages = ((model.TotalRecords - 1) / rows) + 1;

                int offset = rows * (page - 1);
                for (int i = offset; i < offset + rows; i++)
                {
                    if (i == model.TotalRecords)
                        break;

                    BusinessContract businessContract = businessContracts[i];
                    JqGridRow row = new JqGridRow(businessContract.Id);
                    row.AddValues(businessContract.Id, businessContract.ContractTitle, businessContract.ProjectDescription, businessContract.Fees, businessContract.Id);
                    model.AddRow(row);
                }

                // convert model to XDocument for rendering
                document = model.ToXml();
                return Xml(document);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        #endregion

        #region CoastGuardSelfReg

        [HttpPost]
        public async Task<IActionResult> CoastGuardSelfReg(string craftType, string membershipNumber, string boatType, string constructionType, string hullConfiguration, string mooredType, string trailered,
            string boatInsuredValue, string quickQuotePremium, string firstName, string lastName, string email, string orgType, string homePhone, string mobilePhone)
        {
            User currentUser = null;
            bool hasAccount = true;
            string organisationName = null;
            string ouname = null;
            string orgTypeName = null;

            try
            {
                currentUser = await CurrentUser();
                if (orgType == "Private") //orgType = "Private", "Company", "Trust", "Partnership"
                {
                    organisationName = firstName + " " + lastName;
                    ouname = "Home";
                }
                else
                {
                    organisationName = "To be completed";
                    ouname = "Head Office";
                }
                switch (orgType)
                {
                    case "Private":
                        {
                            orgTypeName = "Person - Individual";
                            break;
                        }
                    case "Company":
                        {
                            orgTypeName = "Corporation – Limited liability";
                            break;
                        }
                    case "Trust":
                        {
                            orgTypeName = "Trust";
                            break;
                        }
                    case "Partnership":
                        {
                            orgTypeName = "Partnership";
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Invalid Organisation Type: ", orgType));
                        }
                }
                string phonenumber = null;
                if (homePhone == null)
                {
                    phonenumber = homePhone;
                }
                else
                {
                    phonenumber = mobilePhone;
                }
                OrganisationType organisationType = null;
                organisationType = await _organisationTypeService.GetOrganisationTypeByName(orgTypeName);
                if (organisationType == null)
                {
                    organisationType = await _organisationTypeService.CreateNewOrganisationType(null, orgTypeName);
                }
                Organisation organisation = null;
                organisation = await _organisationService.GetOrganisationByEmail(email);

                //condition for organisation exists
                if (organisation == null)
                {
                    organisation = new Organisation(null, Guid.NewGuid(), organisationName, organisationType);
                    organisation.Phone = phonenumber;
                    organisation.Email = email;
                    await _organisationService.CreateNewOrganisation(organisation);

                    User user = null;
                    User user2 = null;

                    try
                    {
                        user = await _userService.GetUserByEmail(email);
                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);
                        var username = (user.FirstName + "_" + lastName).Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
                    }
                    catch (Exception ex)
                    {
                        string username = (firstName + "_" + lastName).Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

                        try
                        {
                            user2 = await _userService.GetUser(username);

                            if (user2 != null && user == user2)
                            {
                                Random random = new Random();
                                int randomNumber = random.Next(10, 99);
                                username = username + randomNumber.ToString();
                            }
                        }
                        catch (Exception)
                        {

                            try
                            {
                                user = new User(null, Guid.NewGuid(), username);
                                user.FirstName = firstName;
                                user.LastName = lastName;
                                user.FullName = firstName + " " + lastName;
                                user.Email = email;
                                user.Phone = homePhone;
                                user.MobilePhone = mobilePhone;
                                user.Password = "";
                                //user.Organisations.Add (personalOrganisation);
                                // save the new user
                                // creates a new user in the system along with a default organisation
                                await _userService.Create(user);
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine(ex1.Message);
                            }

                        }
                    }
                    finally
                    {
                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);

                        user.SetPrimaryOrganisation(organisation);
                        await _userService.Update(user);

                        try
                        {
                            _ldapService.ChangePassword(user.UserName, "", _appSettingService.IntermediatePassword);
                            var programme = await _programmeService.GetCoastGuardProgramme();
                            var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);
                            var reference = await _referenceService.GetLatestReferenceId();
                            var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                            await _referenceService.CreateClientInformationReference(sheet);

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                OrganisationalUnit ou = new OrganisationalUnit(user, ouname);
                                Boat vessel = new Boat(user)
                                {
                                    BoatType1 = boatType,
                                    BoatType2 = craftType,
                                    HullConstruction = constructionType,
                                    HullConfiguration = hullConfiguration,
                                    BoatIsTrailered = trailered,
                                    MaxSumInsured = Convert.ToInt32(boatInsuredValue)
                                    //BoatQuickQuotePremium = Convert.ToDecimal(quickQuotePremium),
                                };
                                sheet.Boats.Add(vessel);
                                organisation.OrganisationalUnits.Add(ou);
                                clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                clientProgramme.ClientProgrammeMembershipNumber = membershipNumber;
                                sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, "First Mate Cover Quick Quote Consuming Process Completed"));
                                try
                                {
                                    Thread.Sleep(1000);
                                    await uow.Commit();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }

                            }

                            if (programme.ProgEnableEmail)
                            {
                                //send password reset
                                SingleUseToken token = await _authenticationService.GenerateSingleUseToken(email);
                                string domain = "https://" + _appSettingService.domainQueryString; //HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                                await _emailService.SendPasswordResetEmail(email, token.Id, domain);
                                EmailTemplate emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == "SendInformationSheetInstruction");
                                if (emailTemplate != null)
                                {
                                    await _emailService.SendEmailViaEmailTemplate(email, emailTemplate, null, sheet, null);
                                }
                                //send out information sheet issue notification email

                                await _emailService.SendSystemEmailUISIssueNotify(programme.BrokerContactUser, programme, clientProgramme.InformationSheet, organisation);
                            }

                        }
                        catch (Exception ex)
                        {
                            await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                        }

                    }
                }
                else
                {
                    hasAccount = false;
                }

                if (hasAccount)
                {
                    return new JsonResult(true);
                }

                else
                {
                    return new JsonResult(false);
                }

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        #endregion

        #region Advisory
        [HttpGet]
        public async Task<IActionResult> CloseAdvisory(string ClientInformationSheetId)
        {
            User user = await CurrentUser();
            try
            {
                var sheet = await _clientInformationService.GetInformation(Guid.Parse(ClientInformationSheetId));
                sheet.Status = "Not Taken Up";
                foreach (var agreement in sheet.Programme.Agreements)
                {
                    agreement.Status = "Not Taken Up";
                    await _clientAgreementService.UpdateClientAgreement(agreement);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> IssueUIS(IFormCollection collection)
        {            
            User currentUser = null;            
            string Email = collection["OrganisationViewModel.Organisation.Email"].ToString();
            string Name = collection["OrganisationViewModel.Organisation.Name"].ToString();
            string FirstName = collection["OrganisationViewModel.User.FirstName"].ToString();
            string LastName = collection["OrganisationViewModel.User.FirstName"].ToString();
            string OrganisationTypeName = collection["OrganisationViewModel.OrganisationType"].ToString();

            Guid programmeId = Guid.Parse(collection["Programme.Id"]);
            Organisation organisation = null;
            string membershipNumber = collection["MemberShipNo"];

            try
            {
                currentUser = await CurrentUser();
                organisation = await _organisationService.CreateOrganisation(Email, OrganisationTypeName, Name, OrganisationTypeName, FirstName, LastName, currentUser, collection);                                    
                var user = await _userService.GetUserByEmail(Email);
                var sheet = await _programmeService.CreateUIS(programmeId, user, organisation);
                
                if (organisation != null)
                {
                    sheet.Organisation.Add(organisation);
                }                                
                if (!string.IsNullOrWhiteSpace(membershipNumber))
                {
                    sheet.Programme.ClientProgrammeMembershipNumber = membershipNumber;
                }

                if (user != null && organisation != null)
                {
                    if (!user.Organisations.Contains(organisation))
                        user.Organisations.Add(organisation);

                    await _userService.Update(user);
                }

                await _clientInformationService.UpdateInformation(sheet);

                return RedirectPermanent("../Home/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateDateOfCommencement(IFormCollection collection)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                if (Guid.TryParse(collection["Id"], out Guid Id) && !string.IsNullOrWhiteSpace(collection["Date"]))
                {

                    DateTime.TryParse(collection["Date"], UserCulture, DateTimeStyles.None, out DateTime dateTime);
                    //DateTime dateTime = DateTime.Parse(().ToString(CultureInfo.GetCultureInfo("en-NZ").DateTimeFormat.ShortDatePattern));
                    ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(Id);
                    var product = clientProgramme.BaseProgramme.Products.FirstOrDefault();
                    if(dateTime >= product.DefaultInceptionDate && dateTime <= product.DefaultExpiryDate)
                    {
                        return Json(false);
                    }
                    return Json(true);
                }

                return Json(false);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

    }
}


