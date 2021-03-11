#region Using
using AutoMapper;
using ClosedXML.Excel;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
#endregion

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        UserManager<IdentityUser> _userManager;
        IClientInformationService _customerInformationService;
        IPrivateServerService _privateServerService;
        ITaskingService _taskingService;
        IClientInformationService _clientInformationService;
        IClientAgreementService _clientAgreementService;
        IHttpClientService _httpClientService;
        IMapper _mapper;
        IAppSettingService _appSettingService;
        IEmailService _emailService;
        IProgrammeService _programmeService;
        IProductService _productService;
        ILogger<HomeController> _logger;
        IApplicationLoggingService _applicationLoggingService;
        IOrganisationService _organisationService;
        IClientInformationAnswerService _clientInformationAnswer;
        IUnitOfWork _unitOfWork;

        public HomeController(
            UserManager<IdentityUser> userManager,
            IOrganisationService organisationService,
            IAppSettingService appSettingService,
            IEmailService emailService,
            IMapper mapper,
            IApplicationLoggingService applicationLoggingService,
            ILogger<HomeController> logger,
            IProductService productService,
            IProgrammeService programmeService,
            IUserService userRepository,
            IHttpClientService httpClientService,
            ITaskingService taskingService,
            IClientInformationService customerInformationService,
            IPrivateServerService privateServerService,
            IClientAgreementService clientAgreementService,
            IClientInformationService clientInformationService,
            IUnitOfWork unitOfWork,
            IClientInformationAnswerService clientInformationAnswer


            )

            : base(userRepository)
        {
            _userManager = userManager;
            _organisationService = organisationService;
            _appSettingService = appSettingService;
            _emailService = emailService;
            _applicationLoggingService = applicationLoggingService;
            _logger = logger;
            _productService = productService;
            _programmeService = programmeService;
            _httpClientService = httpClientService;
            _customerInformationService = customerInformationService;
            _privateServerService = privateServerService;
            _taskingService = taskingService;
            _clientInformationService = clientInformationService;
            _clientAgreementService = clientAgreementService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clientInformationAnswer = clientInformationAnswer;

        }

        // GET: home/index
        public async Task<IActionResult> Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "DealEngine Dashboard";

            DashboardViewModel model = new DashboardViewModel();
            model.ProductItems = new List<ProductItemV2>();
            model.DealItems = new List<ProductItem>();

            User user = null;
            try
            {
                user = await CurrentUser();
                model.UserTasks = user.UserTasks.Where(t=>t.Completed == false && t.Removed == false).ToList();
                model.DisplayDeals = true;
                model.DisplayProducts = false;
                model.CurrentUserType = "Client";
                if (user.PrimaryOrganisation.IsBroker)
                {
                    model.CurrentUserType = "Broker";
                }
                if (user.PrimaryOrganisation.IsInsurer)
                {
                    model.CurrentUserType = "Insurer";
                }
                if (user.PrimaryOrganisation.IsTC)
                {
                    model.CurrentUserType = "TC";
                }
                if (user.PrimaryOrganisation.IsProgrammeManager)
                {
                    model.CurrentUserType = "ProgrammeManager";
                }

                IList<string> languages = new List<string>();
                languages.Add("nz");
                IList<Programme> programmeList = new List<Programme>();
                model.ProgrammeItems = new List<ProgrammeItem>();
                if (model.CurrentUserType == "Client")
                {
                    var clientProgList = _programmeService.GetClientProgrammesByOwner(user.PrimaryOrganisation.Id).Result.GroupBy(bp => bp.BaseProgramme);
                    foreach (var clientProgramme in clientProgList)
                    {
                        programmeList.Add(clientProgramme.Key);
                    }
                }
                else
                {
                    programmeList = await _programmeService.GetAllProgrammes();
                }

                foreach (Programme programme in programmeList)
                {
                    model.ProgrammeItems.Add(new ProgrammeItem(programme)
                    {
                        Languages = languages
                    });
                }

                return View("IndexNew", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        #region Search
        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var Programmes = await _programmeService.GetAllProgrammes();
            SearchViewModel model = new SearchViewModel(Programmes);
            return View("Search", model);
        }

        [HttpPost]
        public async Task<IActionResult> ViewProgramme(IFormCollection collection)
        {
            
            var sheets = await _programmeService.SearchProgrammes(collection);
            ProgrammeItem model = new ProgrammeItem(sheets);

            //if (searchTerm == "Boat")
            //{
            //model.Deals = await GetBoatNameSearch(searchValue);
            //}
            //if (searchTerm == "Name")
            //{
            //    model.Deals = await GetClientNameSearch(searchValue);
            //}
            //if (searchTerm == "Reference")
            //{
            //    model.Deals = await GetReferenceSearch(searchValue);
            //}

            return View(model);
        }

        //private async Task<IList<DealItem>> GetReferenceSearch(string searchValue)
        //{
        //    List<DealItem> deals = new List<DealItem>();

        //    var informationForList = await _clientInformationService.GetAllInformationFor(searchValue);
        //    foreach (ClientInformationSheet sheet in informationForList)
        //    {
        //        ClientProgramme client = sheet.Programme;

        //        string status = client.InformationSheet.Status;
        //        string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");
        //        string localDateSubmitted = null;

        //        if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
        //        {
        //            localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy");
        //        }

        //        deals.Add(new DealItem
        //        {
        //            Id = client.Id.ToString(),
        //            Name = sheet.Programme.BaseProgramme.Name + " for " + client.Owner.Name,
        //            LocalDateCreated = localDateCreated,
        //            LocalDateSubmitted = localDateSubmitted,
        //            Status = status,
        //            SubClientProgrammes = client.SubClientProgrammes,
        //            ReferenceId = client.InformationSheet.ReferenceId// Move into ClientProgramme?
        //        });
        //    }

        //    ClientAgreement agreement = await _clientAgreementService.GetAgreementbyReferenceNum(searchValue);

        //    if (agreement != null)
        //    {
        //        ClientInformationSheet sheet2 = await _clientInformationService.GetInformation(agreement.ClientInformationSheet.Id);

        //        if (sheet2 != null)
        //        {
        //            ClientProgramme client = sheet2.Programme;

        //            string status = client.InformationSheet.Status;
        //            string referenceid = client.InformationSheet.ReferenceId;
        //            string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");//"dd/MM/yyyy h:mm tt"
        //            string localDateSubmitted = null;

        //            if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
        //            {
        //                localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy");
        //            }

        //            deals.Add(new DealItem
        //            {
        //                Id = client.Id.ToString(),
        //                Name = sheet2.Programme.BaseProgramme.Name + " for " + client.Owner.Name,
        //                LocalDateCreated = localDateCreated,
        //                LocalDateSubmitted = localDateSubmitted,
        //                Status = status,
        //                SubClientProgrammes = sheet2.Programme.SubClientProgrammes,
        //                ReferenceId = referenceid// Move into ClientProgramme?
        //            });

        //        }
        //    }

        //    return deals;
        //}

        //private async Task<IList<DealItem>> GetClientNameSearch(string searchValue)
        //{
        //    List<DealItem> deals = new List<DealItem>();
        //    //List<ClientProgramme> clients = await _programmeService.FindByOwnerName(searchValue);

        //    if (clients.Count != 0)
        //    {
        //        foreach (var client in clients)
        //        {
        //            string status = client.InformationSheet.Status;
        //            string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");
        //            string localDateSubmitted = null;

        //            if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
        //            {
        //                localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy");
        //            }

        //            deals.Add(new DealItem
        //            {
        //                Id = client.Id.ToString(),
        //                Name = client.BaseProgramme.Name + " for " + client.Owner.Name,
        //                ProgrammeAllowUsesChange = client.BaseProgramme.AllowUsesChange,
        //                LocalDateCreated = localDateCreated,
        //                LocalDateSubmitted = localDateSubmitted,
        //                Status = status,
        //                ReferenceId = client.InformationSheet.ReferenceId,// Move into ClientProgramme?
        //                SubClientProgrammes = client.SubClientProgrammes,
        //            });
        //        }
        //    }

        //    return deals;
        //}

        //private async Task<IList<DealItem>> GetAdvisoryNameSearch(IFormCollection collection, List<Programme> programmes)
        //{
        //    List<DealItem> deals = new List<DealItem>();
        //    //List<ClientInformationSheet> clients = await _clientInformationService.FindByAdvisoryName(collection);
        //    if (clients.Count != 0)
        //    {
        //        foreach (var client in clients)
        //        {
        //            string status = client.Status;
        //            string localDateCreated = LocalizeTime(client.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");
        //            string localDateSubmitted = null;

        //            if (client.Status != "Not Started" && client.Status != "Started")
        //            {
        //                localDateSubmitted = LocalizeTime(client.SubmitDate, "dd/MM/yyyy");
        //            }

        //            deals.Add(new DealItem
        //            {
        //                Id = client.Programme.Id.ToString(),
        //                Name = client.Programme.BaseProgramme.Name + " for " + client.Owner.Name,
        //                ProgrammeAllowUsesChange = client.Programme.BaseProgramme.AllowUsesChange,
        //                LocalDateCreated = localDateCreated,
        //                LocalDateSubmitted = localDateSubmitted,
        //                Status = status,
        //                ReferenceId = client.ReferenceId,// Move into ClientProgramme?
        //                SubClientProgrammes = client.Programme.SubClientProgrammes,
        //            });
        //        }
        //    }

        //    return deals;
        //}

        //private async Task<IList<DealItem>> GetBoatNameSearch(string searchValue)
        //{
        //    List<DealItem> deals = new List<DealItem>();
        //    //List<ClientInformationSheet> clients = await _clientInformationService.FindByBoatName(searchValue);

        //    if (clients.Count != 0)
        //    {
        //        foreach (var client in clients)
        //        {
        //            string status = client.Status;
        //            string localDateCreated = LocalizeTime(client.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");
        //            string localDateSubmitted = null;

        //            if (client.Status != "Not Started" && client.Status != "Started")
        //            {
        //                localDateSubmitted = LocalizeTime(client.SubmitDate, "dd/MM/yyyy");
        //            }

        //            deals.Add(new DealItem
        //            {
        //                Id = client.Programme.Id.ToString(),
        //                Name = client.Programme.BaseProgramme.Name + " for " + client.Owner.Name,
        //                ProgrammeAllowUsesChange = client.Programme.BaseProgramme.AllowUsesChange,
        //                LocalDateCreated = localDateCreated,
        //                LocalDateSubmitted = localDateSubmitted,
        //                Status = status,
        //                ReferenceId = client.ReferenceId,// Move into ClientProgramme?
        //                SubClientProgrammes = client.Programme.SubClientProgrammes,
        //            });
        //        }
        //    }

        //    return deals;
        //}


        #endregion Search

        [HttpGet]
        public async Task<IActionResult> ViewSubClientProgrammes(string clientProgrammeId)
        {            
            User user = null;
            var clientList = new List<ClientProgramme>();
            try
            {
                user = await CurrentUser();
                ClientProgramme clientprogramme = await _programmeService.GetClientProgramme(Guid.Parse(clientProgrammeId));
                if (clientprogramme.SubClientProgrammes.Any())
                {
                    foreach (var client in clientprogramme.SubClientProgrammes)
                    {
                        clientList.Add(client);
                    }
                }
                else
                {
                    clientList.Add(clientprogramme);
                }
                ProgrammeItem model = new ProgrammeItem(clientList.FirstOrDefault().BaseProgramme);
                model = await GetClientProgrammeListModel(user, clientList, clientList.FirstOrDefault().BaseProgramme, true);

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        private async Task<ProgrammeItem> GetClientProgrammeListModel(User user, IList<ClientProgramme> clientList, Programme programme, bool isClient = false)
        {
            var clientProgramme = clientList.FirstOrDefault();
            //ProgrammeItem model = new ProgrammeItem(clientProgramme.BaseProgramme);
            ProgrammeItem model = new ProgrammeItem(programme);
            if (clientProgramme != null)
            {
                if (!isClient)
                {
                    var isBaseClientProg = await _programmeService.IsBaseClass(clientProgramme);
                    if (isBaseClientProg)
                    {
                        clientList = await _programmeService.GetClientProgrammesForProgramme(clientProgramme.BaseProgramme.Id);
                    }
                }
            }
            if (user.PrimaryOrganisation.IsBroker || user.PrimaryOrganisation.IsInsurer || user.PrimaryOrganisation.IsTC || user.PrimaryOrganisation.IsProgrammeManager)
            {
                Boolean Issubclientsubmitted = false;
                foreach (ClientProgramme client in clientList.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    if (client.InformationSheet != null)
                    {                       
                        string status = client.InformationSheet.Status;
                        string referenceId = client.InformationSheet.ReferenceId;
                        bool nextInfoSheet = false;
                        string agreementSatus = "";
                        string DocSendDate = "";
                        foreach (ClientAgreement agreement in client.Agreements)
                        {
                            if (agreement.ClientInformationSheet.Status != "Not Started" && agreement.ClientInformationSheet.Status != "Started" && agreement.DateDeleted == null && (agreement.Status == "Referred" || agreement.Status == "Authorised"))
                            {
                                if (agreement.Status == "Referred") {
                                    agreementSatus = "Referred";
                                }
                                else if(agreement.Status == "Authorised")
                                {
                                    agreementSatus = "Authorised";
                                }
                                
                                break;
                            }
                            if (agreement.IsPolicyDocSend)
                                DocSendDate = ", Document Issued on: " + agreement.DocIssueDate;
                        }
                        if (null != client.InformationSheet.NextInformationSheet)
                        {
                            nextInfoSheet = true;
                        }

                        string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy h:mm tt");
                        string localDateSubmitted = null;

                        if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
                        {
                            localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy h:mm tt");
                        }
                        if (client.SubClientProgrammes.Count > 0)
                        {
                            Issubclientsubmitted = true;
                        }
                        try
                        {
                            if (client.SubClientProgrammes.Any(s => s.InformationSheet.Status != "Submitted"))
                            {
                                Issubclientsubmitted = false;
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                            
                        //for (var index = 0; index < client.SubClientProgrammes.Count; index++)
                        //{
                        //    if (client.SubClientProgrammes[index].InformationSheet.Status != "Submitted")
                        //        Issubclientsubmitted = false;

                        //}
                        model.Deals.Add(new DealItem
                        {
                            Id = client.Id.ToString(),
                            Name = client.BaseProgramme.Name + " for " + client.Owner.Name,
                            NextInfoSheet = nextInfoSheet,
                            LocalDateCreated = localDateCreated,
                            LocalDateSubmitted = localDateSubmitted,
                            Status = status,
                            IsChange = client.InformationSheet.IsChange,
                            ReferenceId = referenceId,// Move into ClientProgramme?
                            SubClientProgrammes = client.SubClientProgrammes,
                            AgreementStatus = agreementSatus,
                            IsSubclientSubmitted = Issubclientsubmitted,
                            DocSendDate = DocSendDate
                        });
                    }

                }
            }
            else
            {
                clientList = await _programmeService.GetClientProgrammesByOwner(user.PrimaryOrganisation.Id);
                foreach (ClientProgramme client in clientList.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    string status = client.InformationSheet.Status;
                    string referenceId = client.InformationSheet.ReferenceId;
                    bool nextInfoSheet = false;
                    bool programmeAllowUsesChange = false;
                    bool programmeUseEglobal = false;
                    string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy h:mm tt");
                    string localDateSubmitted = null;
                    string agreementSatus = "";
                    foreach (ClientAgreement agreement in client.Agreements)
                    {
                        if (agreement.ClientInformationSheet.Status != "Not Started" && agreement.ClientInformationSheet.Status != "Started" && agreement.DateDeleted == null && agreement.Status == "Referred")
                        {
                            agreementSatus = "Referred";
                            break;
                        }
                    }
                    if (client.BaseProgramme.AllowUsesChange)
                    {
                        programmeAllowUsesChange = true;
                    }
                    if (client.BaseProgramme.UsesEGlobal)
                    {
                        programmeUseEglobal = true;
                    }

                    if (null != client.InformationSheet.PreviousInformationSheet)
                    {
                        nextInfoSheet = true;
                    }

                    if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
                    {
                        localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy h:mm tt");
                    }

                    model.Deals.Add(new DealItem
                    {
                        Id = client.Id.ToString(),
                        Name = client.BaseProgramme.Name + " for " + client.Owner.Name,
                        NextInfoSheet = nextInfoSheet,
                        IsChange = client.InformationSheet.IsChange,
                        ProgrammeAllowUsesChange = programmeAllowUsesChange,
                        ProgrammeUseEglobal = programmeUseEglobal,
                        LocalDateCreated = localDateCreated,
                        LocalDateSubmitted = localDateSubmitted,
                        Status = status,
                        ReferenceId = referenceId,// Move into ClientProgramme?
                        SubClientProgrammes = client.SubClientProgrammes,
                        AgreementStatus = agreementSatus
                    }); ;
                }
            }
            if (user.PrimaryOrganisation.IsBroker)
            {
                model.CurrentUserIsBroker = "True";
            }
            else
            {
                model.CurrentUserIsBroker = "False";
            }
            if (user.PrimaryOrganisation.IsInsurer)
            {
                model.CurrentUserIsInsurer = "True";
            }
            else
            {
                model.CurrentUserIsInsurer = "False";
            }
            if (user.PrimaryOrganisation.IsTC)
            {
                model.CurrentUserIsTC = "True";
            }
            else
            {
                model.CurrentUserIsTC = "False";
            }
            if (user.PrimaryOrganisation.IsProgrammeManager)
            {
                model.CurrentUserIsProgrammeManager = "True";
            }
            else
            {
                model.CurrentUserIsProgrammeManager = "False";
            }

            return model;
        }

        [HttpGet]
        public async Task<IActionResult> ViewSubClientProgramme(Guid subClientProgrammeId)
        {
            User user = null;
            var clientList = new List<ClientProgramme>();
            try
            {
                user = await CurrentUser();
                SubClientProgramme subClientprogramme = await _programmeService.GetSubClientProgrammebyId(subClientProgrammeId);
                clientList.Add(subClientprogramme);
                ProgrammeItem model = new ProgrammeItem(clientList.FirstOrDefault().BaseProgramme);
                model = await GetClientProgrammeListModel(user, clientList, clientList.FirstOrDefault().BaseProgramme);
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewProgramme(Guid id)
        {
            List<DealItem> deals = new List<DealItem>();
            User user = null;
            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(id);
                var clientList = await _programmeService.GetClientProgrammesByOwner(user.PrimaryOrganisation.Id);
                if (!clientList.Any())
                {
                    clientList = await _programmeService.GetClientProgrammesForProgramme(id);
                }
                //ProgrammeItem model = new ProgrammeItem(clientList.FirstOrDefault().BaseProgramme);
                ProgrammeItem model = new ProgrammeItem(programme);
                model = await GetClientProgrammeListModel(user, clientList, programme);
                model.IsSubclientEnabled = programme.HasSubsystemEnabled;

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IssueUIS(string ProgrammeId, string actionname)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                IssueUISViewModel model = new IssueUISViewModel();
                var clientProgrammes = new List<ClientProgramme>();
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));
                List<ClientProgramme> mainClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(programme.Id);
                List<ClientProgramme> subClientProgrammes = await _programmeService.GetSubClientProgrammesForProgramme(programme.Id);

                foreach (var client in mainClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    if (client.DateDeleted == null && client.InformationSheet.Status != "Bound")
                    {
                        clientProgrammes.Add(client);
                    }
                }
                model.ClientProgrammes = clientProgrammes;
                model.ProgrammeId = ProgrammeId;
                model.IsSubUIS = "false";
                if (actionname == "IssueUIS")
                {
                    return View(model);
                }
                else
                {
                    return View("EditClient", model);
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IssueSubUIS(string ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                IssueUISViewModel model = new IssueUISViewModel();
                var clientProgrammes = new List<ClientProgramme>();
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));
                List<ClientProgramme> subClientProgrammes = await _programmeService.GetSubClientProgrammesForProgramme(programme.Id);

                foreach (var client in subClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    if (client.DateDeleted == null && (client.InformationSheet.Status == "Started" || client.InformationSheet.Status == "Not Started"))
                    {
                        clientProgrammes.Add(client);
                    }
                }
                model.IsSubUIS = "true";
                model.ClientProgrammes = clientProgrammes;
                model.ProgrammeId = ProgrammeId;

                return View("IssueUIS", model);


            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IssueReminder(string ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                IssueUISViewModel model = new IssueUISViewModel();
                var clientProgrammes = new List<ClientProgramme>();
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));

                foreach (var client in programme.ClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    if (client.DateDeleted == null && (client.InformationSheet.Status == "Started" || client.InformationSheet.Status == "Not Started"))
                    {
                        clientProgrammes.Add(client);
                    }
                }

                model.ClientProgrammes = clientProgrammes;
                model.ProgrammeId = ProgrammeId;

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        ////[HttpGet]
        ////public async Task<DataTable> ToDataTable(this IList<User> data)
        ////{
        ////    PropertyDescriptorCollection properties =
        ////        TypeDescriptor.GetProperties(typeof(User));
        ////    DataTable table = new DataTable();
        ////    foreach (PropertyDescriptor prop in properties)
        ////        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        ////    foreach (User item in data)
        ////    {
        ////        DataRow row = table.NewRow();
        ////        foreach (PropertyDescriptor prop in properties)
        ////            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
        ////        table.Rows.Add(row);
        ////    }
        ////    return table;
        ////}
        ///
        [HttpGet]
        public async Task<IActionResult> ReportView(string ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                IssueUISViewModel model = new IssueUISViewModel();
                model.ProgrammeId = ProgrammeId;
                Programme programme = await _programmeService.GetProgrammeById(new Guid(ProgrammeId));

                model.ProgrammeName = programme.Name;
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public PropertyDescriptorCollection generatequeryField(string Query)
        {
            PropertyDescriptorCollection props = null ;

            if (Query == "PI Cover Limit")
            {
                props = TypeDescriptor.GetProperties(typeof(PIReport));
            }
            if (Query == "ED Premium")
            {
                props = TypeDescriptor.GetProperties(typeof(PIReport));
            }
            if (Query == "Cyber Premium")
            {
                props = TypeDescriptor.GetProperties(typeof(PIReport));
            }
            if (Query == "Limit and Premium Breakdown")
            {
                props = TypeDescriptor.GetProperties(typeof(PIReport));
            }
            if (Query == "FAP")
            {
                props = TypeDescriptor.GetProperties(typeof(FAPReport));
            }

            return props;
        }

        public async Task<List<List<string>>> GetNZFGReportSet(Guid programmeId , string reportName)
        {
            Programme programme =  await _programmeService.GetProgrammeById(programmeId);
            List<List<string>> ListReportSet = new List<List<string>>();
            List<String> ListReport = new List<String>();
            ListReport.Add("Member Name");
            ListReport.Add("Status");
            ListReport.Add("Email");
            ListReport.Add("Advisor Name");
            ListReport.Add("Are you coming under NZFSG/Loan Markets Transitional Licence?");
            ListReport.Add("Please select from the following options(if selected yes)");
            ListReport.Add("Please select from the following options (if selected no)");
            ListReportSet.Add(ListReport);

            foreach (ClientProgramme cp in programme.ClientProgrammes.Where(o =>  o.InformationSheet.DateDeleted == null))
            {
                try
                {
                    if (reportName == "NZFSGFAP")
                    {
                        ListReport = new List<String>();

                        Organisation organisation = cp.InformationSheet.Owner;
                        ////adding collumns to ListReport
                        

                        ListReport.Add(organisation.Name);
                        ListReport.Add(cp.InformationSheet.Status);
                        ListReport.Add(organisation.Email);
                        Guid clientInformationSheetID = Guid.NewGuid();
                        User user = await _userService.GetUserPrimaryOrganisationOrEmail(organisation);
                        if (user.FullName != null)
                        {
                            ListReport.Add(user.FullName);
                        }
                        else{
                            ListReport.Add(organisation.Name);

                        }

                        //if(programme.Name == "NZFSG Programme")
                        //{
                            if(cp.BaseProgramme.Id == programme.Id)
                            {
                                clientInformationSheetID = cp.InformationSheet.Id;

                            }
                            ClientInformationAnswer TraditionalLicenceOptionsAnswers = await _clientInformationAnswer.GetSheetAnsByName("FAPViewModel.HasTraditionalLicenceOptions" , clientInformationSheetID);
                            ClientInformationAnswer AdvisersOptionsAnswers = await _clientInformationAnswer.GetSheetAnsByName("FAPViewModel.HasAdvisersOptions", clientInformationSheetID);
                            ClientInformationAnswer AdditionalTraditionalLicenceOptionsAnswers = await _clientInformationAnswer.GetSheetAnsByName("FAPViewModel.HasAdditionalTraditionalLicenceOptions", clientInformationSheetID);
                            //TraditionalLicenceOptionsAnswers.Value == "0" ? ListReport.Add("Not Selected") : (TraditionalLicenceOptionsAnswers.Value == "1") ? ListReport.Add("Yes") : (TraditionalLicenceOptionsAnswers.Value == "2") ? ListReport.Add("No") ;
                            if (TraditionalLicenceOptionsAnswers.Value == "0" )
                            {
                                ListReport.Add("Not Selected");
                            } else if (TraditionalLicenceOptionsAnswers.Value == "1")
                            {
                                ListReport.Add("Yes");
                            }
                            else if (TraditionalLicenceOptionsAnswers.Value == "2")
                            {
                                ListReport.Add("No");
                            }



                            if (AdvisersOptionsAnswers.Value == "0")
                            {
                                ListReport.Add("Not Selected");
                            }
                            else if (AdvisersOptionsAnswers.Value == "1")
                            {
                                ListReport.Add("I do not have any other advisers working under my license");
                            }
                            else if (AdvisersOptionsAnswers.Value == "2")
                            {
                                ListReport.Add("I do have other advisers working under my license");
                            }




                            if (AdditionalTraditionalLicenceOptionsAnswers.Value == "0")
                            {
                                ListReport.Add("Not Selected");
                            }
                            else if (AdditionalTraditionalLicenceOptionsAnswers.Value == "1")
                            {
                                ListReport.Add("I am taking my own Transitional Licence with no other advisers working under my license");
                            }
                            else if (AdditionalTraditionalLicenceOptionsAnswers.Value == "2")
                            {
                                ListReport.Add("I will be coming under someone elses Transitional Licence");
                            }
                            else if (AdditionalTraditionalLicenceOptionsAnswers.Value == "3")
                            {
                                ListReport.Add("Undecided");
                            }
                       // }

                        ListReportSet.Add(ListReport);


                    }
                   // if (reportName == "PI")
                   // {
                    //PIReport report = new PIReport();
                    //report.ReferenceID = cp.InformationSheet.ReferenceId;
                    //report.IndividualName = cp.Owner.Name;
                    //report.CompanyName = cp.Owner.Name;

                    //if (cp.Agreements.Count > 0)
                    //{
                    //    foreach (ClientAgreement agreement in cp.Agreements)
                    //    {
                    //        var term = agreement.ClientAgreementTerms.FirstOrDefault(ter => ter.SubTermType == queryselect && ter.Bound == true);
                    //        if (term != null)
                    //        {
                    //            report.selectedlimit = term.TermLimit.ToString();
                    //            report.Premium = term.Premium.ToString();
                    //            report.Inceptiondate = agreement.InceptionDate.ToString();
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            report.selectedlimit = "0";
                    //            report.Premium = "0";
                    //            report.Inceptiondate = agreement.InceptionDate.ToString();
                    //            break;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    report.selectedlimit = "0";
                    //    report.Premium = "0";
                    //    report.Inceptiondate = "0";
                    //}
                    //ListReport.Add(report);
                    // }
                   // else
                    //{

                   // }

                }
                catch (Exception ex)
                { }
            }
            return ListReportSet;
        }

        [HttpPost]
        public async Task<IActionResult> GetReportView(IFormCollection formCollection , string IsReport)
        {
            User user = null;
            try
            {
                Guid ProgrammeId = Guid.Parse(formCollection["ProgrammeId"]);
                string queryselect = formCollection["queryselect"];
                ViewBag.reportName = queryselect;
                ViewBag.ProgrammeId = Guid.Parse(formCollection["ProgrammeId"]);
                ViewBag.Title = queryselect + " Report";
                PropertyDescriptorCollection props = generatequeryField(queryselect);

                List<PIReport> reportset = new List<PIReport>();
                DataTable table = new DataTable();
                //List<String> ListReport = new List<String>();
                List<List<string>> Lreportset = new List<List<string>>();
                if (queryselect  == "NZFSGFAP")
                {
                   Lreportset = await GetNZFGReportSet(ProgrammeId, queryselect);

                }


                try
                {
                    for (int i = 0; i < Lreportset[0].Count; i++)
                    {
                        table.Columns.Add(Lreportset[0][i]);
                    }

                }
                catch (Exception ex)
                {
                    if (table.Columns.Contains("Id"))
                        table.Columns.Remove("Id");
                }

                //object[] values = new object[props.Count];
                object[] values1 = new object[table.Columns.Count];

                for (int i = 1; i < Lreportset.Count-1; i++)
                {
                    try
                    {

                        var count = 0;
                        for (int j = 0; j < Lreportset[i].Count; j++)
                        {
                            try
                            {
                                var val = Lreportset[i].ElementAt(j);

                                if (val != null)
                                {
                                    values1[count] = val;
                                    count++;
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        table.Rows.Add(values1);

                    }
                    catch (Exception ex)
                    {
                    }
                }

                
                if (IsReport != "True")
                {
                    return View(table);
                }
                else
                {
                    table.TableName = "MyDt";
                    try
                    {
                      XLWorkbook workbook = new XLWorkbook();
                      workbook.Worksheets.Add(table, "WorksheetName");
                      // wb.SaveAs(@"C:\\Users\\Public\\DataImport\\Students1.xlsx");

                        //Defining the ContentType for excel file.
                        string ContentType = "Application/msexcel";

                        //Define the file name.
                        string fileName = queryselect+ "Report.xlsx";

                        //Creating stream object.
                        MemoryStream stream = new MemoryStream();

                        //Saving the workbook to stream in XLSX format
                        workbook.SaveAs(stream);

                        stream.Position = 0;

                        return File(stream, ContentType, fileName);

                    }
                    catch (Exception ex)
                    {

                    }

                    return View(table);

                }
                   
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> IssueUIS(IFormCollection formCollection)
        {
            User user = null;
            Programme programme = null;
            string email = null;

            try
            {
                user = await CurrentUser();
                programme = await _programmeService.GetProgramme(Guid.Parse(formCollection["ProgrammeId"]));
                var isSubUis = formCollection["IsSubUIS"];
                foreach (var key in formCollection.Keys)
                {

                    email = key;
                    var correctEmail = await _userService.GetUserByEmail(email);
                    if (correctEmail != null)
                    {
                        if (programme.ProgEnableEmail)
                        {
                            var clientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse(formCollection[key]));
                            clientProgramme.IssueDate = DateTime.Now;
                            await _programmeService.Update(clientProgramme);

                            //send out login instruction email
                            await _emailService.SendSystemEmailLogin(email);
                            //send out information sheet instruction email
                            EmailTemplate emailTemplate = null;

                            if (isSubUis.Contains("true"))
                            {
                                emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == "SendSubInformationSheetInstruction");
                            }
                            else
                            {
                                emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == "SendInformationSheetInstruction");
                            }
                            if (emailTemplate != null)
                            {
                                await _emailService.SendEmailViaEmailTemplate(email, emailTemplate, null, null, null);
                            }
                            //send out uis issue notification email
                            //await _emailService.SendSystemEmailUISIssueNotify(programme.BrokerContactUser, programme, sheet, programme.Owner);
                        }
                    }

                }

                return await RedirectToLocal();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditClients(string ProgrammeId)
        {
            User user = null;            
            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));
                EditClientsViewModel model = new EditClientsViewModel(programme);
                return View(model);                
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddClientUser(IFormCollection formCollection)
        {
            User Currentuser = null;
            Organisation ownerorg = null;
            Organisation primaryorg = null;
            string email = null;
            OrganisationViewModel orgmodel = new OrganisationViewModel();
            User userdb = null;
            User user2 = null;
            var jsdkf = formCollection["Id"];
            string username = "";
            try
            {
                Currentuser = await CurrentUser();
                ownerorg = await _organisationService.GetOrganisation(Guid.Parse(formCollection["Id"]));
                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {

                    var Action = formCollection["Action"];
                    var FirstName = formCollection["FirstName"];
                    var LastName = formCollection["LastName"];
                    var Email = formCollection["Email"];
                    var Phone = formCollection["Phone"];

                    if (Action == "Edit")
                    {
                        userdb = await _userService.GetUserById(Guid.Parse(formCollection["UserId"]));
                        if (userdb == null)
                        {
                            username = FirstName + "_" + LastName;

                            try
                            {
                                user2 = await _userService.GetUser(username);

                                if (user2 != null && userdb == user2)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    username = username + randomNumber.ToString();
                                }
                            }
                            catch (Exception)
                            {
                                username = FirstName + "_" + LastName;
                            }
                        }

                        primaryorg = await _organisationService.GetOrganisation(userdb.PrimaryOrganisation.Id);
                        primaryorg.Email = Email;
                        userdb.FirstName = FirstName;
                        userdb.LastName = LastName;
                        userdb.FullName = FirstName + " " + LastName;
                        userdb.Email = Email;
                        userdb.Phone = Phone;
                        await _userService.Update(userdb);
                        await uow.Commit();

                    }
                    else
                    {
                        if (Action == "Add")
                        {
                            userdb = new User(Currentuser, Guid.NewGuid(), username);
                            userdb.FirstName = FirstName;
                            userdb.LastName = LastName;
                            userdb.FullName = FirstName + " " + LastName;
                            userdb.Email = Email;
                            userdb.Phone = Phone;
                            await _userService.Create(userdb);
                            userdb.Organisations.Add(ownerorg);
                            userdb.SetPrimaryOrganisation(ownerorg);
                            await uow.Commit();
                        }
                    }

                }

                return await RedirectToLocal();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, Currentuser, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> EditClient(IFormCollection formCollection)
        {
            User user = null;
            Organisation organisation = null;
            string email = null;

            try
            {
                foreach (var key in formCollection.Keys)
                {
                    organisation = await _organisationService.GetOrganisation(Guid.Parse(formCollection["Id"]));
                    var userList = await _userService.GetAllUserByOrganisation(organisation);
                    user = userList.Last(user => user.PrimaryOrganisation == organisation);
                    using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                    {
                        organisation.ChangeOrganisationName(formCollection["OrganisationName"]);
                        organisation.Email = formCollection["Email"];
                        organisation.Phone = formCollection["Phone"];
                        if (user != null)
                            user.Email = formCollection["Email"];
                        await uow.Commit();
                    }
                }

                return await RedirectToLocal();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> IssueReminder(IFormCollection formCollection)
        {
            User user = null;
            Programme programme = null;
            string email = null;

            try
            {
                user = await CurrentUser();
                programme = await _programmeService.GetProgramme(Guid.Parse(formCollection["ProgrammeId"]));
                foreach (var key in formCollection.Keys)
                {
                    email = key;
                    var correctEmail = await _userService.GetUserByEmail(email);
                    if (correctEmail != null)
                    {
                        if (programme.ProgEnableEmail)
                        {
                            var clientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse(formCollection[key]));
                            clientProgramme.ReminderDate = DateTime.Now;
                            await _programmeService.Update(clientProgramme);

                            //send out login instruction email
                            await _emailService.SendSystemEmailLogin(email);
                            //send out information sheet instruction email
                            EmailTemplate emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == "SendInformationSheetReminder");
                            if (emailTemplate != null)
                            {
                                await _emailService.SendEmailViaEmailTemplate(email, emailTemplate, null, null, null);
                            }
                        }
                    }

                }

                return await RedirectToLocal();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBreakdown(string ProgrammeId)
        {
            Guid.TryParse(ProgrammeId, out Guid Id);
            Programme programme = await _programmeService.GetProgrammeById(Id);
            BreakdownModel model = new BreakdownModel(programme);
            return View(model);
        }
    }
}