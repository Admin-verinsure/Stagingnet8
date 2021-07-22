using AutoMapper;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.Organisation;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Infrastructure.FluentNHibernate;
using System.Linq.Dynamic;
using NHibernate.Linq;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class OrganisationController : BaseController
    {
        ISerializerationService _serialiserService;
        IOrganisationService _organisationService;
        IClientInformationService _clientInformationService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<OrganisationController> _logger;        
        IMilestoneService _milestoneService;
        IProgrammeService _programmeService;
        IMapper _mapper;
        IEmailService _emailService;

        public OrganisationController(
            IProgrammeService programmeService,
            IMilestoneService milestoneService,
            ISerializerationService serialiserService,
            ILogger<OrganisationController> logger,
            IClientInformationService clientInformationService,
            IApplicationLoggingService applicationLoggingService,
            IOrganisationService organisationService,
            IUserService userRepository,
            IEmailService emailService,
            IMapper mapper
            )
            : base (userRepository)
        {
            _mapper = mapper;
            _programmeService = programmeService;
            _milestoneService = milestoneService;
            _serialiserService = serialiserService;
            _clientInformationService = clientInformationService;
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;
            _organisationService = organisationService;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOrganisationEmail(IFormCollection collection)
        {
            var email = collection["OrganisationViewModel.User.Email"].ToString();
            bool ValidBackEndEmail;
            Guid.TryParse(collection["OrganisationViewModel.Organisation.Id"].ToString(), out Guid OrganisationId);
            Guid.TryParse(collection["ClientInformationSheet.Id"].ToString(), out Guid SheetId);
            ClientInformationSheet sheet = await _clientInformationService.GetInformation(SheetId);
            Organisation organisation = await _organisationService.GetOrganisationByEmail(email);
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email.Trim());
                ValidBackEndEmail = addr.Address == email;

                if (organisation != null)
                {
                    if (OrganisationId == Guid.Empty)
                    {
                        //return Json(true);
                    }
                    if (sheet.Owner.Id == OrganisationId)
                    {
                        return Json(false);
                    }
                    if (sheet.Organisation.Contains(organisation))
                    {
                        return Json(false);
                    }
                }
                return Json(false);
            }
            catch
            {
                return Json(true);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateAttachOrganisationEmail(IFormCollection collection)
        {
            bool ValidBackEndEmail;

            var email = collection["RemovedOrganisation.Email"].ToString();
            Organisation removedOrg = await _organisationService.GetOrganisation(Guid.Parse(collection["RemovedOrganisation.Id"]));
            Organisation organisation = await _organisationService.GetOrganisationByEmail(email);
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                ValidBackEndEmail = addr.Address == email;
                
                // Checks if you can create an email from the string (only works if it's a valid email)
                if (ValidBackEndEmail == true)
                {
                    // Checks if there is an organisation with that email already
                    if (organisation != null)
                    {
                        // Then it has to be the same as removedOrg.Email otherwise data is invalid
                        if (email == removedOrg.Email)
                        {
                            return Json(false); //Valid
                        }
                        else
                        {
                            return Json(true); // Not Valid
                        }
                    }
                    else
                    {
                        return Json(false); //Valid
                    }
                }
                else
                {
                    return Json(true); // Not Valid
                }                                             
            }
            catch
            {
                return Json(true); // Not Valid
            }
        }


        [HttpPost]
        public async Task<IActionResult> GetOrganisation(OrganisationViewModel model)
        {
            User user = null;
            Guid OrganisationId = Guid.Parse(model.ID.ToString());//Guid.Parse(collection["OrganisationId"]);
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            try
            {
                Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
                User orgUser = await _userService.GetUserPrimaryOrganisationOrEmail(organisation);
                JsonObjects.Add("Organisation", organisation);
                JsonObjects.Add("User", orgUser);
                var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);

                return Json(jsonObj);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetFAPOrgsByClientProgrammeId(Guid clientProgrammeId)
        {
            User currentUser = await CurrentUser();
            ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(clientProgrammeId);           
            ClientInformationSheet lastInformationSheet = clientProgramme.InformationSheet;

            while (lastInformationSheet.NextInformationSheet != null)
            {
                lastInformationSheet = lastInformationSheet.NextInformationSheet;
            }

            IList<Organisation> organisations = lastInformationSheet.Organisation;

            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();

            try
            {
                foreach (Organisation org in organisations)
                {
                    //List<OrganisationalUnit> ListAdvisorunit = (List<OrganisationalUnit>)org.OrganisationalUnits.Where(u => u.Name == "Advisor" );

                    //var orgHasFAPLicenseNumber = org.OrganisationalUnits.FirstOrDefault(ou => ou.FAPLicenseNumber != null);

                    if (org.isOrganisationTheFAP)
                    {
                        JsonObjects.Add(org.Id.ToString(), org);

                    }
                    foreach (AdvisorUnit Advisorunit in org.OrganisationalUnits.Where(u => u.Name == "Advisor"))
                    {
                        if (Advisorunit.isTheFAP)
                        {
                            JsonObjects.Add(org.Id.ToString(), org);

                        }
                        else
                        {
                            continue;
                        }

                    }

                    foreach (AdministratorUnit administratorUnit in org.OrganisationalUnits.Where(u => u.Name == "Administrator"))
                    {
                        if (administratorUnit.isAdministratorTheFAP)
                        {
                            JsonObjects.Add(org.Id.ToString(), org);

                        }
                        else
                        {
                            continue;
                        }

                    }

                    //if (Advisorunit.FAPLicenseNumber == null)

                    //var isTheFAP = org.OrganisationalUnits.FirstOrDefault(u => (u.isTheFAP == true) && (u.DateDeleted == null));

                }
               
                var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);
                return Json(jsonObj);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return Json(ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> GetAllisTheFAPOrgs(Guid clientProgrammeId)
        //{
        //    User currentUser = await CurrentUser();
        //    ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(clientProgrammeId);

            
        //    Dictionary<string, object> JsonObjects = new Dictionary<string, object>();

        //    IList<OrganisationalUnit> allisTheFAPOUs = await _organisationalUnitRepository.FindAll().Where(ou => ou.isTheFAP == true).ToListAsync();
        //    IList<Guid> allisTheFAPOrgsIds = new List<Guid>();

        //    foreach (var unit in allisTheFAPOUs)
        //    {
        //        allisTheFAPOrgsIds.Add(unit.organisation_id);
        //    }
        //    foreach (var FAPOrgId in allisTheFAPOrgsIds)
        //    {
        //        var org = await _organisationRepository.GetByIdAsync(FAPOrgId);
        //        JsonObjects.Add(org.Id.ToString(), org);
        //    }

        //    var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);
        //    return Json(jsonObj);
        //}

        [HttpPost]
        public async Task<IActionResult> GetFAPLicenseNumOrgsByClientProgrammeId(Guid clientProgrammeId)
        {
            User currentUser = await CurrentUser();
            ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(clientProgrammeId);
            IList<Organisation> organisations = new List<Organisation>();
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            ClientInformationSheet lastInformationSheet = clientProgramme.InformationSheet;

            while (lastInformationSheet.NextInformationSheet != null)
            {
                lastInformationSheet = lastInformationSheet.NextInformationSheet;
            }

            try
            {
                foreach (Organisation org in lastInformationSheet.Organisation)
                {


                    var ListAdvisorunit = (AdvisorUnit)org.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");

                    //foreach (var Advisorunit in ListAdvisorunit)
                    //{
                    if (ListAdvisorunit!= null && ListAdvisorunit.isTheFAP)
                    {
                        if (ListAdvisorunit.FAPLicenseNumber == null)
                        {
                            continue;
                        }
                        else
                        {
                            JsonObjects.Add(org.Id.ToString(), org);
                        }
                    }
                    else
                    {
                        continue;
                    }

                   // }


                    //var Advisorunit = (AdvisorUnit)org.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");

                    //var orgHasFAPLicenseNumber = org.OrganisationalUnits.FirstOrDefault(ou => ou.FAPLicenseNumber != null);
                   
                }

                var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);
                return Json(jsonObj);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganisation(IFormCollection collection)
        {
            User currentUser = await CurrentUser();
            Guid.TryParse(collection["OrganisationViewModel.Organisation.Id"], out Guid OrganisationId);
            Guid.TryParse(collection["ClientInformationSheet.Id"], out Guid Id);
            ClientInformationSheet Sheet = await _clientInformationService.GetInformation(Id);

            var jsonOrganisation = (Organisation)await _serialiserService.GetDeserializedObject(typeof(Organisation), collection);
            var jsonUser = (User)await _serialiserService.GetDeserializedObject(typeof(User), collection);
            string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            string OrganisationTypeName = collection["OrganisationViewModel.OrganisationType"].ToString();
            Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
            //condition for organisation exists
            try
            {

                if (organisation == null)
                {
                    organisation = await _organisationService.CreateOrganisation(jsonUser.Email, TypeName, jsonOrganisation.Name, OrganisationTypeName, jsonUser.FirstName, jsonUser.LastName, currentUser, collection);
                }

                await _organisationService.PostOrganisation(collection, organisation);
                if (!Sheet.Organisation.Contains(organisation))
                    Sheet.Organisation.Add(organisation);

                await _clientInformationService.UpdateInformation(Sheet);
                //return Ok();
                return Redirect("../Information/EditInformation?Id=" + Sheet.Programme.Id.ToString());
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ManageOrganisations(Guid Id)
        {
            Programme programme = await _programmeService.GetProgrammeById(Id);
            OrganisationViewModel model = new OrganisationViewModel(null, null);
            var marinas = await _organisationService.GetAllMarinas();
            foreach(var mar in marinas)
            {
                model.Organisations.Add(mar);
            }
            
            var institutes = await _organisationService.GetFinancialInstitutes();
            foreach (var inst in institutes)
            {
                model.Organisations.Add(inst);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetMarina(IFormCollection model)
        {
            Organisation organisation = await _organisationService.GetOrganisation(Guid.Parse(model["Id"]));
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            if (organisation != null)
            {
                var unit = (MarinaUnit)organisation.OrganisationalUnits.FirstOrDefault();
                JsonObjects.Add("Marina", organisation);
                JsonObjects.Add("WaterLocation", unit.WaterLocation);
                var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);
                return Json(jsonObj);
            }
            return NoContent();
        }
        

        [HttpPost]
        public async Task<IActionResult> PostMarina(IFormCollection model)
        {
            await _organisationService.PostMarina(model);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> GetInstitute(IFormCollection model)
        {
            Organisation organisation = await _organisationService.GetOrganisation(Guid.Parse(model["Id"]));
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            if (organisation != null)
            {
                var unit = (InterestedPartyUnit)organisation.OrganisationalUnits.FirstOrDefault();
                JsonObjects.Add("Institute", organisation);
                JsonObjects.Add("Location", unit.Location);
                var jsonObj = await _serialiserService.GetSerializedObject(JsonObjects);
                return Json(jsonObj);
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostInstitute(IFormCollection model)
        {
            await _organisationService.PostInstitute(model); 
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganisationSkipperAPI(IFormCollection collection)
        {
            User currentUser = null;
            try
            {
                string FirstName = collection["FirstName"].ToString();
                string Email = collection["Email"].ToString();
                string LastName = collection["LastName"].ToString();
                currentUser = await CurrentUser();
                Guid.TryParse(collection["AnswerSheetId"], out Guid SheetId);
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(SheetId);
                OrganisationType organisationType = new OrganisationType(currentUser, "Person - Individual");
                InsuranceAttribute insuranceAttribute = new InsuranceAttribute(currentUser, "Skipper");
                OrganisationalUnit organisationalUnit = new OrganisationalUnit(currentUser, "Person - Individual");
                InterestedPartyUnit interestedPartyUnit = new InterestedPartyUnit(currentUser, "Skipper", "Person - Individual", null);
                Organisation organisation = new Organisation(currentUser, Guid.NewGuid())
                {
                    OrganisationType = organisationType,
                    Email = Email,
                    Name = FirstName + " " + LastName
                };

                organisation.OrganisationalUnits.Add(organisationalUnit);
                organisation.OrganisationalUnits.Add(interestedPartyUnit);
                organisation.InsuranceAttributes.Add(insuranceAttribute);

                Random random = new Random();
                string UserName = FirstName.Replace(" ", string.Empty)
                    + "_"
                    + LastName.Replace(" ", string.Empty)
                    + random.Next(1000);

                User user = new User(currentUser, UserName)
                {
                    FirstName = collection["FirstName"].ToString(),
                    LastName = collection["LastName"].ToString(),
                    Email = collection["Email"].ToString(),
                    FullName = FirstName + " " + LastName,
                    Id = Guid.NewGuid()
                };
                user.SetPrimaryOrganisation(organisation);

                if (!sheet.Organisation.Contains(organisation))
                {
                    sheet.Organisation.Add(organisation);
                }

                await _clientInformationService.UpdateInformation(sheet);

                return Json(organisation);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganisationInterestedPartyAPI(IFormCollection collection)
        {
            User currentUser = null;
            try
            {
                string FirstName = collection["FirstName"].ToString();
                string Email = collection["OrganisationEmail"].ToString();
                string Name = collection["OrganisationName"].ToString();
                string InsuranceAttribute = collection["InsuranceAttribute"].ToString();
                string OrganisationType = collection["OrganisationTypeName"].ToString();
                currentUser = await CurrentUser();
                Guid.TryParse(collection["AnswerSheetId"], out Guid SheetId);
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(SheetId);
                OrganisationType organisationType = new OrganisationType(currentUser, OrganisationType);
                InsuranceAttribute insuranceAttribute = new InsuranceAttribute(currentUser, InsuranceAttribute);
                OrganisationalUnit organisationalUnit = new OrganisationalUnit(currentUser, OrganisationType);
                InterestedPartyUnit interestedPartyUnit = new InterestedPartyUnit(currentUser, InsuranceAttribute, OrganisationType, null);
                Organisation organisation = new Organisation(currentUser, Guid.NewGuid())
                {
                    OrganisationType = organisationType,
                    Email = Email,
                    Name = Name
                };

                organisation.OrganisationalUnits.Add(organisationalUnit);
                organisation.OrganisationalUnits.Add(interestedPartyUnit);
                organisation.InsuranceAttributes.Add(insuranceAttribute);

                if (!sheet.Organisation.Contains(organisation))
                {
                    sheet.Organisation.Add(organisation);
                }

                await _clientInformationService.UpdateInformation(sheet);

                return Json(organisation);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AttachOrganisation(Guid ProgrammeId, Guid OrganisationId)
        {
            var allClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(ProgrammeId);
            var RemovedOrg = await _organisationService.GetOrganisation(OrganisationId);

            // We want to prepare which clientprogrammes we want to give to our ViewModel, 
            // which is Non deleted and Latest versions of a Programme (ie. Change and Original ones (if original and no change then won't have a nextInformationSheet) 
            var selectedClientProgrammes = new List<ClientProgramme>();
            
            // Add Change ClientProgrammes
            foreach (var clientProgramme in allClientProgrammes.Where(p => p.DateDeleted == null && p.InformationSheet.Status != "Not Taken Up By Broker").Where(p => p.InformationSheet.IsChange == true).ToList())
            {
                bool isClientProgrammeNotSub = await _programmeService.IsBaseClass(clientProgramme);
                if (isClientProgrammeNotSub)
                {
                    selectedClientProgrammes.Add(clientProgramme);
                }
            }
            // Add Original ClientProgrammes
            foreach (var clientProgramme in allClientProgrammes.Where(p => p.DateDeleted == null && p.InformationSheet.Status != "Not Taken Up By Broker").Where(p => p.InformationSheet.IsChange == false).Where(p => p.InformationSheet.NextInformationSheet == null).ToList())
            {
                bool isClientProgrammeNotSub = await _programmeService.IsBaseClass(clientProgramme);
                if (isClientProgrammeNotSub)
                {
                    selectedClientProgrammes.Add(clientProgramme);
                }
            }
            AttachOrganisationViewModel model = new AttachOrganisationViewModel(selectedClientProgrammes, RemovedOrg);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AttachOrganisation(IFormCollection collection)
        {
            User currentUser = await CurrentUser();
            ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse(collection["ClientProgrammeId"]));
            if (clientProgramme.InformationSheet.Status == "Submitted")
            {
                clientProgramme.InformationSheet.Status = "Started";
                await _programmeService.Update(clientProgramme);
            }
            else if (clientProgramme.InformationSheet.Status == "Bound")
            {
                Dictionary<string, string> changeDefaults = new Dictionary<string, string>();
                changeDefaults.Add("ChangeType", "Amend or Add NamedParty");
                changeDefaults.Add("Reason", "Change in cover requirements");
                changeDefaults.Add("ReasonDesc", "Reattach Advisor");
                changeDefaults.Add("ClientProgrammeID", Guid.Parse(collection["ClientProgrammeId"]).ToString());
                clientProgramme = await _programmeService.CloneForUpdate(currentUser, null, changeDefaults);                
            }

            await _clientInformationService.DetachOrganisation(collection);

            // here we update the emails we've saved in form - can put in service
            if (Guid.TryParse(collection["RemovedOrganisation.Id"], out Guid AttachOrganisationId))
            {
                if (AttachOrganisationId != Guid.Empty)
                {
                    var Organisation = await _organisationService.GetOrganisation(AttachOrganisationId);
                    var User = await _userService.GetUserPrimaryOrganisationOrEmail(Organisation);
                    if (User != null) //&& User.Email != collection["RemovedOrganisation.Email"]
                    {
                        User.PrimaryOrganisation.Removed = false;
                        User.Email = collection["RemovedOrganisation.Email"];
                        User.PrimaryOrganisation = Organisation;
                        await _userService.Update(User);
                    }
                    if (Organisation != null && Organisation.Email != collection["RemovedOrganisation.Email"])
                    {
                        Organisation.Email = collection["RemovedOrganisation.Email"];
                        await _organisationService.Update(Organisation);
                    }
                }
            }

            await _programmeService.AttachOrganisationToClientProgramme(collection, clientProgramme);
            Organisation organisation = await _organisationService.GetOrganisation(Guid.Parse(collection["RemovedOrganisation.Id"]));
            await _milestoneService.CompleteAttachOrganisationTask(currentUser, clientProgramme.BaseProgramme, organisation);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePrincipalAdvisors(IFormCollection collection)
        {
            User currentUser = await CurrentUser();
            Guid Id = Guid.Parse(collection["ClientInformationSheet.Id"]);
            string Name = "Advisor";
            ClientInformationSheet Sheet = await _clientInformationService.GetInformation(Id);
            foreach(var organisation in Sheet.Organisation)
            {
                var advisorUnit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(i => i.Name == Name);
                if(advisorUnit != null)
                {
                    advisorUnit.IsPrincipalAdvisor = false;
                }

                await _organisationService.Update(organisation);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> IfFapExist(Guid clientInformationSheet)
        {
            User currentUser = await CurrentUser();
            ClientInformationSheet Sheet = await _clientInformationService.GetInformation(clientInformationSheet);
            Boolean IfFapExistflag = false;
            foreach (var organisation in Sheet.Organisation)
            {
                var advisorUnit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(i => i.Name == "Advisor");
                if (advisorUnit != null && advisorUnit.isTheFAP)
                {
                    IfFapExistflag = true;
                }

                var administratorUnit = (AdministratorUnit)organisation.OrganisationalUnits.FirstOrDefault(i => i.Name == "Administrator");
                if (administratorUnit != null && administratorUnit.isAdministratorTheFAP)
                {
                    IfFapExistflag = true;
                }

                if (organisation.isOrganisationTheFAP) {
                    IfFapExistflag = true;
                }

            }
            if (!IfFapExistflag)
            {
                throw new ArgumentException("Please Add atleastone FAP Org"); ;
            }

            return Json(IfFapExistflag);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveIsTheFAPS(IFormCollection collection)
        {
            User currentUser = await CurrentUser();
            Guid Id = Guid.Parse(collection["ClientInformationSheet.Id"]);
            //string Name = "Advisor";
            ClientInformationSheet Sheet = await _clientInformationService.GetInformation(Id);
            foreach (var organisation in Sheet.Organisation)
            {
                var advisorUnit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(i => i.Name == "Advisor");
                if (advisorUnit != null)
                {
                    advisorUnit.isTheFAP = false;
                }

                var administratorUnit = (AdministratorUnit)organisation.OrganisationalUnits.FirstOrDefault(i => i.Name == "Administrator");
                if (administratorUnit != null)
                {
                    administratorUnit.isAdministratorTheFAP = false;
                }

                organisation.isOrganisationTheFAP = false;

                await _organisationService.Update(organisation);
            }

            return Ok();
        }

        public async Task<IActionResult> SetPrimary(Guid id)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Organisation org = user.Organisations.FirstOrDefault(o => o.Id == id);
                if (org != null)
                {
                    user.SetPrimaryOrganisation(org);
                    await _userService.Update(user);
                }

                return Redirect("~/Organisation/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> RemoveOrganisation(IFormCollection collection)
        {
            User user = await CurrentUser();
            Guid Id = Guid.Parse(collection["OrganisationId"]);
            Organisation organisation = await _organisationService.GetOrganisation(Id);
            organisation.Removed = true;
            await _organisationService.Update(organisation);
            ClientInformationSheet clientInformationSheet = await _clientInformationService.GetInformation(Guid.Parse(collection["ClientInformationId"]));
           
            if(clientInformationSheet != null)
            {
                if (clientInformationSheet.IsChange)
                {                    
                    var organisationUser = await _userService.GetUserPrimaryOrganisationOrEmail(organisation);

                    if (organisationUser != null)
                    {
                        Guid.TryParse(collection["ProgrammeId"].ToString(), out Guid ProgrammeId);
                        var Programme = await _programmeService.GetProgramme(ProgrammeId);
                        await _milestoneService.CreateJoinOrganisationTask(user, organisationUser, Programme, organisation);
                        if (Programme.ProgEnableEmail)
                        {
                            await _emailService.RemoveOrganisationUserEmail(organisationUser, Programme.BrokerContactUser, clientInformationSheet);
                        }
                    }
                    else
                    {
                        // Case where there is no User for the Organisation being removed
                        // If you want it to not end up in ListRemoved then uncomment below two lines
                        //organisation.Removed = false;
                        //await _organisationService.Update(organisation);
                        throw new ArgumentException("organisationUser cannot be null");
                    }
                }
                
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RestoreOrganisation(IFormCollection collection)
        {
            Guid Id = Guid.Parse(collection["OrganisationId"]);
            Organisation organisation = await _organisationService.GetOrganisation(Id);
            organisation.Removed = false;
            await _organisationService.Update(organisation);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> RejoinProgramme(Guid ProgrammeId, Guid OrganisationId)
        {
            User user = await CurrentUser();
            Programme programme = await _programmeService.GetProgrammeById(ProgrammeId);
            Organisation organisation = await _organisationService.GetOrganisation(OrganisationId);
            await _milestoneService.CreateAttachOrganisationTask(user, programme, organisation);
            return RedirectToAction("Index", "Home");
        }

    }
}
