using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.Programme;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using DealEngine.Infrastructure.FluentNHibernate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic;
using DocumentFormat.OpenXml.Bibliography;
using DealEngine.WebUI.Models.Information;
using NHibernate.Linq;
using SystemDocument = DealEngine.Domain.Entities.Document;
using Document = DealEngine.Domain.Entities.Document;
using System.Net.Mime;
using NReco.PdfGenerator;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class InformationController : BaseController
    {
        IProgrammeService _programmeService;
        IEmailTemplateService _emailTemplateService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<InformationController> _logger;
        ISharedDataRoleService _sharedDataRoleService;
        IAppSettingService _appSettingService;
        IActivityService _activityService;
        IInformationItemService _informationItemService;
        IInformationSectionService _informationSectionService;
        IInformationTemplateService _informationTemplateService;
        ITerritoryService _territoryService;
        IFileService _fileService;
        IClientInformationService _clientInformationService;
        IUnlockProcessService _unlockProcessService;
        IClientAgreementService _clientAgreementService;
        IClientAgreementTermService _clientAgreementTermService;
        IClientAgreementMVTermService _clientAgreementMVTermService;
        IClientAgreementRuleService _clientAgreementRuleService;
        IUWMService _uWMService;
        ITaskingService _taskingService;
        IEmailService _emailService;
        IUnitOfWork _unitOfWork;
        IReferenceService _referenceService;
        IMilestoneService _milestoneService;
        IAdvisoryService _advisoryService;
        IOrganisationService _organisationService;
        IInsuranceAttributeService _insuranceAttributeService;
        IBusinessActivityService _businessActivityService;
        IProductService _productService;
        IMapper _mapper;
        IMapperSession<DropdownListItem> _IDropdownListItem;
        IClientInformationAnswerService _clientInformationAnswer;
        IOrganisationTypeService _organisationTypeService;
        IMapperSession<ClientProgramme> _clientProgrammeRepository;
        IMapperSession<WaterLocation> _waterLocation;
        ISubsystemService _subsystemService;
        IChangeProcessService _changeProcessService;
        IMapperSession<OrganisationalUnit> _organisationalUnitRepository;
        IMapperSession<Organisation> _organisationRepository;
        IClientAgreementExtensionTermService _clientAgreementExtensionTermService;

        public InformationController(
            ISubsystemService subsystemService,
            IOrganisationTypeService organisationTypeService,
            IEmailTemplateService emailTemplateService,
            IApplicationLoggingService applicationLoggingService,
            ILogger<InformationController> logger,
            IInformationSectionService informationSectionService,
            IInsuranceAttributeService insuranceAttributeService,
            IOrganisationService organisationService,
            IActivityService activityService,
            IAppSettingService appSettingService,
            IAdvisoryService advisoryService,
            IUserService userService,
            ITerritoryService territoryService,
            IInformationItemService informationItemService,
            IUnlockProcessService unlockProcessService,
            IFileService fileService,
            IEmailService emailService,
            IMilestoneService milestoneService,
            IInformationTemplateService informationTemplateService,
            IClientInformationService clientInformationService,
            IClientAgreementService clientAgreementService,
            IClientAgreementTermService clientAgreementTermService,
            IClientAgreementMVTermService clientAgreementMVTermService,
            IClientAgreementRuleService clientAgreementRuleService,
            IUWMService uWMService,
            IReferenceService referenceService,
            IProductService productService,
            ITaskingService taskingService,
            ISharedDataRoleService sharedDataRoleService,
            IUnitOfWork unitOfWork,
            IProgrammeService programmeService,
            IBusinessActivityService businessActivityService,
            IClientInformationAnswerService clientInformationAnswer,
            IMapperSession<DropdownListItem> dropdownListItem,
            IMapperSession<ClientProgramme> clientProgrammeRepository,
            IMapperSession<WaterLocation> waterLocation,
            IChangeProcessService changeProcessService,
            IMapperSession<OrganisationalUnit> organisationalUnitRepository,
            IMapperSession<Organisation> organisationRepository,
            //IGeneratePdf generatePdf,
            IClientAgreementExtensionTermService clientAgreementExtensionTermService,
        IMapper mapper
            )
            : base(userService)
        {
            _subsystemService = subsystemService;
            _waterLocation = waterLocation;
            _organisationTypeService = organisationTypeService;
            _emailTemplateService = emailTemplateService;
            _applicationLoggingService = applicationLoggingService;
            _logger = logger;
            _insuranceAttributeService = insuranceAttributeService;
            _organisationService = organisationService;
            _appSettingService = appSettingService;
            _sharedDataRoleService = sharedDataRoleService;
            _territoryService = territoryService;
            _advisoryService = advisoryService;
            _activityService = activityService;
            _userService = userService;
            _unlockProcessService = unlockProcessService;
            _productService = productService;
            _informationItemService = informationItemService;
            _informationSectionService = informationSectionService;
            _clientInformationAnswer = clientInformationAnswer;
            _informationTemplateService = informationTemplateService;
            _clientAgreementService = clientAgreementService;
            _clientAgreementTermService = clientAgreementTermService;
            _clientAgreementMVTermService = clientAgreementMVTermService;
            _clientAgreementRuleService = clientAgreementRuleService;
            _clientInformationService = clientInformationService;
            _referenceService = referenceService;
            _milestoneService = milestoneService;
            _uWMService = uWMService;
            _taskingService = taskingService;
            _fileService = fileService;
            _businessActivityService = businessActivityService;
            _IDropdownListItem = dropdownListItem;
            _programmeService = programmeService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _clientProgrammeRepository = clientProgrammeRepository;
            _changeProcessService = changeProcessService;
            _organisationalUnitRepository = organisationalUnitRepository;
            _organisationRepository = organisationRepository;
            _clientAgreementExtensionTermService = clientAgreementExtensionTermService;
            //_generatePdf = generatePdf;
        }

        [HttpGet]
        public async Task<IActionResult> GetProgrammeSections(Guid informationTemplateID)
        {
            InformationTemplate template = await _informationTemplateService.GetTemplate(informationTemplateID);
            Information model = new Information();
            var Litems = new List<InformationItems>();
            User user = null;
            try
            {
                user = await CurrentUser();
                foreach (var item in template.Sections)
                {
                    Litems.Add(new InformationItems() { Id = item.Id, Name = item.Name });

                }
                model.informationitem = Litems;
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        // GET: Information
        [HttpGet]
        public async Task<IActionResult> ViewInformation(Guid id)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                InformationViewModel model = await GetClientInformationSheetViewModel(id);

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPanel(Guid panelId, string panelName, int panelPosition)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {

                    if (panelName != null)
                    {

                        InformationSection section = await _informationSectionService.GetSection(panelId);
                        section.Position = panelPosition;
                        // TODO: Add these items at templates so it can be clonned properly 
                        await uow.Commit();
                    }

                }
                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ViewProgrammeDetails(Guid Id)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(Id);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                InformationViewModel model = await GetInformationViewModel(clientProgramme);
                model.ClientInformationSheet = sheet;

                model.ClientProgramme = clientProgramme;
                List<string> sections = new List<string>();
                foreach (var Section in model.Section.OrderBy(s => s.Position))
                {
                    sections.Add(Section.CustomView);
                }
                model.ListSection = sections;
                
                ViewBag.Title = "View Information Sheet ";
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

     

        [HttpGet]
        public async Task<IActionResult> PartialViewProgramme(Guid id, String name = "", List<string> viewlist = null)
        {

            User user = null;

            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(id);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                InformationViewModel model = await GetInformationViewModel(clientProgramme);
                model.ClientInformationSheet = sheet;

                model.SectionView = name;
                model.ListSection = viewlist;
                model.ClientProgramme = clientProgramme;
                user = await CurrentUser();

                //build custom models
                await GetRevenueViewModel(model, sheet.RevenueData);
                await GetRoleViewModel(model, sheet.RoleData);

                //build models from answers
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PMINZEPLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("ELViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("EPLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("CLIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("SLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PMINZPIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("DAOLIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("GLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("ClaimsHistoryViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("FAPViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("IPViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("OTViewModel", StringComparison.CurrentCulture)));

                model.AnswerSheetId = sheet.Id;
                model.ClientInformationSheet = sheet;
                model.ClientProgramme = clientProgramme;

                //testing dynamic wizard here
                var isSubsystem = await _programmeService.IsBaseClass(clientProgramme);
                if (isSubsystem)
                {
                    model.Wizardsteps = LoadWizardsteps("Subsystem");
                }
                else
                {
                    model.Wizardsteps = LoadWizardsteps("Standard");
                }


                string advisoryDesc = "";
                if (sheet.Status == "Not Started")
                {
                    #region Commented code
                    //var milestone = await _milestoneService.GetMilestoneByBaseProgramme(clientProgramme.BaseProgramme.Id);
                    //if (milestone != null)
                    //{
                    //    var advisoryList = await _advisoryService.GetAdvisorysByMilestone(milestone);
                    //    var advisory = advisoryList.LastOrDefault(a => a.Activity.Name == "Agreement Status - Not Started" && a.DateDeleted == null);
                    //    if (advisory != null)
                    //    {
                    //        advisoryDesc = advisory.Description;
                    //    }
                    //}
                    #endregion
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        sheet.Status = "Started";
                        await uow.Commit();
                    }
                }
                var claims = new List<ClaimViewModel>();
                for (var i = 0; i < sheet.ClaimNotifications.Count; i++)
                {
                    claims.Add(ClaimViewModel.FromEntity(sheet.ClaimNotifications.ElementAtOrDefault(i)));
                }
                model.Claims = claims;
                var interestedParties = new List<OrganisationViewModel>();
                var insuranceAttributeList = await _insuranceAttributeService.GetInsuranceAttributes();
                foreach (InsuranceAttribute IA in insuranceAttributeList.Where(ia => ia.InsuranceAttributeName == "Financial" || ia.InsuranceAttributeName == "Private" || ia.InsuranceAttributeName == "CoOwner"))
                {
                    #region Commented code
                    //foreach (var org in IA.IAOrganisations)
                    //{
                    //    if (org.OrganisationType.Name == "Person - Individual" || org.OrganisationType.Name == "Corporation – Limited liability" || org.OrganisationType.Name == "Corporation – Unlimited liability" || org.OrganisationType.Name == "Corporation – Public-Listed" ||
                    //        org.OrganisationType.Name == "Corporation – Public Unlisted" || org.OrganisationType.Name == "Corporation – Overseas" || org.OrganisationType.Name == "Incorporated Society")
                    //    {
                    //        OrganisationViewModel ovm = _mapper.Map<OrganisationViewModel>(org);
                    //        ovm.Organisation.Name = org.Name;
                    //        interestedParties.Add(ovm);
                    //    }
                    //}
                    #endregion
                }
                
                #region Commented code

                //model.InterestedParties = interestedParties;

                //List<SelectListItem> linterestedparty = new List<SelectListItem>();

                //for (var i = 0; i < model.InterestedParties.Count(); i++)
                //{
                //    linterestedparty.Add(new SelectListItem
                //    {
                //        Selected = false,
                //        Text = model.InterestedParties.ElementAtOrDefault(i).Organisation.Name,
                //        Value = model.InterestedParties.ElementAtOrDefault(i).ID.ToString(),
                //    });
                //}

                //model.InterestedPartyList = linterestedparty;

                //var boatUses = new List<BoatUseViewModel>();
                //for (var i = 0; i < sheet.BoatUses.Count(); i++)
                //{
                //    boatUses.Add(BoatUseViewModel.FromEntity(sheet.BoatUses.ElementAtOrDefault(i)));

                //}

                //List<SelectListItem> list = new List<SelectListItem>();

                //for (var i = 0; i < boatUses.Count(); i++)
                //{
                //    var text = boatUses.ElementAtOrDefault(i).BoatUseCategory.Substring(0, 4);
                //    var val = boatUses.ElementAtOrDefault(i).BoatUseId.ToString();

                //    list.Add(new SelectListItem
                //    {
                //        Selected = false,
                //        Value = val,
                //        Text = text
                //    });

                //}

                //model.BoatUseslist = list;
                // TODO - find a better way to pass these in
                //model.HasVehicles = sheet.Vehicles.Count > 0;
                //var vehicles = new List<VehicleViewModel>();
                //foreach (Vehicle v in sheet.Vehicles.Where(v => v.Removed == false))
                //{
                //    vehicles.Add(VehicleViewModel.FromEntity(v));
                //}
                //model.AllVehicles = vehicles;
                //model.RegisteredVehicles = vehicles.Where(v => !string.IsNullOrWhiteSpace(v.Registration));
                //model.UnregisteredVehicles = vehicles.Where(v => string.IsNullOrWhiteSpace(v.Registration));


                //var locations = new List<LocationViewModel>();
                //var buildings = new List<BuildingViewModel>();
                //var waterLocations = new List<WaterLocationViewModel>();
                //var MarinaLocations = new List<OrganisationViewModel>();
                //var organisationalunit = new List<OrganisationalUnit>();

                //for (var i = 0; i < sheet.Buildings.Count(); i++)
                //{
                //    buildings.Add(BuildingViewModel.FromEntity(sheet.Buildings.ElementAtOrDefault(i)));

                //}
                #endregion
                var insuranceAttributeList1 = await _insuranceAttributeService.GetInsuranceAttributes();
                foreach (InsuranceAttribute IA in insuranceAttributeList1.Where(ia => ia.InsuranceAttributeName == "Marina" || ia.InsuranceAttributeName == "Other Marina"))
                {
                    #region Commented code
                    //foreach (var org in IA.IAOrganisations)
                    //{
                    //    if (org.OrganisationType.Name == "Corporation – Limited liability" || org.OrganisationType.Name == "Corporation – Unlimited liability" || org.OrganisationType.Name == "Corporation – Public-Listed" ||
                    //    org.OrganisationType.Name == "Corporation – Public Unlisted" || org.OrganisationType.Name == "Corporation – Overseas" || org.OrganisationType.Name == "Incorporated Society")
                    //    {
                    //        OrganisationViewModel ovm = _mapper.Map<OrganisationViewModel>(org);
                    //        ovm.Organisation.Name = org.Name;
                    //        MarinaLocations.Add(ovm);
                    //    }
                    //}
                    #endregion
                }
                #region Commented code
                //model.MarinaLocations = MarinaLocations;

                //for (var i = 0; i < sheet.WaterLocations.Count(); i++)
                //{
                //    waterLocations.Add(WaterLocationViewModel.FromEntity(sheet.WaterLocations.ElementAtOrDefault(i)));
                //}
                #endregion

                var availableProducts = new List<SelectListItem>();

                foreach (Product product in clientProgramme.BaseProgramme.Products)
                {
                    availableProducts.Add(new SelectListItem
                    {
                        Selected = false,
                        Value = "" + product.Id,
                        Text = product.Name
                    });
                }
                var AgreementCount = clientProgramme.Agreements.Where(cp => cp.DateDeleted == null).Count();
                String[][] OptionItems = new String[AgreementCount][];
                String[] OptionItem;
                var count = 0;
                ///not deleted agreement deleteddate = null 
                foreach (var agreement in clientProgramme.Agreements.Where(cp => cp.DateDeleted == null))
                {
                    foreach (var term in agreement.ClientAgreementTerms)
                    {
                        OptionItem = new String[3];
                        if (term.Bound)
                        {
                            OptionItem[0] = agreement.Product.Name;
                            OptionItem[1] = ""+ term.TermLimit;
                            OptionItem[2] = "" + term.Excess;
                            OptionItems[count] = OptionItem;
                            count++;
                        }
                    }
                }
                model.LimitsSelected = OptionItems;
               // model.DeclarationMessage = sheet.Programme.BaseProgramme.Declaration;

                model.Advisory = await _milestoneService.SetMilestoneFor("Agreement Status - Not Started", user, sheet);

                model.Status = sheet.Status;
                List<ClientInformationAnswer> informationAnswers = await _clientInformationAnswer.GetAllClaimHistory();
                informationAnswers.Where(c => c.ClientInformationSheet.Id == sheet.Id);

                ViewBag.Title = " View Information Sheet ";
                if (model.SectionView == "")
                {
                    return View("ProgrammeDetailsReport", model);
                }
                else
                {
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> GetdefaulProductSelect(string[] Answers, Guid ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var option in Answers)
                    {
                        if (option != "None")
                        {
                            var clientAgreementTerm = await _clientAgreementTermService.GetAllClientAgreementTerm();
                            List<ClientAgreementTerm> listClientAgreementerm = clientAgreementTerm.Where(cagt => cagt.Id == Guid.Parse(option)).ToList();
                            foreach (var term in listClientAgreementerm)
                            {
                                term.Bound = true;
                                await uow.Commit();
                            }
                        }
                    }
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveExtensionCoverOptions(string[] Answers, Guid ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var agreement in clientProgramme.Agreements)
                    {
                            if (agreement.Product.IsExtentionOption)

                            {
                                foreach (var term in agreement.ClientAgreementTermExtensions)
                            {
                                term.Bound = false;
                            }
                        }


                      if (clientProgramme.BaseProgramme.NamedPartyUnitName == "NZPI Programme")
                      {
                        ClientAgreementEndorsement cAELPLAIncl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Inclusion");
                        ClientAgreementEndorsement cAELPLAExcl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Exclusion");

                        if (cAELPLAIncl != null)
                        {
                            cAELPLAIncl.DateDeleted = DateTime.UtcNow;
                            cAELPLAIncl.DeletedBy = user;
                        }
                        if (cAELPLAExcl != null)
                        {
                            cAELPLAExcl.DateDeleted = null;
                            cAELPLAExcl.DeletedBy = null;
                        }
                      }
                    }
                    await uow.Commit();
                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var option in Answers)
                    {
                        if (option != "None" && option != null)
                        {
                            ClientAgreementTermExtension clientAgreementExtentionTerm = await _clientAgreementExtensionTermService.GetAgreementExtentionById(Guid.Parse(option));
                            
                                clientAgreementExtentionTerm.Bound = true;
                              
                            ClientAgreement agreement = clientAgreementExtentionTerm.ClientAgreement;
                            if (clientProgramme.BaseProgramme.NamedPartyUnitName == "NZPI Programme")
                            {
                                ClientAgreementEndorsement cAELPLAIncl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Inclusion");
                                ClientAgreementEndorsement cAELPLAExcl = agreement.ClientAgreementEndorsements.FirstOrDefault(cae => cae.Name == "Landscape Planning & Landscape Architectural Exclusion");
                                if (cAELPLAIncl != null)
                                {
                                    cAELPLAIncl.DateDeleted = null;
                                    cAELPLAIncl.DeletedBy = null;
                                }
                                if (cAELPLAExcl != null)
                                {
                                    cAELPLAExcl.DateDeleted = DateTime.UtcNow;
                                    cAELPLAExcl.DeletedBy = user;
                                }
                            }
                            

                        }
                    }

                    await uow.Commit();
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> SaveCoverOptions(string[] Answers, Guid ProgrammeId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var agreement in clientProgramme.Agreements)
                    {
                        if (agreement.Product.IsMultipleOption)
                        {
                            foreach (var term in agreement.ClientAgreementTerms)
                            {
                                term.Bound = false;
                                await uow.Commit();
                            }
                        }
                    }
                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var option in Answers)
                    {
                        if (option != "None")
                        {
                            var clientAgreementTerm = await _clientAgreementTermService.GetAllClientAgreementTerm();
                            List<ClientAgreementTerm> listClientAgreementerm = clientAgreementTerm.Where(cagt => cagt.Id == Guid.Parse(option)).ToList();
                            foreach (var term in listClientAgreementerm)
                            {
                                term.Bound = true;
                                await uow.Commit();
                            }
                        }
                    }
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetProductName(Guid id)
        {
            List<string> productname = new List<string>();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(id);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;

                foreach (ClientAgreement agreement in clientProgramme.Agreements.Where(a => a.Product.IsMultipleOption == true && a.DateDeleted == null))
                {
                    productname.Add(agreement.Product.Name);
                }

                return Json(productname);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetdefaultCoverOptions(Guid ProgrammeId)
        {
            List<ClientAgreementTerm> listClientAgreementerm = new List<ClientAgreementTerm>();
            List<Guid> listClientAgreementermid = new List<Guid>();
            String[] OptionItem;
            User user = null;
            Boolean boundval = false;

            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                String[][] OptionItems = new String[clientProgramme.Agreements.Where(ag => ag.DateDeleted == null).Count()][];
                var count = 0;
                foreach (var agreement in clientProgramme.Agreements.Where(ag => ag.DateDeleted == null))
                {
                    //count = 0;
                    boundval = false;
                    foreach (var selectterm in agreement.ClientAgreementTerms.Where(agt => agt.DateDeleted == null))
                    {

                        if (selectterm.Bound)
                        {
                            OptionItem = new String[2];

                            OptionItem[0] = agreement.Product.Name;
                            OptionItem[1] = "" + selectterm.Id;
                            OptionItems[count] = OptionItem;
                            count++;
                            boundval = true;
                            continue;
                        }

                    }
                    if (!boundval)
                    {
                        var term = agreement.ClientAgreementTerms.FirstOrDefault(o => o.Bound = true);
                        if (term == null)
                            term = agreement.ClientAgreementTerms.OrderBy(o => o.TermLimit).ThenBy(o => o.Excess).FirstOrDefault();

                        OptionItem = new String[2];

                        OptionItem[0] = agreement.Product.Name;
                        OptionItem[1] = "" + term.Id;
                        OptionItems[count] = OptionItem;
                        count++;
                        boundval = true;
                    }

                }
                return Json(OptionItems);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSelectedCoverOptionPremium(string SelectedId)
        {
            List<ClientAgreementTerm> listClientAgreementerm = new List<ClientAgreementTerm>();
            List<Guid> listClientAgreementermid = new List<Guid>();
            var count = 0;

            String[] OptionItem;
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreementTerm selectedterm = await _clientAgreementTermService.GetAgreementById(SelectedId);
               // var prem = selectedterm.Premium;
                    //ClientAgreementTerms.Where(at => at.DateDeleted == null);
                    //String[][] OptionItems = new String[clientProgramme.Agreements.Count][];

                return Json(selectedterm);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        public async Task<IActionResult> GetExtensionCoverOptions(Guid ProgrammeId)
        {
            List<ClientAgreementTermExtension> listClientAgreementermExtension = new List<ClientAgreementTermExtension>();
            List<Guid> listClientAgreementermExtensionid = new List<Guid>();
            var count = 0;
            String[] OptionItem;
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                String[][] OptionItems = new String[clientProgramme.Agreements.Count][];
                var chosenoption = 0;
                foreach (var agreement in clientProgramme.Agreements.Where(a => a.DateDeleted == null))
                {
                    chosenoption = 0;
                    foreach (var term in agreement.ClientAgreementTermExtensions.Where(at => at.DateDeleted == null))
                    {

                        OptionItem = new String[2];
                        if (term.Bound)
                        {
                            OptionItem[0] = agreement.Product.Name;
                            //OptionItem[0] = agreement.Product.Name + " Extensiontable";
                            OptionItem[1] = "" + term.Id;
                            OptionItems[count] = OptionItem;
                            count++;
                            chosenoption++;
                        }

                    }
                    if (chosenoption == 0)
                    {
                        OptionItem = new String[2];
                        OptionItem[0] = agreement.Product.Name;
                        OptionItem[1] = "None";
                        OptionItems[count] = OptionItem;
                        count++;
                    }
                }
                return Json(OptionItems);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetCoverOptions(Guid ProgrammeId)
        {
            List<ClientAgreementTerm> listClientAgreementerm = new List<ClientAgreementTerm>();
            List<Guid> listClientAgreementermid = new List<Guid>();
            var count = 0;
            String[] OptionItem;
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(ProgrammeId);

                String[][] OptionItems = new String[clientProgramme.Agreements.Count][];
                var chosenoption = 0;
                foreach (var agreement in clientProgramme.Agreements.Where(a => a.DateDeleted == null))
                {
                    chosenoption = 0;
                    foreach (var term in agreement.ClientAgreementTerms.Where(at => at.DateDeleted == null))
                    {

                        OptionItem = new String[2];
                        if (term.Bound)
                        {
                            OptionItem[0] = agreement.Product.Name;
                            OptionItem[1] = "" + term.Id;
                            OptionItems[count] = OptionItem;
                            count++;
                            chosenoption++;
                        }

                    }
                    if (chosenoption == 0)
                    {
                        OptionItem = new String[2];
                        OptionItem[0] = agreement.Product.Name;
                        OptionItem[1] = "None";
                        OptionItems[count] = OptionItem;
                        count++;
                    }
                }
                return Json(OptionItems);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetClaimHistory(Guid ClientInformationSheet)
        {
            String[][] ClaimAnswers = new String[5][];
            var count = 0;
            String[] ClaimItem;
            User user = null;

            try
            {
                user = await CurrentUser();
                foreach (var answer in _clientInformationAnswer.GetAllClaimHistory().Result.Where(c => c.ClientInformationSheet.Id == ClientInformationSheet && (c.ItemName == "Claimexp1" || c.ItemName == "Claimexp2" || c.ItemName == "Claimexp3"
                                                                                                                                                         || c.ItemName == "Claimexp4" || c.ItemName == "Claimexp5")))
                {
                    ClaimItem = new String[3];

                    for (var i = 0; i < 1; i++)
                    {
                        ClaimItem[i] = answer.ItemName;
                        ClaimItem[i + 1] = answer.Value;
                        ClaimItem[i + 2] = answer.ClaimDetails;
                    }

                    ClaimAnswers[count] = ClaimItem;
                    count++;
                }

                return Json(ClaimAnswers);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        // public async Task<IActionResult> EditInformation(Guid id)
        public async Task<IActionResult> EditInformation(Guid id, string updateType)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                var clientProgramme = await _programmeService.GetClientProgramme(id);
                var sheet = clientProgramme.InformationSheet;
                InformationViewModel model = await GetInformationViewModel(clientProgramme);
                model.Advisory = await _milestoneService.SetMilestoneFor("Agreement Status - Not Started", user, sheet);
                //build custom models
                await GetRevenueViewModel(model, sheet.RevenueData);
                await GetRoleViewModel(model, sheet.RoleData);

                //build models from answers
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PMINZEPLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("ELViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("EPLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("CLIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PMINZPIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("PIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("DAOLIViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("GLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("ClaimsHistoryViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("SLViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("FAPViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("IPViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("OTViewModel", StringComparison.CurrentCulture)));
                await BuildModelFromAnswer(model, sheet.Answers.Where(s => s.ItemName.StartsWith("GeneralViewModel", StringComparison.CurrentCulture)));               

                if (sheet.Status == "Not Started")
                {
                    model.Status = "Not Started";
                    sheet.Status = "Started";
                    await _clientInformationService.UpdateInformation(sheet);
                }

                var claims = new List<ClaimViewModel>();
                for (var i = 0; i < sheet.ClaimNotifications.Count; i++)
                {
                    claims.Add(ClaimViewModel.FromEntity(sheet.ClaimNotifications.ElementAtOrDefault(i)));
                }               


                model.Claims = claims;

                //if(updateType == "")
                if(string.IsNullOrWhiteSpace(updateType))
                {
                    updateType = "common_you";
                }
               
                model.selectedUpdateType = new List<string>();

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (model.selectedUpdateType != null)
                    {
                        //const string v = "TC_amendLocation";
                        model.selectedUpdateType.Add(updateType);
                    }
                    await uow.Commit();
                }
                //model.UpdateTypesViewModel.ValueType = "TC_amendLocation";

                return View("InformationWizard", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        private async Task BuildModelFromAnswer(InformationViewModel model, IEnumerable<ClientInformationAnswer> Model)
        {
            //build models from answers
            foreach (var answer in Model)
            {
                try
                {
                    var modelName = "";
                    var split = answer.ItemName.Split('.').ToList();
                    if (split.FirstOrDefault() == "PMINZEPLViewModel")
                    {
                        modelName = "EPLViewModel";
                    }
                    else if (split.FirstOrDefault() == "PMINZPIViewModel")
                    {
                        modelName = "PIViewModel";
                    }
                    else
                    {
                        modelName = split.FirstOrDefault();
                    }
                    if (split.Count > 1)
                    {
                        var modeltype = typeof(InformationViewModel).GetProperty(modelName);
                        var reflectModel = modeltype.GetValue(model);

                        var property = reflectModel.GetType().GetProperty(split.LastOrDefault());
                        if (typeof(string) == property.PropertyType)
                        {
                            property.SetValue(reflectModel, answer.Value);
                        }
                        if (typeof(int) == property.PropertyType)
                        {
                            int.TryParse(answer.Value, out int intvalue);
                            property.SetValue(reflectModel, intvalue);
                        }
                        if (typeof(decimal) == property.PropertyType)
                        {
                            decimal.TryParse(answer.Value, out decimal decvalue);
                            property.SetValue(reflectModel, decvalue);
                        }
                        if (typeof(IList<SelectListItem>) == property.PropertyType)
                        {
                            var propertylist = (IList<SelectListItem>)property.GetValue(reflectModel);
                            var options = answer.Value.Split(',').ToList();
                            foreach (var option in options)
                            {
                                propertylist.FirstOrDefault(i => i.Value == option).Selected = true;
                            }
                            property.SetValue(reflectModel, propertylist);
                        }
                        if (typeof(DateTime) == property.PropertyType)
                        {
                            var date = DateTime.Parse(answer.Value);
                            property.SetValue(reflectModel, date);
                        }
                        if (typeof(DateTime?) == property.PropertyType)
                        {
                            var date = DateTime.Parse(answer.Value);                         
                            property.SetValue(reflectModel, date.ToString("dd/MM/yyyy"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                }
            }
        }

        private async Task GetRoleViewModel(InformationViewModel model, RoleData roleData)
        {
            try
            {
                if (roleData.DataRoles.Any())
                {
                    model.RoleDataViewModel = _mapper.Map<RoleDataViewModel>(roleData);
                    model.RoleDataViewModel.AdditionalRoleInformationViewModel = _mapper.Map<AdditionalRoleInformationViewModel>(roleData.AdditionalRoleInformation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task GetRevenueViewModel(InformationViewModel model, RevenueData revenueData)
        {
            try
            {
                if (revenueData.Activities.Any() || revenueData.Territories.Any())
                {
                    model.RevenueDataViewModel = _mapper.Map<RevenueDataViewModel>(revenueData);
                    model.RevenueDataViewModel.AdditionalActivityViewModel = _mapper.Map<AdditionalActivityViewModel>(revenueData.AdditionalActivityInformation);
                    model.RevenueDataViewModel.AdditionalActivityViewModel.SetOptions();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private IList<string> LoadWizardsteps(string wizardType)
        {
            IList<string> steps = new List<string>();
            //convert this to load from the DB?
            if (wizardType == "Standard")
            {
                steps.Add("Details");
                steps.Add("Declaration");
            }
            else if (wizardType == "Subsystem")
            {
                steps.Add("Details");
                steps.Add("Quote");
                steps.Add("Declaration");
                steps.Add("Payment");
                steps.Add("Documents");
            }
            return steps;
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(UnlockReason UnlockReason)
        {
            User user = null;
            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(UnlockReason.DealId);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                user = await CurrentUser();
                await _unlockProcessService.CreateUnlockReason(user, UnlockReason);

                if (sheet != null)
                {
                    await _clientInformationService.UnlockSheet(sheet, user);
                }

                var url = "/Information/EditInformation/" + UnlockReason.DealId;
                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ResetStatus(Guid id)
        {
            User user = null;
            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(id);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                user = await CurrentUser();
                if (sheet != null)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (sheet.Status == "Started")
                        {
                            sheet.Status = "Not Started";
                            sheet.LastModifiedOn = DateTime.UtcNow;
                            sheet.LastModifiedBy = user;
                        }
                        await uow.Commit();
                    }
                }

                var url = "/Home/ViewProgramme/" + clientProgramme.BaseProgramme.Id;
                return Redirect(url);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Resume(Guid id)
        {
            User user = null;
            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(id);
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                user = await CurrentUser();
                if (sheet != null)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (sheet.Status == "Not Taken Up")
                        {
                            sheet.Status = "Started";
                            //sheet.Answers.FirstOrDefault(i => i.ItemName == "ClientInformationSheet.Status").Value = "Started";
                            sheet.LastModifiedOn = DateTime.UtcNow;
                            sheet.LastModifiedBy = user;
                        }
                        await uow.Commit();

                    }
                }

                var url = "/Information/EditInformation/" + id;
                return Redirect(url);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveInformation(IFormCollection collection)
        {
            Guid sheetId = Guid.Empty;
            User user = null;
            try
            {
                user = await CurrentUser();
                sheetId = Guid.Parse(collection["AnswerSheetId"]);
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(sheetId);
                if (sheet == null)
                    return Json("Failure");

                if (sheet.Status != "Submitted" && sheet.Status != "Bound")
                {
                    await _clientInformationService.SaveAnswersFor(sheet, collection, user);
                }

                return Json("Success");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitInformation(IFormCollection collection)
        {
            ClientInformationSheet sheet = null;
            User user = null;

            try
            {
                user = await CurrentUser();
                sheet = await _clientInformationService.GetInformation(Guid.Parse(collection["AnswerSheetId"]));                
                var isBaseSheet = await _clientInformationService.IsBaseClass(sheet);
                if (isBaseSheet)
                {
                    var programme = sheet.Programme.BaseProgramme;
                    var reference = await _referenceService.GetLatestReferenceId();

                    if (sheet.Status != "Submitted" && sheet.Status != "Bound")
                    {
                        await _clientInformationService.SaveAnswersFor(sheet, collection, user);

                        if (sheet.Programme.BaseProgramme.ProgEnableRequireNoCover)
                        {
                            foreach (Product product in sheet.Programme.BaseProgramme.Products.OrderBy(t => t.OrderNumber))
                            {
                                if (product.IsMasterProduct)
                                {
                                    if (sheet.Answers.Where(sa => sa.ItemName == product.NoCoverRequiredAnswer).Any())
                                    {
                                        if (sheet.Answers.Where(sa => sa.ItemName == product.NoCoverRequiredAnswer).First().Value != "1")
                                        {
                                            using (var uow = _unitOfWork.BeginUnitOfWork())
                                            {
                                                sheet.Status = "Not Taken Up";
                                                await uow.Commit();
                                            }
                                        }
                                    }
                                }
                            }
                        }                        

                        await GenerateUWM(user, sheet, reference);
                    }

                    return Content("/Agreement/ViewAgreement/" + sheet.Programme.Id);
                }
                else
                {
                    await _clientInformationService.SaveAnswersFor(sheet, collection, user);
                    return Redirect("/Information/QuoteToAgree?id=" + sheet.Programme.Id);
                }

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        private async Task GenerateUWM(User user, ClientInformationSheet sheet, string reference)
        {
            using (var uow = _unitOfWork.BeginUnitOfWork())
            {
                //UWM
                _uWMService.UWM(user, sheet, reference);

                //sheet.Status = "Submitted";
                await uow.Commit();
            }

            foreach (ClientAgreement agreement in sheet.Programme.Agreements)
            {
                await _referenceService.CreateClientAgreementReference(agreement.ReferenceId, agreement.Id);
            }
        }


        [HttpGet]
        public async Task<IActionResult> QuoteToAgree(string id)
        {
            Guid sheetId = Guid.Empty;
            User user = null;

            try
            {
                user = await CurrentUser();
                var clientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse(id));                
                var sheet = clientProgramme.InformationSheet;
                var isBaseSheet = await _clientInformationService.IsBaseClass(sheet);

                if (sheet.Status != "Submitted" && sheet.Status != "Bound")
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        sheet.Status = "Submitted";
                        sheet.SubmitDate = DateTime.UtcNow;
                        sheet.SubmittedBy = user;
                        await uow.Commit();
                    }

                    if (!isBaseSheet)
                    {
                        SubClientInformationSheet subSheet = (SubClientInformationSheet)sheet;
                        sheet = subSheet.BaseClientInformationSheet;

                        if (sheet.SubClientInformationSheets.Where(c => c.Status != "Submitted").ToList().Count == 0)
                        {
                            await GenerateUWM(user, sheet, sheet.ReferenceId);
                            if (sheet.Programme.BaseProgramme.ProgEnableEmail)
                            {
                                await _emailService.SendSystemEmailAllSubUISComplete(sheet.Owner, sheet.Programme.BaseProgramme, sheet);
                            }
                            //sheet = baseSheet;
                        }
                    }

                    if (sheet.Programme.BaseProgramme.ProgEnableEmail)
                    {
                        //sheet owner is null
                    //    await _emailService.SendSystemEmailUISSubmissionConfirmationNotify(user, sheet.Programme.BaseProgramme, sheet, sheet.Owner);
                        //send out information sheet submission notification email
                    //    await _emailService.SendSystemEmailUISSubmissionNotify(user, sheet.Programme.BaseProgramme, sheet, sheet.Owner);
                        //send out agreement refer notification email
                        foreach (ClientAgreement agreement in clientProgramme.Agreements)
                        {
                            if (agreement.Status == "Referred")
                            {
                                await _milestoneService.SetMilestoneFor("Agreement Status – Referred", user, sheet);
                                await _emailService.SendSystemEmailAgreementReferNotify(user, sheet.Programme.BaseProgramme, agreement, sheet.Owner);
                            }
                        }
                    }
                }

                return Content("/Agreement/ViewAgreementDeclaration/" + id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> PaymentInformation(IFormCollection collection)
        {
            Guid sheetId = Guid.Empty;
            ClientInformationSheet sheet = null;
            User user = null;

            try
            {
                user = await CurrentUser();
                if (Guid.TryParse(HttpContext.Request.Form["AnswerSheetId"], out sheetId))
                {
                    sheet = await _clientInformationService.GetInformation(sheetId);
                }

                return Content("/Agreement/ViewPayment/" + sheet.Programme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInformation(IFormCollection formCollection)
        {
            User createdBy = null;
            if (formCollection != null)
            {
                var changeType = formCollection["ChangeType"];

                if (changeType == "TC_AdminUpdateMoveAdvisor")
                {
                    createdBy = await CurrentUser();
                    ChangeReason changeReason = new ChangeReason(createdBy, formCollection);
                    // Usually changeReason is saved by linking to the newProgramme and saving, but in this instance we aren't creating a newProgramme...
                    await _changeProcessService.Add(changeReason);

                    return Redirect("/Information/MoveAdvisors/" + Guid.Parse(formCollection["DealId"]));
                }            
            }
        
            try
            {
                createdBy = await CurrentUser();
                ClientProgramme CloneProgramme = await _programmeService.CloneForUpdate(createdBy, formCollection, null);

                var updateType = formCollection["ChangeType"];

                return (RedirectToAction("EditInformation", new { id = CloneProgramme.Id, updateType = updateType }));


                //return Redirect("/Information/EditInformation/" + CloneProgramme.Id );
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, createdBy, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<ViewResult> MoveAdvisors(Guid id)
        {
            ClientProgramme clientProgramme = await _clientProgrammeRepository.GetByIdAsync(id);
            Programme programme = clientProgramme.BaseProgramme;
            ClientInformationSheet lastInformationSheet = clientProgramme.InformationSheet;

            while (lastInformationSheet.NextInformationSheet != null)
            {
                lastInformationSheet = lastInformationSheet.NextInformationSheet;
            }

            IList<Organisation> organisations = lastInformationSheet.Organisation;
            IList<Organisation> advisors = new List<Organisation>();
            IList<ClientProgramme> allClientProgrammes = programme.ClientProgrammes.Where(o => o.InformationSheet.DateDeleted == null && o.InformationSheet.Status == "Bound").ToList();

            foreach (Organisation org in organisations)
            {
                var principleAdvisorUnit = (AdvisorUnit)org.OrganisationalUnits.FirstOrDefault(u => (u.Name == "Advisor") && (u.DateDeleted == null));
                if (principleAdvisorUnit != null)
                {
                    advisors.Add(org);
                }
            }

            MoveAdvisorsViewModel model = new MoveAdvisorsViewModel(id, clientProgramme.Owner.Name, advisors, allClientProgrammes);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MoveAdvisors(IFormCollection collection)
        {
            User user = await CurrentUser();
            IList<string> advisors = collection["MoveAdvisorsViewModel.AdvisorToMove"].ToList<string>();
            string newFAPKey = collection["MoveAdvisorsViewModel.NewFAP"];
            Guid.TryParse(newFAPKey, out Guid newIsTheFAPOrganisationId);
            string ownerandCPIds = collection["MoveAdvisorsViewModel.TargetOwner"];
            string[] ownerandCPIdsArray = ownerandCPIds.Split(' ');
            //Guid.TryParse(ownerandCPIdsArray[0], out Guid targetOwnerId); // not used
            Guid.TryParse(ownerandCPIdsArray[1], out Guid targetClientProgrammeId);
            string sourceClientProgrammeStr = collection["MoveAdvisorsViewModel.SourceClientProgrammeId"];
            string targetOwnerFAP = collection["MoveAdvisorsViewModel.TargetOwnerFAP"];
            Guid.TryParse(sourceClientProgrammeStr, out Guid sourceClientProgrammeId);
            ClientProgramme clientProgramme = await _clientProgrammeRepository.GetByIdAsync(targetClientProgrammeId);
            ClientProgramme sourceClientProgramme = await _clientProgrammeRepository.GetByIdAsync(sourceClientProgrammeId);
            Programme programme = sourceClientProgramme.BaseProgramme;
            EmailTemplate moveAdvisorsPrevOwner = programme.EmailTemplates.FirstOrDefault(et => et.Name == "Advice Of Removal Of An Advisor From Policy");
            EmailTemplate moveAdvisorsNewOwner = programme.EmailTemplates.FirstOrDefault(et => et.Name == "Advice Of Addition Of An Advisor From Policy");

            // Fix Extra isTheFAPs and set current isTheFAP (Use case 0 & 2)
            try
            {
                
                // Attach the Advisors
                await _programmeService.MoveAdvisorsToClientProgramme(advisors, clientProgramme, sourceClientProgramme, user);

                //Clone or create sub uis
                IList<SubClientProgramme> SubClientProgrammes;
                if (clientProgramme.InformationSheet.NextInformationSheet.IsChange)
                {
                    SubClientProgrammes = clientProgramme.SubClientProgrammes;
                    if (!SubClientProgrammes.Any())
                    {
                        SubClientProgrammes = clientProgramme.InformationSheet.NextInformationSheet.Programme.SubClientProgrammes;
                    }
                }
                else
                {
                    SubClientProgrammes = clientProgramme.InformationSheet.NextInformationSheet.Programme.SubClientProgrammes;
                }
                if (SubClientProgrammes.Any())
                {
                    await _subsystemService.ValidateProgramme(clientProgramme.InformationSheet.NextInformationSheet, user);
                }
                else
                {
                    await _subsystemService.CreateSubObjects(clientProgramme.InformationSheet.NextInformationSheet.Programme.Id, clientProgramme.InformationSheet.NextInformationSheet, user);
                }

                //render new clientagreement doc
                if (clientProgramme.InformationSheet.NextInformationSheet != null)
                {
                    clientProgramme.InformationSheet.NextInformationSheet.Status = "Bound";
                    await _clientInformationService.UpdateInformation(clientProgramme.InformationSheet.NextInformationSheet);

                    foreach (ClientAgreement newclientAgreement in clientProgramme.InformationSheet.NextInformationSheet.Programme.Agreements.Where(nca => nca.DateDeleted == null))
                    {
                        //Render Documents
                        var documents = new List<SystemDocument>();
                        var documentspremiumadvice = new List<SystemDocument>();
                        var agreeTemplateList = newclientAgreement.Product.Documents;
                        var agreeDocList = newclientAgreement.GetDocuments();


                        //tripleA DO use case, remove when all client set as company
                        if (newclientAgreement.Product.Id == new Guid("bdbdda02-ee4e-44f5-84a8-dd18d17287c1") &&
                            newclientAgreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDAOLIOptions").First().Value == "2")
                        {

                        }
                        else
                        {

                            if (!newclientAgreement.Product.IsOptionalCombinedProduct)
                            {
                                foreach (SystemDocument template in agreeTemplateList)
                                {
                                    if (template.ContentType == MediaTypeNames.Application.Pdf)
                                    {
                                        SystemDocument notRenderedDoc = await _fileService.GetDocumentByID(template.Id);
                                        newclientAgreement.Documents.Add(notRenderedDoc);
                                        documents.Add(notRenderedDoc);
                                    }
                                    else
                                    {
                                        //render docs except invoice
                                        if (template.DocumentType != 4 && template.DocumentType != 6)
                                        {
                                            if (template.Name == "TripleA Individual TL Certificate")
                                            {
                                                if (newclientAgreement.Product.IsOptionalProductBasedSub &&
                                                    newclientAgreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == newclientAgreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                                                {
                                                    SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, newclientAgreement, null, null);
                                                    renderedDoc.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                    newclientAgreement.Documents.Add(renderedDoc);
                                                    documents.Add(renderedDoc);
                                                    await _fileService.UploadFile(renderedDoc);
                                                }
                                            }
                                            else if (template.DocumentType == 7)
                                            {
                                                SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, newclientAgreement, null, null);
                                                renderedDoc.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                newclientAgreement.Documents.Add(renderedDoc);
                                                //documents.Add(renderedDoc);
                                                documentspremiumadvice.Add(renderedDoc);
                                                await _fileService.UploadFile(renderedDoc);
                                            }
                                            else if (template.DocumentType == 8)
                                            {
                                                SystemDocument renderedDoc1 = await _fileService.RenderDocument(user, template, newclientAgreement, null, null);

                                                SystemDocument renderedDoc = await GetInvoicePDF(renderedDoc1, template.Name);
                                                renderedDoc.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                newclientAgreement.Documents.Add(renderedDoc1);
                                                documents.Add(renderedDoc);
                                                await _fileService.UploadFile(renderedDoc);
                                            }
                                            else
                                            {
                                                SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, newclientAgreement, null, null);
                                                renderedDoc.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                newclientAgreement.Documents.Add(renderedDoc);
                                                documents.Add(renderedDoc);
                                                await _fileService.UploadFile(renderedDoc);
                                            }

                                        }

                                        //render all subsystem
                                        if (template.DocumentType == 6)
                                        {
                                            foreach (var subSystemClient in clientProgramme.InformationSheet.NextInformationSheet.SubClientInformationSheets)
                                            {
                                                if (newclientAgreement.Product.IsOptionalProductBasedSub)
                                                {
                                                    if (subSystemClient.Answers.Where(sa => sa.ItemName == newclientAgreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                                                    {
                                                        SystemDocument renderedDocSub = await _fileService.RenderDocument(user, template, newclientAgreement, subSystemClient, null);
                                                        renderedDocSub.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                        newclientAgreement.Documents.Add(renderedDocSub);
                                                        documents.Add(renderedDocSub);
                                                        await _fileService.UploadFile(renderedDocSub);
                                                    }
                                                }
                                                else
                                                {
                                                    SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, newclientAgreement, subSystemClient, null);
                                                    renderedDoc.OwnerOrganisation = newclientAgreement.ClientInformationSheet.Owner;
                                                    newclientAgreement.Documents.Add(renderedDoc);
                                                    documents.Add(renderedDoc);
                                                    await _fileService.UploadFile(renderedDoc);
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

            // Send the Emails
            try
            {
                if (clientProgramme.InformationSheet.NextInformationSheet.Programme.BaseProgramme.ProgEnableEmail)
                {
                    if (moveAdvisorsPrevOwner != null && moveAdvisorsNewOwner != null)
                    {
                        foreach (string advisorId in advisors)
                        {
                            Guid.TryParse(advisorId, out Guid AdvisorOrganisationId);

                            if (AdvisorOrganisationId != Guid.Empty)
                            {
                                var Organisation = await _organisationRepository.GetByIdAsync(AdvisorOrganisationId);
                                if (Organisation != null)
                                {
                                    var moveAdvisorsAdvisorRecipient = Organisation.Email;
                                    await _emailService.SendEmailViaEmailTemplate(moveAdvisorsAdvisorRecipient, moveAdvisorsNewOwner, null, null, null);
                                }
                            }
                        }

                        var moveAdvisorsPrevOwnerRecipient = sourceClientProgramme.Owner.Email;
                        await _emailService.SendEmailViaEmailTemplate(moveAdvisorsPrevOwnerRecipient, moveAdvisorsPrevOwner, null, null, null);

                        var moveAdvisorsNewOwnerRecipient = clientProgramme.Owner.Email;
                        await _emailService.SendEmailViaEmailTemplate(moveAdvisorsNewOwnerRecipient, moveAdvisorsNewOwner, null, null, null);
                    }
                }
                 
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<Document> GetInvoicePDF(SystemDocument renderedDoc, string invoicename)
        {
            User user = null;

            //SystemDocument doc = await _documentRepository.GetByIdAsync(Id);


            var docContents = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            // DOCX & HTML
            string html = _fileService.FromBytes(renderedDoc.Contents);

            if (renderedDoc.DocumentType == 8) // Apollo Invoice
            {
                html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /></head>"); // Removed to fix Image 
            }
            else
            {
                html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /><style>img { height:auto; max-width: 300px }</style></head>"); // Ashu old values -> width: 120px; height:120px
            }

            var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
            htmlToPdfConv.License.SetLicenseKey(
               _appSettingService.NRecoUserName,
               _appSettingService.NRecoLicense
           );            // for Linux/OS-X: "wkhtmltopdf"
            if (_appSettingService.IsLinuxEnv == "True")
            {
                htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
            }
            htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;
            var margins = new PageMargins();
            margins.Bottom = 10;
            margins.Top = 10;
            margins.Left = 30;
            margins.Right = 10;
            htmlToPdfConv.Margins = margins;

            htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";
            var pdfBytes = htmlToPdfConv.GeneratePdf(html);
            Document document = new Document(user, invoicename, "application/pdf", 99);
            document.Contents = pdfBytes;
            return document;
        }

        [HttpGet]
        public async Task<IActionResult> RenewInformation(Guid id)
        {
            User user = null;

            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(id);
                user = await CurrentUser();
                if (clientProgramme == null)
                    throw new Exception("ClientProgramme (" + id + ") doesn't belong to User " + user.UserName);

                ClientProgramme newClientProgramme = await _programmeService.CloneForRewenal(clientProgramme, user);

                await _programmeService.Update(newClientProgramme);

                return Redirect("/Information/EditInformation/" + newClientProgramme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IssueDemoUIS(string id)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (!string.IsNullOrWhiteSpace(id))
                    user = await _userService.GetUser(id);

                // issues a demo UIS for every template in the system, assuming it hasn't been issued yet
                var templates = await _informationTemplateService.GetAllTemplates();
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var template in templates)
                    {
                        await _clientInformationService.IssueInformationFor(user, user.PrimaryOrganisation, template);
                    }
                    await uow.Commit();
                }
                return Redirect("~/Home/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        public async Task<InformationViewModel> GetInformationViewModel(ClientProgramme clientProgramme)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                var isSubsystem = await _programmeService.IsBaseClass(clientProgramme);
                var OrgUser = await _userService.GetUserByEmail(clientProgramme.InformationSheet.Owner.Email);
                List<Organisation> DefaultMarinas = await _organisationService.GetPublicMarinas();
                List<Organisation> DefaultInstitutes = await _organisationService.GetPublicFinancialInstitutes();
                
                Programme programme = clientProgramme.BaseProgramme;
                InformationViewModel model = new InformationViewModel(clientProgramme.InformationSheet, OrgUser, user)
                {
                    Name = programme.Name,
                    ProgNamedPartyName = programme.NamedPartyUnitName,
                    Sections = new List<InformationSectionViewModel>()
                };
                if (DefaultMarinas.Any())
                {                    
                    foreach(var marina in DefaultMarinas)
                    {
                        MarinaUnit unit = (MarinaUnit)marina.OrganisationalUnits.FirstOrDefault();
                        if (!model.ClientInformationSheet.WaterLocations.Contains(unit.WaterLocation))
                            model.ClientInformationSheet.WaterLocations.Add(unit.WaterLocation);
                    }                    
                }
                if (DefaultInstitutes.Any())
                {
                    foreach (var Institute in DefaultInstitutes)
                    {
                        InterestedPartyUnit unit = (InterestedPartyUnit)Institute.OrganisationalUnits.FirstOrDefault(i => i.Name == "Financial");
                        if (!model.ClientInformationSheet.Locations.Contains(unit.Location))
                        {
                            //model.ClientInformationSheet.Locations.Add(unit.Location);
                            model.OrganisationViewModel.PublicOrganisations.Add(Institute);
                        }
                    }
                }
                model.Name = programme.Name;
                model.ProgNamedPartyName = programme.NamedPartyUnitName;
                Product product = null;
                if (programme.Products.Count > 1)
                {
                    product = programme.Products.FirstOrDefault(progp => progp.IsMasterProduct);
                }
                else
                {
                    product = programme.Products.FirstOrDefault();
                }

                InformationTemplate informationTemplate;
                List<InformationSection> sections = new List<InformationSection>();                
                if (!isSubsystem)
                {
                    informationTemplate = product.SubInformationTemplate;
                    model.Wizardsteps = LoadWizardsteps("Standard");
                }
                else
                {
                    model.Wizardsteps = LoadWizardsteps("Subsystem");
                    //remove after checking with ray
                    if (product.InformationTemplate == null)
                    {
                        informationTemplate = await _informationTemplateService.GetTemplatebyProduct(product.Id);
                        product.InformationTemplate = informationTemplate;
                        await _productService.UpdateProduct(product);
                    }
                    informationTemplate = product.InformationTemplate;
                }
                sections = await _informationSectionService.GetInformationSectionsbyTemplateId(informationTemplate.Id);
                foreach (var section in sections)
                {
                    section.Items = section.Items.OrderBy(i => i.ItemOrder).ToList();
                }

                foreach (var section in informationTemplate.Sections)
                {
                    section.Items = section.Items.OrderBy(i => i.ItemOrder).ToList();
                }

                (model.Sections as List<InformationSectionViewModel>).InsertRange(model.Sections.Count(), _mapper.Map<InformationViewModel>(informationTemplate).Sections);

                model.Section = sections;
                return model;
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                throw ex;
            }
        }

        public async Task<InformationViewModel> GetClientInformationSheetViewModel(Guid sheetId)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientInformationSheet sheet = await _clientInformationService.GetInformation(sheetId);
                InformationViewModel model = await GetInformationViewModel(sheet.Programme);
                model.Sections = model.Sections.OrderBy(sec => sec.Position);
                model.ClientInformationSheet = sheet;

                foreach (var section in model.Sections)
                    foreach (var item in section.Items)
                    {
                        var answer = sheet.Answers.FirstOrDefault(a => a.ItemName == item.Name);
                        if (answer != null)
                            item.Value = answer.Value;
                    }

                return model;
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                throw ex;
            }
        }

        [HttpGet]
        public async Task<IActionResult> SendOnlineAcceptance(string ClientAgreement)
        {
            var clientAgreement = await _clientAgreementService.GetAgreement(Guid.Parse(ClientAgreement));
            var programme = clientAgreement.ClientInformationSheet.Programme.BaseProgramme;

            if (programme.ProgEnableEmail)
            {
                EmailTemplate emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == "SendAgreementOnlineAcceptanceInstructions");
                if (emailTemplate != null)
                {
                    //send out agreement online acceptance instruction email
                    await _emailService.SendEmailViaEmailTemplate(clientAgreement.ClientInformationSheet.Programme.Owner.Email, emailTemplate, null, null, null);
                    //send out login instruction email
                    await _emailService.SendSystemEmailLogin(clientAgreement.ClientInformationSheet.Programme.Owner.Email);
                    clientAgreement.SentOnlineAcceptance = true;
                    await _clientAgreementService.UpdateClientAgreement(clientAgreement);
                }
            }

            return await RedirectToLocal();
        }

        #region Commented code
        //[HttpPost]
        //public async Task<IActionResult> CreateInformationSheet(IFormCollection form)
        //{
        //    User currentUser = null;
        //    //Add User, Organisation, Information Sheet, Quick Term saving process here
        //    string organisationName = null;
        //    string ouname = null;
        //    string orgTypeName = null;

        //    try
        //    {
        //        var orgType = form["cgradioselect"];
        //        var orgName = form["fname"].ToList().First();
        //        var firstName = form["fname"].ToList().Last();
        //        var lastName = form["lname"];
        //        var mobilePhone = form["mphon"];
        //        var programmeList = await _programmeService.GetAllProgrammes();
        //        var programme = programmeList.LastOrDefault();
        //        var email = form["email"];
        //        var membershipNumber = form["memno"];

        //        currentUser = await CurrentUser();
        //        if (orgType == "Private") //orgType = "Private", "Company", "Trust", "Partnership"
        //        {
        //            organisationName = firstName + " " + lastName;
        //            ouname = "Home";
        //        }
        //        else
        //        {
        //            organisationName = orgName;
        //            ouname = "Head Office";
        //        }
        //        switch (orgType)
        //        {
        //            case "Private":
        //                {
        //                    orgTypeName = "Person - Individual";
        //                    break;
        //                }
        //            case "Company":
        //                {
        //                    orgTypeName = "Corporation – Limited liability";
        //                    break;
        //                }
        //            case "Trust":
        //                {
        //                    orgTypeName = "Trust";
        //                    break;
        //                }
        //            case "Partnership":
        //                {
        //                    orgTypeName = "Partnership";
        //                    break;
        //                }
        //            default:
        //                {
        //                    throw new Exception(string.Format("Invalid Organisation Type: ", orgType));
        //                }
        //        }
        //        string phonenumber = null;

        //        phonenumber = mobilePhone;

        //        OrganisationType organisationType = null;
        //        organisationType = await _organisationTypeService.GetOrganisationTypeByName(orgTypeName);
        //        if (organisationType == null)
        //        {
        //            organisationType = await _organisationTypeService.CreateNewOrganisationType(null, orgTypeName);
        //        }
        //        Organisation organisation = null;
        //        organisation = await _organisationService.GetOrganisationByEmail(email);
        //        organisation = new Organisation(null, Guid.NewGuid(), organisationName, organisationType);
        //        organisation.Phone = phonenumber;
        //        organisation.Email = email;
        //        await _organisationService.CreateNewOrganisation(organisation);
        //        User user = null;
        //        User user2 = null;

        //        try
        //        {
        //            user = await _userService.GetUserByEmail(email);
        //            if (!user.Organisations.Contains(organisation))
        //                user.Organisations.Add(organisation);
        //            var username = user.FirstName;
        //        }
        //        catch (Exception ex)
        //        {
        //            string username = firstName + "_" + lastName;

        //            try
        //            {
        //                user2 = await _userService.GetUser(username);

        //                if (user2 != null && user == user2)
        //                {
        //                    Random random = new Random();
        //                    int randomNumber = random.Next(10, 99);
        //                    username = username + randomNumber.ToString();
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                // create personal organisation
        //                //var personalOrganisation = new Organisation (CurrentUser(), Guid.NewGuid (), personalOrganisationName, new OrganisationType (CurrentUser(), "personal"));
        //                //_organisationService.CreateNewOrganisation (personalOrganisation);
        //                // create user object
        //                user = new User(null, Guid.NewGuid(), username);
        //                user.FirstName = firstName;
        //                user.LastName = lastName;
        //                user.FullName = firstName + " " + lastName;
        //                user.Email = email;
        //                user.MobilePhone = mobilePhone;
        //                user.Password = "";
        //                //user.Organisations.Add (personalOrganisation);
        //                // save the new user
        //                // creates a new user in the system along with a default organisation
        //                await _userService.Create(user);
        //                //Console.WriteLine ("Created User " + user.FullName);
        //            }
        //        }
        //        finally
        //        {
        //            if (!user.Organisations.Contains(organisation))
        //                user.Organisations.Add(organisation);

        //            user.SetPrimaryOrganisation(organisation);
        //            await _userService.Update(user);

        //            var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);
        //            var reference = await _referenceService.GetLatestReferenceId();
        //            var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
        //            await _referenceService.CreateClientInformationReference(sheet);

        //            using (var uow = _unitOfWork.BeginUnitOfWork())
        //            {
        //                OrganisationalUnit ou = new OrganisationalUnit(user, ouname);
        //                organisation.OrganisationalUnits.Add(ou);
        //                clientProgramme.BrokerContactUser = programme.BrokerContactUser;
        //                if (!string.IsNullOrWhiteSpace(membershipNumber))
        //                {
        //                    clientProgramme.ClientProgrammeMembershipNumber = membershipNumber;
        //                }
        //                sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
        //                try
        //                {
        //                    await uow.Commit();
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new Exception(ex.Message);
        //                }
        //            }
        //        }
        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, currentUser, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        // Important Notices - CK Editor

        // Oualification - Name, DateObtained

        // Parties - Party, Org Name, Org Type, Qualification
        #endregion

        #region Test Data
        private string _importantNotices = @"<table id=""Table3"" border=""1"" cellpadding=""4"" cellspacing=""0"" width=""100%"">
	                                            <tbody>
		                                            <tr>
			                                            <td style=""padding-top: 10px;"">
				                                            <h2 align=""center"">
					                                            <strong>
						                                            <font size=""5"">[[BrokerCompanyShort]] Professional Liability Package</font>
					                                            </strong>
				                                            </h2>
				                                            <p align=""center"">
					                                            <img src=""/Content/img/demo/WAHAP.jpg"">
                                                                    </p>
						                                            <h3 align=""center"">
							                                            <strong>
								                                            <font size=""3"">NOTICE TO THE PROPOSED INSURED</font>
							                                            </strong>
						                                            </h3>
					                                            </td>
				                                            </tr>
			                                            </tbody>
		                                            </table>
                                                    <p style=""padding-top: 10px;"">
                                                                                            &nbsp;
		                                            </p>
		                                            <div style=""text-align: center; width: 100%"">
			                                            <p/>
			                                            <table border=""0"" cellpadding=""0"" cellspacing=""0""  align=""center"">
				                                            <tbody>
					                                            <tr>
						                                            <td style=""text-align: center; border-right: red 2px solid; border-top: red 2px solid;
                                                                        border-left: red 2px solid; border-bottom: red 2px solid; padding-right: 10px;
                                                                        padding-top: 10px; padding-bottom: 10px; padding-left: 10px; width: 550px"" valign=""middle"">
							                                            <strong>Note:</strong> You can log off your proposal and return to it later at any stage.
                                                                                                            Any information you have already entered will be saved until you return to complete it.
						                                            </td>
					                                            </tr>
				                                            </tbody>
			                                            </table>
		                                            </div>
		                                            <p style=""padding-top: 10px;"">
			                                            <strong>Dear Health Professional,</strong>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <strong>Please familiarise yourself with the following: </strong>
		                                            </p>
		                                            <h3 style=""padding-top: 10px;"">
			                                            <strong>1. DISCLOSURE OF RELEVANT FACTS</strong>
		                                            </h3>
		                                            <p style=""padding-top: 10px;"">
			                                            <strong>
				                                            <span lang=""EN-GB"">Your Duty of </span>
				                                            <!--?xml namespace="""" ns=""urn:schemas-microsoft-com:office:smarttags"" prefix=""st1"" ?-->
				                                            <st1:personname>
					                                            <span lang=""EN-GB""/>
				                                            </st1:personname>
				                                            <span lang=""EN-GB"">Disclosure</span>
			                                            </strong>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang=""EN-GB"">
                                                                                                Before you enter into a contract of general insurance with an insurer, you have a duty to disclose
                                                                                                to the insurer every matter which you know, or could reasonably be expected to know, is
                                                                                                relevant to the insurer's decision whether to accept the risk of the insurance and, if so,
                                                                                                on what terms.
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang=""EN-GB"">
                                                                                                You have the same duty to disclose those matters before you renew, extend,
                                                                                                vary or reinstate a contract of insurance.
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang=""EN-GB"">
                                                                                                Your duty however does not require disclosure of a matter:
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px; padding-left: 40px;""> <ul type=""disc"">
                                        <li class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; mso-margin-top-alt: auto; mso-margin-bottom-alt: auto;
                    mso-list: l0 level1 lfo1; tab-stops: list 36.0pt"" ><span lang=""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"" > that diminishes the risk to be undertaken by the insurer </span></li>
                                        <li class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; mso-margin-top-alt: auto; mso-margin-bottom-alt: auto;
                    mso-list: l0 level1 lfo1; tab-stops: list 36.0pt"" ><span lang=""EN-GB"" style=""padding-top: 10px; padding-left: 40px;""> that is common knowledge </span></li>
                                        <li class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; mso-margin-top-alt: auto; mso-margin-bottom-alt: auto;
                    mso-list: l0 level1 lfo1; tab-stops: list 36.0pt"" >
                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                that the insurer knows or, in the ordinary course of business as an insurer, ought
                                                to know
                                            </span>
                                        </li>
                                        <li class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; mso-margin-top-alt: auto; mso-margin-bottom-alt: auto;
                    mso-list: l0 level1 lfo1; tab-stops: list 36.0pt"" ><span lang=""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">as to which compliance with your duty is waived by the insurer.</span></li>
                                    </ul></p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
				                                            <strong>NON-DISCLOSURE</strong>
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
                                                                                                If you fail to comply with your duty of disclosure, the insurer may be entitled to reduce
                                                                                                its liability under the contract in respect of a claim or may cancel the contract.
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
                                                                                                If your non-disclosure is fraudulent, the insurer may also have the option
                                                                                                of avoiding the contract from its beginning.
                                                        </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
				                                            <strong>COMMENT</strong>
			                                            </span>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
			                                            <em>
				                                            <b>
					                                            <span lang = ""EN-GB"">
                                                                                                        The requirement of full and frank disclosure of anything which may
                                                                                                        be material to the risk for which you seek cover (eg.claims, whether founded or unfounded),
                                                                                                        or to the magnitude of the risk, is of the utmost importance with this type of insurance.
                                                                                                        It is better to err on the side of caution by disclosing anything which might conceivably
                                                                                                        influence the insurer's consideration of your proposal.
					                                            </span>
				                                            </b>
			                                            </em>
		                                            </p>
		                                            <p style=""padding-top: 10px;"">
                                                                                            &nbsp;
		                                            </p>
		                                            <table id = ""Table2"" border= ""1"" cellpadding= ""0"" cellspacing= ""0"" width= ""100%"">
			                                            <tbody>
				                                            <tr>
					                                            <td style=""padding-top: 10px;"">
						                                            <h3>
							                                            <strong>2. FAILURE TO COMPLETE PROPOSAL FORM</strong>
						                                            </h3>
						                                            <p style=""padding-top: 10px;"">
                                                                                                            Failure to fully complete or return this completed proposal form
                                                                                                            will result in you having no current cover.
						                                            </p>
						                                            <p style=""padding-top: 10px;"">
                                                                                                            Should any of the questions not be completed and left blank, this proposal
                                                                                                            form will be returned to you immediately for completion and return. Cover
                                                                                                            will not be granted until such time the fully completed proposal form is returned.
						                                            </p>
					                                            </td>
				                                            </tr>
			                                            </tbody>
		                                            </table>
		                                            <h3 style=""padding-top: 10px;"">
			                                            <strong>3. CLAIMS MADE POLICY</strong>
		                                            </h3>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
                                                                                                This proposal is for a ""claims made"" policy of insurance.This means that the policy covers you for claims made against you
                                                                                                and notified to the insurer during the period of cover. This policy does not provide cover in relation to:
			                                            </span>
		                                            </p>
		                                            <ul >
			                                            <li >
				                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    events that occurred prior to the retroactive date of the policy
                                                                                                    (if such a date is specified);
				                                            </span>
			                                            </li>
			                                            <li >
				                                            <span lang=""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    claims made after the expiry of the period of cover even though the event giving
                                                                                                    rise to the claim may have occurred during the period of cover;
				                                            </span>
			                                            </li>
			                                            <li >
				                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    claims notified or arising out of facts or circumstances notified(or which ought
                                                                                                    reasonably to have been notified) under any previous policy;
				                                            </span>
			                                            </li>
			                                            <li >
				                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    claims made, threatened or intimated against you prior to the commencement of the
                                                                                                    period of cover;
				                                            </span>
			                                            </li>
			                                            <li >
				                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    facts or circumstances which you first became aware of prior to the period of cover,
                                                                                                    and which you knew or ought reasonably to have known had the potential to give rise
                                                                                                    to a claim under this policy;
				                                            </span>
			                                            </li>
			                                            <li >
				                                            <span lang = ""EN-GB"" style=""padding-top: 10px; padding-left: 40px;"">
                                                                                                    claims arising out of circumstances noted on the proposal form for the current period
                                                                                                    of cover or on any previous proposal form.
				                                            </span>
			                                            </li>
		                                            </ul>
		                                            <p style=""padding-top: 10px;"">
			                                            <span lang = ""EN-GB"">
                                                                                                However, where you give notice in writing to the insurer of any facts that might give rise to a claim against you as soon as
                                                                                                reasonably practicable after you become aware of those facts but before the expiry of the period of cover,
                                                                                                the policy will, subject to the terms and conditions, cover you notwithstanding that a claim is only made after the expiry of the period of cover.
                                                        </span>
		                                            </p>
                                                    <p style=""padding-top: 10px;"">
                                                                                            &nbsp;
		                                            </p>
		                                            <div style = ""text-align: center; width: 100%"">
			                                            <table border = ""0"" cellpadding=""0"" cellspacing=""0"" align=""center"">
				                                            <tbody>
					                                            <tr>
						                                            <td style = ""text-align: center; border-right: red 2px solid; border-top: red 2px solid;
        border-left: red 2px solid; border-bottom: red 2px solid; padding-right: 10px;
                                                                        padding-top: 10px; padding-bottom: 10px; padding-left: 10px; width: 550px"" valign=""middle"">
							                                            <strong>Note:</strong> You can log off your proposal and return to it later at any
                                                                                                            stage. Any information you have already entered will be saved until you return to
                                                                                                            complete it.
						                                            </td>
					                                            </tr>
				                                            </tbody>
			                                            </table>";

        private string _avaliableCover = @"<table id=""tblQ"" cellspacing=""0"" cellpadding=""8"" border=""0"">
                                            <tr>
                                                <td colspan = ""2"" style=""padding-top: 10px;"" >
                                                    <table id=""tblCover"" cellspacing=""0"" cellpadding=""8"" border=""1"" width=""50%"" >
                                                        <tr>
                                                            <td style = ""background-color: silver"" style=""padding-top: 10px;"">
                                                                <strong> Insurance Package</strong>
                                                            </td>
                                                            <td style = ""background-color: silver"" style=""padding-top: 10px;"">
                                                                <strong> Your Insurance package will contain the following policies and limits of indemnity:</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""padding-top: 10px;"">
                                                                Medical Malpractice
                                                            </td>
                                                            <td style=""padding-top: 10px;"">$500,000 any one claim and in the aggregate</td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""padding-top: 10px;"">Public Liability</td>
                                                            <td style=""padding-top: 10px;"">$1,000,000 any one occurrence</td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""padding-top: 10px;"">Statutory Liability</td>
                                                            <td style=""padding-top: 10px;"">$500,000 any one claim and in the aggregate</td>
                                                        </tr>
                                                    </table>
                                                </td>

                                            </tr>
                                            <tr>
                                                <td style=""padding-top: 10px;"">
                                                    <strong>Annual Package Premium:</strong> $294.11 inc GST, Credit Card surcharge and broker fees per member
                                                </td>
                                            </tr>

                                            <tr>
                                                <td style=""padding-top: 10px;"">
                                                    <h2 style = ""text-align: center"" style=""padding-top: 10px;"">
                                                        <span style= ""color: #ff0000"" >
                                                            <strong>Important Notice</strong>
                                                        </span>
                                                    </h2>
                                                    <p style=""padding-top: 10px;"">
                                                        <strong>
                                                            Thank you for completing your Application.
                                                        </strong>
                                                    </p>
                                                    <p style=""padding-top: 10px;"">
                                                        <strong>

                                                            If your application is referred to [[InsurerCompanyShort]] and [[BrokerCompanyShort]] for manual processing [[BrokerCompanyShort]] will contact you for more information shortly,
                                                            otherwise your cover will be bound and your Insurance will be in place on receipt of your payment.
                                                            Your policy documents will be sent to you in due course.
                                                        </strong>
                                                    </p>
                                                    <p style=""padding-top: 10px;"">
                                                        <strong>
                                                            Please check the policies and ensure the Insurance is issued in accordance with your requirements.
                                                            If you have any questions please contact [[BrokerName]] at [[BrokerCompanyShort]] via email at
                                                        </strong><a href = ""[[BrokerEmail]]"" ><strong> [[BrokerEmail]]</strong></a><strong>.</strong>
                                                    </p>

                                                </td>
                                            </tr>
                                        </table>";

        private string _declaration = @"<P style=""padding-bottom: 20px;""><STRONG>On behalf of the applicant:</STRONG></P>
                                        <OL>
                                            <LI>
                                                <STRONG>
                                                    I declare that I am the person named above and I am authorised to submit this
                                                    proposal on behalf of the Applicant.<br />
                                                    &nbsp;
                                                </STRONG>
                                            </LI>
                                            <LI>
                                                <STRONG>
                                                    I declare that the information and answers given in this proposal have been
                                                    checked and are true and complete in every respect and the Applicant is not
                                                    aware of any other information that may be material in considering this
                                                    proposal.
                                                </STRONG>
                                                <br />
                                                &nbsp;
                                            </LI>
                                            <LI>
                                                <STRONG>
                                                    I acknowledge that this proposal, declaration and any other information
                                                    supplied in support of this proposal constitutes representations to, and will
                                                    be relied on as the basis of contract by, insurers requested to quote on this
                                                    proposal. We undertake to inform these insurers through our broker of any
                                                    material alteration to this information whether occurring before or after the
                                                    completion of any insurance contract.<br />
                                                    &nbsp;
                                                </STRONG>
                                            </LI>
                                            <LI>
                                                <STRONG>
                                                    I acknowledge that misrepresentations or material non-disclosure of
                                                    relevant information, whether made through this proposal or otherwise, may
                                                    result in the insurance not being available to meet a claim and/or cancellation
                                                    of relevant insurance contract(s), in addition to other remedies.<br />
                                                    &nbsp;
                                                </STRONG>
                                            </LI>
                                            <LI>
                                                <STRONG>
                                                    I confirm that the applicant authorises the disclosure to insurers
                                                    requested to quote on this proposal, of information held by other insurers or
                                                    insurance brokers.<br />
                                                    &nbsp;
                                                </STRONG>
                                            </LI>
                                            <LI>
                                                <STRONG>
                                                    I declare that I am a current member or employee of <br />
                                                    &nbsp;&nbsp;&nbsp;a) Wellness and Health Associated Professionals (NZ) Inc
                                                </STRONG>
                                            </LI>
                                        </OL>
                                        <p align=""center"">
                                        <button class=""btn btn-primary"" type = ""submit"" style=""width:150px"">Agree and Submit</button>
                                        </p>";
        #endregion
    }
}
