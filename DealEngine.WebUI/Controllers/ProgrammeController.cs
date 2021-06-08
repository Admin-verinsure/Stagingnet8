using AutoMapper;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Payment.EGlobalAPI;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Helpers;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.ProductModels;
using DealEngine.WebUI.Models.Programme;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemDocument = DealEngine.Domain.Entities.Document;
using UpdateType = DealEngine.Domain.Entities.UpdateType;

//using DocumentFormat.OpenXml.Wordprocessing;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class ProgrammeController : BaseController
    {
        ILogger<ProgrammeController> _logger;
        IInformationTemplateService _informationService;
        IUnitOfWork _unitOfWork;
        IApplicationLoggingService _applicationLoggingService;
        IProductService _productService;
        IOrganisationService _organisationService;
        IRiskCategoryService _riskCategoryService;
        IRiskCoverService _riskCoverService;
        IBusinessActivityService _busActivityService;
        IMapperSession<Document> _documentRepository;
        IProgrammeService _programmeService;
        ISharedDataRoleService _sharedDataRoleService;
        IFileService _fileService;
        IEmailService _emailService;
        IRuleService _ruleService;
        IMapper _mapper;
        IHttpClientService _httpClientService;
        IEGlobalSubmissionService _eGlobalSubmissionService;
        IImportService _importService;
        IClaimService _claimService;
        ISerializerationService _serializerationService;
        IUpdateTypeService _updateTypeServices;
        public ProgrammeController(
            ISerializerationService serializerationService,
            IClaimService claimService,
            IImportService importService,
            IOrganisationService organisationService,
            IRiskCoverService riskCoverService,
            IRiskCategoryService riskCategoryService,
            IProductService productService,
            IApplicationLoggingService applicationLoggingService,
            ILogger<ProgrammeController> logger,
            IUserService userRepository,
            IInformationTemplateService informationService,
            IUnitOfWork unitOfWork,
            IRuleService ruleService,
            IMapperSession<Document> documentRepository,
            IBusinessActivityService busActivityService,
            ISharedDataRoleService sharedDataRoleService,
            IProgrammeService programmeService,
            IFileService fileService,
            IEmailService emailService,
            IMapper mapper,
            IHttpClientService httpClientService,
            IEGlobalSubmissionService eGlobalSubmissionService,
                    IUpdateTypeService updateTypeService
            )
            : base(userRepository)
        {
            _serializerationService = serializerationService;
            _claimService = claimService;
            _importService = importService;
            _applicationLoggingService = applicationLoggingService;
            _productService = productService;
            _logger = logger;
            _sharedDataRoleService = sharedDataRoleService;
            _informationService = informationService;
            _unitOfWork = unitOfWork;
            _riskCoverService = riskCoverService;
            _riskCategoryService = riskCategoryService;
            _organisationService = organisationService;
            _busActivityService = busActivityService;
            _documentRepository = documentRepository;
            _programmeService = programmeService;
            _unitOfWork = unitOfWork;
            _ruleService = ruleService;
            _fileService = fileService;
            _emailService = emailService;
            _mapper = mapper;
            _httpClientService = httpClientService;
            _eGlobalSubmissionService = eGlobalSubmissionService;
            _updateTypeServices = updateTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> MyProgrammes()
        {
            BaseListViewModel<ProgrammeInfoViewModel> models = new BaseListViewModel<ProgrammeInfoViewModel>();
            User user = null;

            try
            {
                user = await CurrentUser();
                var programmeList = await _programmeService.GetAllProgrammes();
                var programmes = programmeList.Where(p => p.Owner == user.PrimaryOrganisation);

                foreach (Programme programme in programmes)
                {
                    ProgrammeInfoViewModel model = new ProgrammeInfoViewModel
                    {
                        DateCreated = LocalizeTime(programme.DateCreated.GetValueOrDefault()),
                        Id = programme.Id,
                        Name = programme.Name + " for " + programme.Owner.Name,
                        OwnerCompany = programme.Owner.Name,
                    };
                    models.Add(model);

                }
                return View("AllProgrammes", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return View("AllProgrammes");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllProgrammes()
        {
            User user = null;

            try {
                user = await CurrentUser();
                var programmeList = await _programmeService.GetAllProgrammes();
                var programmes = programmeList.Where(p => p.DateDeleted == null);
                BaseListViewModel<ProgrammeInfoViewModel> models = new BaseListViewModel<ProgrammeInfoViewModel>();
                foreach (Programme p in programmes) {
                    ProgrammeInfoViewModel model = new ProgrammeInfoViewModel
                    {
                        DateCreated = LocalizeTime(p.DateCreated.GetValueOrDefault()),
                        Id = p.Id,
                        Name = p.Name,
                        OwnerCompany = p.Owner.Name,
                    };
                    // ClientProgramme programme = _programmeService.GetClientProgrammesForProgramme(Id);

                    models.Add(model);
                }
                return View(models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageClient(Guid Id)
        {
            ProgrammeInfoViewModel model;
            User user = null;
            List<ClientProgramme> clientProgrammes = new List<ClientProgramme>();
            List<Organisation> Owners = new List<Organisation>();
            List<Organisation> Ownerlist = new List<Organisation>();

            try
            {
                user = await CurrentUser();
                
                var clientProgrammeList = await _programmeService.GetClientProgrammesForProgramme(Id);
                model = new ProgrammeInfoViewModel(null, clientProgrammeList.FirstOrDefault().BaseProgramme, null);
                foreach (var programme in clientProgrammeList)
                {
                    Ownerlist.Add(programme.Owner);
                    clientProgrammes.Add(programme);
                    model.Name = programme.BaseProgramme.Name;

                }
                Ownerlist.Select(x => x.Name).Distinct();

                foreach (var owner in Ownerlist)
                {
                    Owners.Add(owner);
                }
                //Notes.Select(x => x.Author).Distinct();
                model.Owner = Owners;

                ViewBag.Title = "Term Sheet Template ";
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivityTemplates(IFormCollection form)
        {
            User user = null;
            try
            {
                var programmeId = form["ActivityAttach.SelectedProgramme"].ToString();
                var templateIds = form["Builder.Activities"].ToArray().ToList();
                var isPublic = bool.Parse(form["Builder.Ispublic"].ToString());
                Programme programme = await _programmeService.GetProgramme(Guid.Parse(programmeId));
                foreach (var id in templateIds)
                {
                    BusinessActivityTemplate businessActivityTemplate = await _busActivityService.GetBusinessActivityTemplate(Guid.Parse(id));
                    await _programmeService.AttachProgrammeToActivities(programme, businessActivityTemplate);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActivityBuilder(Guid Id)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                //reupload everytime
                var busActivityList = await _busActivityService.GetBusinessActivitiesTemplates();
                await _importService.ImportActivities(user);                
                busActivityList = await _busActivityService.GetBusinessActivitiesTemplates();
                var actClassOne = await _busActivityService.GetBusinessActivitiesByClassification(1);
                var actClassTwo = await _busActivityService.GetBusinessActivitiesByClassification(2);
                var actClassThree = await _busActivityService.GetBusinessActivitiesByClassification(3);
                var actClassFour = await _busActivityService.GetBusinessActivitiesByClassification(4);

                ActivityViewModel model = new ActivityViewModel
                {
                    Builder = new ActivityBuilderVM
                    {
                        Ispublic = false,
                        Activities = new List<SelectListItem>(),
                        Level1Classifications = actClassOne,
                        Level2Classifications = actClassTwo,
                        Level3Classifications = actClassThree,
                        Level4Classifications = actClassFour,
                    },
                    ActivityCreate = new ActivityModal()
                };

                model.Id = Id;

                foreach (var item in busActivityList)
                {
                    model.Builder.Activities.Add(new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.AnzsciCode + " --- " + item.Description,
                    });
                }

                List<SelectListItem> proglist = new List<SelectListItem>();
                var programmeList = await _programmeService.GetAllProgrammes();
                foreach (Programme prog in programmeList)
                {
                    proglist.Add(new SelectListItem
                    {
                        Selected = false,
                        Text = prog.Name,
                        Value = prog.Id.ToString(),
                    });
                }

                model.ActivityAttach = new ActivityAttachVM()
                {
                    BaseProgList = proglist
                };

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBusinessActivity(ActivityModal model)
        {
            User user = null;
            IList<BusinessActivityTemplate> BAList = new List<BusinessActivityTemplate>();

            try
            {
                user = await CurrentUser();

                if (model.ClassOne != null)
                {
                    BusinessActivityTemplate ba1 = new BusinessActivityTemplate(user)
                    {
                        AnzsciCode = model.ClassOne.AnzsciCode,
                        Description = model.ClassOne.Description,
                        Classification = model.ClassOne.Classification
                    };
                    BAList.Add(ba1);
                }

                if (model.ClassTwo != null)
                {
                    BusinessActivityTemplate ba2 = new BusinessActivityTemplate(user)
                    {
                        AnzsciCode = model.ClassTwo.AnzsciCode,
                        Description = model.ClassTwo.Description,
                        Classification = model.ClassTwo.Classification
                    };
                    BAList.Add(ba2);
                }

                if (model.ClassThree != null)
                {
                    BusinessActivityTemplate ba3 = new BusinessActivityTemplate(user)
                    {
                        AnzsciCode = model.ClassThree.AnzsciCode,
                        Description = model.ClassThree.Description,
                        Classification = model.ClassThree.Classification
                    };
                    BAList.Add(ba3);
                }

                if (model.ClassFour != null)
                {
                    BusinessActivityTemplate ba4 = new BusinessActivityTemplate(user)
                    {
                        AnzsciCode = model.ClassFour.AnzsciCode,
                        Description = model.ClassFour.Description,
                        Classification = model.ClassFour.Classification
                    };
                    BAList.Add(ba4);
                }

                foreach (BusinessActivityTemplate businessActivity in BAList)
                {
                    await _busActivityService.CreateBusinessActivityTemplate(businessActivity);
                }

                return Redirect("/Programme/ActivityBuilder");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SharedDataRoleBuilder()
        {
            SharedRoleTemplateViewModel model = new SharedRoleTemplateViewModel();
            model.Roles = new List<SelectListItem>();
            model.BaseProgList = new List<SelectListItem>();
            User user = null;

            try
            {
                user = await CurrentUser();
                var programmeList = await _programmeService.GetProgrammesByOwner(user.PrimaryOrganisation.Id);
                var sharedRoleList = await _sharedDataRoleService.GetRolesByOwner(user.PrimaryOrganisation.Id);

                if (programmeList.Count != 0)
                {
                    foreach (var programme in programmeList)
                    {
                        model.BaseProgList.Add(new SelectListItem
                        {
                            Text = programme.Name,
                            Value = programme.Id.ToString()
                        });
                    }
                }

                if (sharedRoleList.Count != 0)
                {
                    foreach (var template in sharedRoleList)
                    {
                        model.Roles.Add(new SelectListItem
                        {
                            Text = template.Name,
                            Value = template.Id.ToString()
                        });
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSharedDataRoleTemplate(string TemplateName, bool IsPublic)
        {
            User user = null;
            var newSharedRole = new SharedDataRoleTemplate();
            newSharedRole.IsPublic = IsPublic;
            newSharedRole.Name = TemplateName;

            try
            {
                user = await CurrentUser();
                newSharedRole.Organisation = user.PrimaryOrganisation;
                user = await CurrentUser();
                await _sharedDataRoleService.CreateSharedDataRoleTemplate(newSharedRole);

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgrammeSharedRoles(string ProgrammeId,  string[] TemplateNames)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Form has not been completed");
                throw new Exception("Form has not been completed");
            }

            User user = null;
            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgramme(Guid.Parse(ProgrammeId));
                foreach (string str in TemplateNames)
                {
                    var sharedRole = await _sharedDataRoleService.GetSharedRoleTemplateById(Guid.Parse(str));
                    await _programmeService.AttachProgrammeToSharedRole(programme, sharedRole);
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
        public async Task<IActionResult> SendInvoice(Guid programmeId)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme programme = await _programmeService.GetClientProgramme(programmeId);
                if (programme.EGlobalClientNumber == null)
                {
                    throw new NullReferenceException("Client number is null");
                }

                var eGlobalSerializer = new EGlobalSerializerAPI();

                //check Eglobal parameters
                if (string.IsNullOrEmpty(programme.EGlobalClientNumber))
                {
                    throw new Exception(nameof(programme.EGlobalClientNumber) + " EGlobal client number");
                }
                string paymentType = "Credit";
                Guid transactionreferenceid = Guid.NewGuid();

                var xmlPayload = eGlobalSerializer.SerializePolicy(programme, user, _unitOfWork, transactionreferenceid, paymentType, false, false, null);

                var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload);

                //used for eglobal request and response log 
                await _emailService.EGlobalLogEmail("marshevents@proposalonline.com", transactionreferenceid.ToString(), xmlPayload, byteResponse);

                EGlobalSubmission eglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmissionByTransaction(transactionreferenceid);

                eGlobalSerializer.DeSerializeResponse(byteResponse, programme, user, _unitOfWork, eglobalsubmission);

                if (programme.ClientAgreementEGlobalResponses.Count > 0)
                {
                    EGlobalResponse eGlobalResponse = programme.ClientAgreementEGlobalResponses.Where(er => er.DateDeleted == null && er.ResponseType == "update").OrderByDescending(er => er.VersionNumber).FirstOrDefault();
                    if (eGlobalResponse != null)
                    {
                        var status = "Bound and invoiced";

                        var documents = new List<SystemDocument>();
                        foreach (ClientAgreement agreement in programme.Agreements)
                        {
                            if (agreement.MasterAgreement && (agreement.ReferenceId == eGlobalResponse.MasterAgreementReferenceID))
                            {
                                foreach (SystemDocument doc in agreement.Documents.Where(d => d.DateDeleted == null && d.DocumentType == 4))
                                {
                                    doc.Delete(user);
                                }
                                foreach (SystemDocument template in agreement.Product.Documents)
                                {
                                    //render docs invoice
                                    if (template.DocumentType == 4)
                                    {
                                        SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, agreement, null);
                                        renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                        agreement.Documents.Add(renderedDoc);
                                        documents.Add(renderedDoc);
                                        await _fileService.UploadFile(renderedDoc);
                                    }
                                }
                            }
                        }

                        foreach (ClientAgreement agreement in programme.Agreements)
                        {
                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                if (agreement.Status != status)
                                {
                                    agreement.Status = status;
                                    await uow.Commit();
                                }
                            }
                            agreement.Status = status;
                        }
                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {
                            if (programme.InformationSheet.Status != status)
                            {
                                programme.InformationSheet.Status = status;
                                await uow.Commit();
                            }
                        }
                    }

                }
                
                var url = "/Agreement/ViewAcceptedAgreement/" + programme.Id;
                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClientProgrammeDetails(Guid programmeId, Guid Id,Guid ownerId)
        {
            ClientProgrammeInfoViewModel clientviewmodel = new ClientProgrammeInfoViewModel();
            List<ClientProgramme> clientProgrammes = new List<ClientProgramme>();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme programme = await _programmeService.GetClientProgramme(programmeId);
                clientviewmodel.Id = Id;
                clientviewmodel.OwnerId = ownerId;
                clientviewmodel.ProgramId = programmeId;
                clientviewmodel.Name = programme.Owner.Name;
                clientviewmodel.Phone = programme.Owner.Phone;
                clientviewmodel.Email = programme.Owner.Email;
                clientviewmodel.DateCreated = (DateTime)programme.Owner.DateCreated;
                clientviewmodel.Status = programme.InformationSheet.Status;

                ViewBag.Title = "Term Sheet Template ";
                return View(clientviewmodel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditBillingConfiguration(Guid programmeId)
        {
            ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(programmeId);
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel(null, clientProgramme.BaseProgramme, clientProgramme);
            User user = await CurrentUser();
            try
            {
                string Active = "Not Active";
                
                model.BrokerContactUser = clientProgramme.BaseProgramme.BrokerContactUser;
                model.EGlobalSubmissions = clientProgramme.ClientAgreementEGlobalSubmissions;

                if (clientProgramme.ClientAgreementEGlobalSubmissions.Any())
                {
                    foreach (EGlobalSubmission esubmission in clientProgramme.ClientAgreementEGlobalSubmissions)
                    {
                        string submissiondiscription = esubmission.DateCreated.Value.ToTimeZoneTime(UserTimeZone).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));

                        if (esubmission.EGlobalResponse != null)
                        {
                            submissiondiscription += " - response received";
                            if (esubmission.EGlobalResponse.ResponseType == "update")
                            {
                                submissiondiscription += "(success) - " + esubmission.EGlobalResponse.TranCode + " - invoice #:" + esubmission.EGlobalResponse.InvoiceNumber;
                            }
                            else
                            {
                                submissiondiscription += "(error)";
                            }
                        }
                        else
                        {
                            submissiondiscription += " - no response";
                        }

                        using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                        {
                            esubmission.SubmissionDesc = submissiondiscription;
                            await uow.Commit();
                        }

                    }
                }
                Active = await _httpClientService.GetEglobalStatus();
                model.EGlobalIsActiveOrNot = (Active == "ACTIVE") ? true : false;
                
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                model.EGlobalIsActiveOrNot = false;
                return View(model);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> ReverseTransaction(Guid transId)
        {
            EGlobalSubmission originalEglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmission(transId);
            User user = null;
            user = await CurrentUser();
            var eGlobalSerializer = new EGlobalSerializerAPI();
            string paymentType = "Credit";
            Guid transactionreferenceid = Guid.NewGuid();

            var xmlPayload = eGlobalSerializer.SerializePolicy(originalEglobalsubmission.EGlobalSubmissionClientProgramme, user, _unitOfWork, transactionreferenceid, paymentType, true, false, originalEglobalsubmission);

            var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload);

            //used for eglobal request and response log 
            await _emailService.EGlobalLogEmail("marshevents@proposalonline.com", transactionreferenceid.ToString(), xmlPayload, byteResponse);

            EGlobalSubmission eglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmissionByTransaction(transactionreferenceid);

            eGlobalSerializer.DeSerializeResponse(byteResponse, originalEglobalsubmission.EGlobalSubmissionClientProgramme, user, _unitOfWork, eglobalsubmission);

            await _programmeService.Update(originalEglobalsubmission.EGlobalSubmissionClientProgramme).ConfigureAwait(false);

            var url = "/Agreement/ViewAcceptedAgreement/" + originalEglobalsubmission.EGlobalSubmissionClientProgramme.Id;
            return Json(new { url });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBillingConfiguration(string[] billingConfig, Guid programmeId)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme programme = await _programmeService.GetClientProgramme(programmeId);
                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    programme.EGlobalBranchCode = billingConfig[0];
                    programme.EGlobalClientNumber = billingConfig[1];

                    await uow.Commit();

                }

                return Redirect("EditBillingConfiguration" + programmeId);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditClientProgrammeDetails(Guid programmeId, Guid Id, Guid OwnerId)
        {
            ClientProgrammeInfoViewModel clientviewmodel = new ClientProgrammeInfoViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientProgramme programme = await _programmeService.GetClientProgramme(programmeId);
                clientviewmodel.Name = programme.Owner.Name;
                clientviewmodel.Email = programme.Owner.Email;
                clientviewmodel.Phone = programme.Owner.Phone;
                clientviewmodel.OwnerId = OwnerId;
                clientviewmodel.ProgramId = programmeId;                
                clientviewmodel.Id = Id;

                ViewBag.Title = "Term Sheet Template ";
                return View(clientviewmodel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveClientProgrammeDetails(Guid programme_id, ClientProgrammeInfoViewModel clientviewmodel, Guid id)
        {
            User user = null;
            ClientProgramme programme = await _programmeService.GetClientProgramme(programme_id);            
            Guid ownerid = clientviewmodel.OwnerId;

            try
            {
                user = await CurrentUser();
                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    var owner = programme.Owner;                    
                    owner.Phone = clientviewmodel.Phone;
                    owner.Email = clientviewmodel.Email;
                    await uow.Commit().ConfigureAwait(false);
                }
                ViewBag.Title = "Term Sheet Template ";

                return RedirectToAction("ClientProgrammeDetails", new { programmeId = programme_id, Id = id, ownerId = ownerid });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }            
        }
     
        [HttpGet]
        public async Task<IActionResult> EmailTemplate(Guid Id)
        {
            //BaseListViewModel<ProgrammeInfoViewModel> models = new BaseListViewModel<ProgrammeInfoViewModel>();
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                model.Id = Id;
                ViewBag.Title = "Programme Email Template ";
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateType(Guid ProgrammeId)
        {
            User user = null;
            UpdateTypesViewModel model = new UpdateTypesViewModel();

            try
            {
                user = await CurrentUser();
                var dbUpdatemodelTypes = await _updateTypeServices.GetAllUpdateTypes();
                var updateTypeModel = new List<UpdateTypesViewModel>();



                model.Id = ProgrammeId;
               Programme Programme = await _programmeService.GetProgrammeById(ProgrammeId);
               



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

                model.SelectedUpdateTypes = new List<string>();

                foreach (var updateType in Programme.UpdateTypes)
                {
                    using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (model.SelectedUpdateTypes != null)
                        { 
                            model.SelectedUpdateTypes.Add(updateType.TypeValue);
                        }
                        await uow.Commit();
                    }
                }


                foreach (var updateType in dbUpdatemodelTypes.Where(t => t.DateDeleted == null))
                {
                    updateTypeModel.Add(new UpdateTypesViewModel
                    {
                        Id = updateType.Id,
                        NameType = updateType.TypeName,
                        ValueType = updateType.TypeValue,
                        TypeIsBroker = updateType.TypeIsBroker,
                        TypeIsClient = updateType.TypeIsClient,
                        TypeIsInsurer = updateType.TypeIsInsurer,
                        TypeIsTc = updateType.TypeIsTc
                    });


                }
                model.UpdateTypes = updateTypeModel.OrderBy(acat => acat.UpdateTypes).ToList();



                return View(model);
            }

            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpPost]
        public async Task<IActionResult> UpdateType(IFormCollection formCollection)
        {
            User user = null;

            try
            {
                Programme programme = null;
               UpdateType updateType = null;
                Programme UpdateTypes = null;
                user = await CurrentUser();
                programme = await _programmeService.GetProgramme(Guid.Parse(formCollection["ProgrammeId"]));
                

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    programme.UpdateTypes.Clear();
                    await uow.Commit();
                }

                var updateTypes = new List<UpdateType>();

                foreach (var key in formCollection.Keys)
                {
                
                   var keyCheck = key;
                    if (keyCheck != "__RequestVerificationToken")
                   {
                        updateType = await _updateTypeServices.GetUpdateType(Guid.Parse(formCollection[key]));
                        if (updateType != null)
                       {
                            using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                            {
                                programme.UpdateTypes.Add(updateType);

                               // programme.UpdateTypes = updateTypes;
                                await uow.Commit();
                            }

                        }
                    }
                }


                // await _updateTypeServices.Update(updateType);

                 return RedirectToAction("UpdateType", new { ProgrammeId = programme.Id });

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }






        [HttpGet]
        public async Task<IActionResult> TermSheetTemplate(Guid Id)
        {
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                model.Id = Id;
                ViewBag.Title = "Term Sheet Template ";
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TermSheetConfirguration(Guid Id)
        {
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            User user = null;
            
            try
            {
                Programme programme = await _programmeService.GetProgramme(Id);
                model.Id = Id;
                model.Name = programme.Name;
                ViewBag.Title = "Term Sheet Template ";
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> ProductRules(Guid Id, Guid productId)
        {
            //return NoContent();
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            var rules = new List<Rule>();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgramme(Id);
                model.Id = Id;

               
                    var product = await _productService.GetProductById(productId);

                    foreach (var rule in product.Rules)
                    {
                        rules.Add(rule);
                    }
               
                model.Rules = rules;
                model.ProductId = productId;

                ViewBag.Title = "Manage Product Rules";

                return View("ProductRules", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditRule(ClientAgreementRuleViewModel rule )
        {
            User user = null;
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            var rules = new List<Rule>();

            try
            {
                user = await CurrentUser();
                Rule Rule = await _ruleService.GetRuleByID(rule.ClientAgreementRuleID);
                if (Rule != null)
                {
                    try
                    {
                        using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                        {
                            Rule.Name = rule.Name;
                            Rule.Description = rule.Description;
                            Rule.OrderNumber = rule.OrderNumber;
                            Rule.Value = rule.Value;
                            await uow.Commit();
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                ViewBag.Title = "Manage Product Rules";
                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ManageRules(Guid Id,string RuleType)
        {
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            var product = new List<ProductInfoViewModel>();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(Id);
                model.BrokerContactUser = programme.BrokerContactUser;
                model.Id = Id;
                if (programme.Products != null)
                {
                    foreach (var prod in programme.Products)
                    {
                        product.Add(new ProductInfoViewModel()
                        {
                            Id = prod.Id,
                            Name = prod.Name

                        });

                    }
                }
                model.Product = product;

                ViewBag.Title = "Add/Edit Programme Email Template";
                ViewBag.RuleType = RuleType;

                return View("ProgrammeRules", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }



        [HttpGet]
        public async Task<IActionResult> EditProgramme(Guid Id)
        {            
            User user = null;

            try
            {
                ProgrammeInfoViewModel model = await GetProgrammeInfoViewModel(Id);

                return View("EditProgramme", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageProgramme()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateProgramme()
        {
            ProgrammeInfoViewModel model = await GetProgrammeInfoViewModel(Guid.Empty);
            model.ProductViewModel = await GetProductViewModel();
            model.InformationBuilderViewModel = await GetInformationBuilderViewModel();
            return View(model);
        }

        private async Task<ProgrammeInfoViewModel> GetProgrammeInfoViewModel(Guid id)
        {            
            var brokers = await _userService.GetBrokerUsers();
            if(id == Guid.Empty)
            {
                ProgrammeInfoViewModel model = new ProgrammeInfoViewModel(brokers, null, null);
                return model;
            }
            else
            {
                Programme programme = await _programmeService.GetProgrammeById(id);
                
                ProgrammeInfoViewModel model = new ProgrammeInfoViewModel(brokers, programme, null);
                
                return model;
            }

        }

        private async Task<InformationBuilderViewModel> GetInformationBuilderViewModel()
        {
            InformationBuilderViewModel model = new InformationBuilderViewModel();
            model.InformationTemplates = await _informationService.GetAllTemplates();
            model.Rules = await _ruleService.GetAllRules();
            return model;
        }

        private async Task<ProductViewModel> GetProductViewModel()
        {
            User user = null;

            user = await CurrentUser();
            ProductViewModel model = new ProductViewModel();
            model.Description = new ProductDescriptionVM
            {
                CreatorOrganisation = user.PrimaryOrganisation.Id,
                OwnerOrganisation = user.PrimaryOrganisation.Id,
                // TODO - load this from db
                Languages = new List<SelectListItem> {
                    new SelectListItem { Text = "English (NZ)", Value = "nz" },
                    new SelectListItem { Text = "English (US)", Value = "uk" },
                    new SelectListItem { Text = "English (UK)", Value = "us" },
                    new SelectListItem { Text = "German", Value = "de" },
                    new SelectListItem { Text = "French", Value = "fr" },
                    new SelectListItem { Text = "Chinese", Value = "cn" }
                },
                BaseProducts = new List<SelectListItem> { new SelectListItem { Text = "Select Base Product", Value = "" } }
            };

            model.Description.BaseProducts.Add(new SelectListItem { Text = "Set as base product", Value = Guid.Empty.ToString() });

            var productList = await _productService.GetAllProducts();
            foreach (Product product in productList.Where(p => p.IsMasterProduct))
            {
                model.Description.BaseProducts.Add(new SelectListItem { Text = product.Name, Value = product.Id.ToString() });
            }

            var riskList = await _riskCategoryService.GetAllRiskCategories();
            foreach (RiskCategory risk in riskList)
                model.Risks.Add(new RiskEntityViewModel { Insured = risk.Name, Id = risk.Id, CoverAll = false, CoverLoss = false, CoverInterruption = false, CoverThirdParty = false });

            // set product settings
            foreach (Document doc in _documentRepository.FindAll().Where(d => d.OwnerOrganisation == user.PrimaryOrganisation))
                model.Settings.Documents.Add(new SelectListItem { Text = doc.Name, Value = doc.Id.ToString() });

            var templates = await _informationService.GetAllTemplates();
            foreach (var template in templates)
                model.Settings.InformationSheets.Add(
                    new SelectListItem
                    {
                        Text = template.Name,
                        Value = template.Id.ToString()
                    }
                    );

            model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = "Select Product Owner", Value = "" });
            model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = user.PrimaryOrganisation.Name, Value = user.PrimaryOrganisation.Id.ToString() });

            return model;
        }
        

        [HttpPost]
        public async Task<IActionResult> CreateProgramme(IFormCollection collection)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = new Programme(user);
                programme.Name = collection["programmeName"];
                programme.TaxRate = decimal.Parse(collection["TaxRate"]);
                programme.PolicyNumberPrefixString = collection["PolicyNumberPrefixString"];
                programme.PolicyNumberPrefixString = collection["Declaration"];
                programme.PolicyNumberPrefixString = collection["StopAgreementMessage"];
                programme.PolicyNumberPrefixString = collection["NoPaymentRequiredMessage"];               
                programme.LastModifiedBy = user;
                programme.LastModifiedOn = DateTime.UtcNow;

                await _programmeService.Update(programme);

                return NoContent();
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return BadRequest();
            }                
        }

        [HttpPost]
        public async Task<IActionResult> EditProgramme(IFormCollection collection)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                User BrokerUser = null;
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(collection["Id"]));
                if(Guid.TryParse(collection["BrokerContactUser"], out Guid BrokerId))
                {
                    BrokerUser = await _userService.GetUserById(BrokerId);
                }
                var currentClaim = programme.Claim;
                Programme jsonProgramme = (Programme) await _serializerationService.GetDeserializedObject(typeof(Programme), collection);
                programme = await _programmeService.PostProgramme(user, BrokerUser, jsonProgramme, programme);
                if (string.IsNullOrEmpty(programme.Claim))
                {
                    if (!string.IsNullOrEmpty(currentClaim))
                    {
                        await _claimService.RemoveClaim(currentClaim);
                    }
                }
                else
                {                    
                    await _claimService.AddClaim(new Claim(programme.Claim, programme.Claim));
                }              

                return Redirect("/Programme/TermSheetConfirguration/" + programme.Id);
            }
            catch (Exception ex) 
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> getselectedParty(Guid informationId, string title)
        {
            List<string> userEmail = new List<string>();
            PartyUserViewModel model = new PartyUserViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(informationId);
                IList<User> users = null;

                if (title == "Manage UIS Issue Notification Users")
                {
                    users = programme.UISIssueNotifyUsers;
                }
                else if (title == "Manage UIS Submission Notification Users")
                {
                    users = programme.UISSubmissionNotifyUsers;
                }
                else if (title == "Manage Agreement Refer Notification Users")
                {
                    users = programme.AgreementReferNotifyUsers;
                }
                else if (title == "Manage Agreement Issue Notification Users")
                {
                    users = programme.AgreementIssueNotifyUsers;
                }
                else if (title == "Manage Agreement Bound Notification Users")
                {
                    users = programme.AgreementBoundNotifyUsers;
                }



                foreach (var selecteduser in users)
                {
                    userEmail.Add(selecteduser.Email);
                }

                return Json(userEmail);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> selectedParty(Guid selectedParty, Guid informationId)
        {
            List<PartyUserViewModel> userPartyList = new List<PartyUserViewModel>();
            PartyUserViewModel model = new PartyUserViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(informationId);
                Organisation organisation = await _organisationService.GetOrganisation(selectedParty);
               
                if ("organisation" != null)
                {
                    var userList = await _userService.GetAllUserByOrganisation(organisation);
                    foreach (var userOrg in userList)
                    {
                        userPartyList.Add(new PartyUserViewModel()
                        {
                            Name = userOrg.FullName,
                            Id = userOrg.Id.ToString(),
                            Email = userOrg.Email,
                        });
                    }
                }

                return Json(userPartyList);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddselectedParty(IFormCollection collection)
        {            
            User user = null;
            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(Guid.Parse(collection["Id"]));
                string userType = "";
                var title = collection["Name"];
                var selectedParty = collection["selectedEmail"];
                var PartyName = collection["selectedparty"];
                if (programme != null)
                {
                    using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (title == "Manage UIS Issue Notification Users")
                        {
                            programme.UISIssueNotifyUsers.Clear();
                        }
                        else if (title == "Manage UIS Submission Notification Users")
                        {
                            programme.UISSubmissionNotifyUsers.Clear();
                        }
                        else if (title == "Manage Agreement Refer Notification Users")
                        {
                            programme.AgreementReferNotifyUsers.Clear();
                        }
                        else if (title == "Manage Agreement Issue Notification Users")
                        {
                            programme.AgreementIssueNotifyUsers.Clear();
                        }
                        else if (title == "Manage Agreement Bound Notification Users")
                        {
                            programme.AgreementBoundNotifyUsers.Clear();
                        }

                        foreach (var party in selectedParty)
                        {
                            var userParty = await _userService.GetUserByEmail(party);
                            if (title == "Manage UIS Issue Notification Users")
                            {
                                if (!programme.UISIssueNotifyUsers.Contains(userParty))
                                {
                                    programme.UISIssueNotifyUsers.Add(userParty);
                                }
                            }
                            else if (title == "Manage UIS Submission Notification Users")
                            {
                                if (!programme.UISSubmissionNotifyUsers.Contains(userParty))
                                {
                                    programme.UISSubmissionNotifyUsers.Add(userParty);
                                }
                            }
                            else if (title == "Manage Agreement Refer Notification Users")
                            {
                                if (!programme.AgreementReferNotifyUsers.Contains(userParty))
                                {
                                    programme.AgreementReferNotifyUsers.Add(userParty);
                                }
                            }
                            else if (title == "Manage Agreement Issue Notification Users")
                            {
                                if (!programme.AgreementIssueNotifyUsers.Contains(userParty))
                                {
                                    programme.AgreementIssueNotifyUsers.Add(userParty);
                                }

                            }
                            else if (title == "Manage Agreement Bound Notification Users")
                            {
                                if (!programme.AgreementBoundNotifyUsers.Contains(userParty))
                                {
                                    programme.AgreementBoundNotifyUsers.Add(userParty);
                                }
                            }

                        }
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


        [HttpGet]
        public async Task<IActionResult> IssueNotification(Guid Id, String Title)
        {
            var orguser = new List<string>();
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(Id);
                model.Id = Id;
                model.Programme = programme;
                model = new ProgrammeInfoViewModel(null, programme, null);
                List<SelectListItem> usrlist = new List<SelectListItem>();
                model.Name = Title;

                //foreach(var org in programme.ClientProgrammes)
                //{

                //    List<User> userList = await _userService.GetAllUserByOrganisation(org.Owner);

                //    foreach (var userOrg in userList)
                //    {
                //        usrlist.Add(new SelectListItem()
                //        {
                //            Selected = false,
                //            Text = userOrg.FullName,
                //            Value = userOrg.Email,
                //        });
                //    }
                //}
                //model.OrgUser = usrlist;

                ViewBag.Title = "Add/Edit Programme Email Template";

                return View("IssueNotification", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> SendEmailTemplates(Guid Id, string type, string description)
        {
            EmailTemplateViewModel model = new EmailTemplateViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(Id);
                EmailTemplate emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == type);

                model.Description = description;
                model.BaseProgrammeID = Id;
                model.Type = type;

                if (emailTemplate != null)
                {
                    model.Name = emailTemplate.Name;
                    model.Subject = emailTemplate.Subject;
                    model.Body = System.Net.WebUtility.HtmlDecode(emailTemplate.Body);

                }
                else
                {
                    model.Name = "";
                    model.Subject = "";
                    model.Body = "";
                }

                ViewBag.Title = "Add/Edit Programme Email Template";

                return View("SendEmailTemplates", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmailTemplates(EmailTemplateViewModel model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                Programme programme = await _programmeService.GetProgrammeById(model.BaseProgrammeID);
                EmailTemplate emailTemplate = programme.EmailTemplates.FirstOrDefault(et => et.Type == model.Type);
                string emailtemplatename = null;

                switch (model.Type)
                {
                    case "SendSchemeEmail":
                        {
                            emailtemplatename = "Scheme Email";
                            break;
                        }
                    case "SendInformationSheetInstruction":
                        {
                            emailtemplatename = "Information Sheet Instruction";
                            break;
                        }
                    case "SendInformationSheetInstructionRenew":
                        {
                            emailtemplatename = "Information Sheet Instruction for Renew";
                            break;
                        }
                    case "SendSubInformationSheetInstruction":
                        {
                            emailtemplatename = "SubInformation Sheet Instruction";
                            break;
                        }
                    case "SendSubInformationSheetCompletion":
                        {
                            emailtemplatename = "SubInformation Sheet Completion";
                            break;
                        }
                    case "SendInformationSheetReminder":
                        {
                            emailtemplatename = "Information Sheet Reminder";
                            break;
                        }
                    case "SendInformationSheetRenewalInstruction":
                        {
                            emailtemplatename = "Information Sheet Instructions For Renewals";
                            break;
                        }
                    case "SendPolicyDocuments":
                        {
                            emailtemplatename = "Agreement Policy Documents Covering Text";
                            break;
                        }
                    case "SendQuoteDocuments":
                        {
                            emailtemplatename = "Agreement Quote Documents Covering Text";
                            break;
                        }
                    case "SendAgreementOnlineAcceptanceInstructions":
                        {
                            emailtemplatename = "Agreement Online Acceptance Instructions";
                            break;
                        }
                    case "ResendPolicyDocuments":
                        {
                            emailtemplatename = "Agreement Policy Documents Resend Covering Text";
                            break;
                        }
                    case "SendAgreementAcceptanceConfirmation":
                        {
                            emailtemplatename = "Agreement Acceptance Confirmation";
                            break;
                        }
                    case "SendOnlinePaymentInstructions":
                        {
                            emailtemplatename = "Online Payment Instructions";
                            break;
                        }
                    case "SendPDFReport":
                        {
                            emailtemplatename = "PDF Report";
                            break;
                        }
                    case "SendAdviceAdvisorRemoval":
                        {
                            emailtemplatename = "Advice Of Removal Of An Advisor From Policy";
                            break;
                        }
                    case "SendAdviceAdvisorAddition":
                        {
                            emailtemplatename = "Advice Of Addition Of An Advisor From Policy";
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Invalid Email Template Type for Programme ID: ", model.BaseProgrammeID));
                        }
                }

                if (emailTemplate != null)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        emailTemplate.Subject = model.Subject;
                        emailTemplate.Body = model.Body;
                        emailTemplate.LastModifiedBy = user;
                        emailTemplate.LastModifiedOn = DateTime.UtcNow;
                        await uow.Commit();
                    }
                }
                else
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        emailTemplate = new EmailTemplate(user, emailtemplatename, model.Type, model.Subject, model.Body, null, programme);
                        programme.EmailTemplates.Add(emailTemplate);
                        await uow.Commit();
                    }
                }

                return RedirectToAction("SendEmailTemplates", new { Id = programme.Id, type = model.Type, description = model.Description });
            }
            catch (Exception ex)
            {                
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }


        [HttpPost]
        public async Task<IActionResult> PostPaymentOptionAPI(IFormCollection model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (Guid.TryParse(model["Id"], out Guid Id))
                {
                    ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(Id);
                    clientProgramme.PaymentType = model["PaymentType"];
                    await _programmeService.Update(clientProgramme);
                    if (clientProgramme.PaymentType == "Hunter Premium Funding")
                    {
                        await _emailService.EmailHunterPremiumFunding(clientProgramme);
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
        public async Task<IActionResult> PostPaymentFrequencyAPI(IFormCollection model)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (Guid.TryParse(model["Id"], out Guid Id))
                {
                    ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(Id);
                    clientProgramme.PaymentFrequency = model["PaymentFrequency"];
                    await _programmeService.Update(clientProgramme);
                    await _emailService.EmailPaymentFrequency(clientProgramme);
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }
    }
}
