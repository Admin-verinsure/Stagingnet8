using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Payment.EGlobalAPI;
using DealEngine.Infrastructure.Payment.PxpayAPI;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Helpers;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.Agreement;
using DealEngine.WebUI.Models.Programme;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using NReco.PdfGenerator;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Document = DealEngine.Domain.Entities.Document;
using SystemDocument = DealEngine.Domain.Entities.Document;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class AgreementController : BaseController
    {
        #region Interfaces
        IUWMService _underwritingModule;
        ISubsystemService _subsystemService;
        IActivityService _activityService;
        IInformationTemplateService _informationService;
        IClientInformationService _customerInformationService;
        IPaymentGatewayService _paymentGatewayService;
        IPaymentService _paymentService;
        IMerchantService _merchantService;
        IClientAgreementTermService _clientAgreementTermService;
        IMilestoneService _milestoneService;
        IAdvisoryService _advisoryService;
        ITaskingService _taskingService;
        IHttpClientService _httpClientService;
        IProductService _productService;
        IAppSettingService _appSettingService;
        IClientAgreementService _clientAgreementService;
        IClientAgreementRuleService _clientAgreementRuleService;
        IClientAgreementEndorsementService _clientAgreementEndorsementService;
        IFileService _fileService;
        IDataService _dataService;
        IEmailService _emailService;
        IOrganisationService _organisationService;
        IProgrammeService _programmeService;
        IUnitOfWork _unitOfWork;
        IInsuranceAttributeService _insuranceAttributeService;
        IEGlobalSubmissionService _eGlobalSubmissionService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<AgreementController> _logger;
        IClientAgreementTermCanService _clientAgreementTermCanService;
        IClientAgreementBVTermCanService _clientAgreementBVTermCanService;
        ISerializerationService _serializationService;

        //convert to service?
        IMapperSession<Rule> _ruleRepository;
        IMapperSession<SystemDocument> _documentRepository;
        #endregion

        public AgreementController(
            IUWMService underwritingModule,
            ISubsystemService subsystemService,
            ILogger<AgreementController> logger,
            IApplicationLoggingService applicationLoggingService,
            IUserService userRepository,
            IUnitOfWork unitOfWork,
            IMilestoneService milestoneService,
            IInformationTemplateService informationService,
            IClientInformationService customerInformationService,
            IProductService productService,
            IClientAgreementService clientAgreementService,
            IClientAgreementRuleService clientAgreementRuleService,
            IAdvisoryService advisoryService,
            IClientAgreementEndorsementService clientAgreementEndorsementService,
            IFileService fileService,
            IDataService dataService,
            IHttpClientService httpClientService,
            ITaskingService taskingService,
            IActivityService activityService,
            IOrganisationService organisationService,
            IMapperSession<Rule> ruleRepository,
            IEmailService emailService,
            IMapperSession<SystemDocument> documentRepository,
            IProgrammeService programmeService,
            ISerializerationService serializerationService,
            IPaymentGatewayService paymentGatewayService,
            IInsuranceAttributeService insuranceAttributeService,
            IPaymentService paymentService,
            IMerchantService merchantService,
            IClientAgreementTermService clientAgreementTermService,
            IAppSettingService appSettingService,
            IEGlobalSubmissionService eGlobalSubmissionService,
            IClientAgreementTermCanService clientAgreementTermCanService,
            IClientAgreementBVTermCanService clientAgreementBVTermCanService
            )
            : base(userRepository)
        {
            _underwritingModule = underwritingModule;
            _subsystemService = subsystemService;
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;
            _activityService = activityService;
            _advisoryService = advisoryService;
            _taskingService = taskingService;
            _informationService = informationService;
            _customerInformationService = customerInformationService;
            _milestoneService = milestoneService;
            _organisationService = organisationService;
            _httpClientService = httpClientService;
            _productService = productService;
            _clientAgreementService = clientAgreementService;
            _clientAgreementRuleService = clientAgreementRuleService;
            _clientAgreementEndorsementService = clientAgreementEndorsementService;
            _fileService = fileService;
            _dataService = dataService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _ruleRepository = ruleRepository;
            _documentRepository = documentRepository;
            _paymentGatewayService = paymentGatewayService;
            _paymentService = paymentService;
            _merchantService = merchantService;
            _clientAgreementTermService = clientAgreementTermService;
            _insuranceAttributeService = insuranceAttributeService;
            _programmeService = programmeService;
            _appSettingService = appSettingService;
            _eGlobalSubmissionService = eGlobalSubmissionService;
            _clientAgreementTermCanService = clientAgreementTermCanService;
            _clientAgreementBVTermCanService = clientAgreementBVTermCanService;
            _serializationService = serializerationService;

            ViewBag.Title = "Wellness and Health Associated Professionals Agreement";
        }

        [HttpGet]
        public async Task<IActionResult> ReRunUWM(Guid id)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    _underwritingModule.UWM(user, agreement.ClientInformationSheet, agreement.ClientInformationSheet.ReferenceId);

                    string auditLogDetail = "Underwriting Module Run by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit();
                }
                //return RedirectPermanent("AcceptAgreement?Id=" + ClientId);
                return Redirect("/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        public async Task<IActionResult> AgreementTemplates()
        {
            return View();
        }

        public async Task<IActionResult> AgreementBuilder()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                var templates = await _informationService.GetAllTemplates();
                var products = await _productService.GetAllProducts();

                AgreementTemplateViewModel model = new AgreementTemplateViewModel();

                List<string> allLanguages = new List<string>();
                foreach (var product in products)
                    allLanguages.AddRange(product.Languages);

                model.Languages = new List<SelectListItem>();
                foreach (var language in allLanguages.Distinct())
                    model.Languages.Add(
                        new SelectListItem
                        {
                            Text = language,
                            Value = language
                        }
                    );

                model.InformationSheets = new List<SelectListItem>();
                foreach (var template in templates)
                    model.InformationSheets.Add(
                        new SelectListItem
                        {
                            Text = template.Name,
                            Value = template.Id.ToString()
                        }
                    );

                model.Products = new List<SelectListItem>();
                foreach (var product in products)
                    model.Products.Add(
                        new SelectListItem
                        {
                            Text = product.Name,
                            Value = product.Id.ToString()
                        }
                    );

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> AuthoriseReferrals(Guid sheetId, Guid informationsheet, Guid agreementId)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            model.Referrals = new List<ClientAgreementReferral>();
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(agreementId);
                model.InformationSheetId = sheetId;
                model.ClientAgreementId = agreementId;
                model.ClientProgrammeId = agreement.ClientInformationSheet.Programme.Id;
                model.ProductName = agreement.Product.Name;
                //ClientAgreementTerm clientAgreementTerm = _clientAgreementTermService.GetAllClientAgreementTerm

                foreach (var term in await _clientAgreementTermService.GetListAgreementTermFor(agreement))
                {
                    if (term.Bound)
                    {
                        model.SelectedPremium = term.Premium;
                        model.BasePremium = term.BasePremium;
                    }

                }


                foreach (var terms in agreement.ClientAgreementReferrals)
                {
                    model.Referrals.Add(terms);
                }

                model.ReferralLoading = agreement.ClientAgreementTerms.FirstOrDefault().ReferralLoading;
                model.ReferralAmount = agreement.ClientAgreementTerms.FirstOrDefault().ReferralLoadingAmount;
                model.AuthorisationNotes = agreement.ClientAgreementTerms.FirstOrDefault().AuthorisationNotes;

                ViewBag.Title = "Agreement Referrals ";

                return View("AuthoriseReferrals", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> IssuetoBroker(Guid programmeId, Guid agreementId)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            model.Referrals = new List<ClientAgreementReferral>();
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(agreementId);
                ClientProgramme clientprog = await _programmeService.GetClientProgrammebyId(programmeId);
                Programme programme = clientprog.BaseProgramme;
                //model.InformationSheetId = sheetId;
                model.ClientAgreementId = agreementId;
                model.ClientProgrammeId = agreement.ClientInformationSheet.Programme.Id;
                model.ProgrammeName = programme.Name;
                model.ProgrammeNamedPartyName = programme.NamedPartyUnitName;

                if (!string.IsNullOrEmpty(agreement.issuetobrokercomment))
                {
                    model.issuetobrokercomment = agreement.issuetobrokercomment;
                    model.issuetobrokerby = agreement.issuetobrokerby;
                    model.SelectedBroker = agreement.SelectedBroker;
                    model.IssuedToBroker = agreement.IssuedToBroker;
                }

                var org = programme.BrokerContactUser.PrimaryOrganisation;

                List<SelectListItem> usrlist = new List<SelectListItem>();
                List<User> userList = await _userService.GetAllUserByOrganisation(org);

                foreach (var userOrg in userList)
                {
                    usrlist.Add(new SelectListItem()
                    {
                        Selected = false,
                        Text = userOrg.FullName,
                        Value = userOrg.Email,
                    });
                }
                usrlist.Add(new SelectListItem()
                {
                    Selected = true,
                    Text = programme.BrokerContactUser.FullName,
                    Value = programme.BrokerContactUser.Email,
                });

                model.UserList = usrlist;

                ViewBag.Title = "Issue - To Broker ";

                return View("ReferBacktoBroker", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> IssuetoBroker(ViewAgreementViewModel model)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementId);
                //var clientAgreementendorsement = await _clientAgreementEndorsementService.GetClientAgreementEndorsementBy(model.ClientAgreementEndorsementID);
                ClientProgramme clientProgramme = await _programmeService.GetClientProgramme(model.ClientProgrammeId);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (model.Content != null)
                    {
                        agreement.IssuedToBroker = DateTime.UtcNow;
                        agreement.issuetobrokerby = user.FullName;
                        agreement.Status = "Quoted";
                        agreement.SelectedBroker = await _userService.GetUserByEmail(model.issuetobrokerto);
                        agreement.issuetobrokercomment = model.Content;
                    }
                    await uow.Commit();
                }
                if (model.Content != null && agreement.ClientInformationSheet.Programme.BaseProgramme.ProgEnableEmail)
                {
                    await _emailService.IssueToBrokerSendEmail(model.issuetobrokerto, model.Content, agreement.ClientInformationSheet, agreement, user);
                }
                return RedirectToAction("ViewAcceptedAgreement", new { id = model.ClientProgrammeId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateAdjustment(decimal AdjustmentAmount, Guid AgreementId)
        {
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(AgreementId);
                var sheet = agreement.ClientInformationSheet;
                user = await CurrentUser();
                var premium = 0.0m;
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var term in await _clientAgreementTermService.GetListAgreementTermFor(agreement))
                    {
                        term.Premium = term.Premium + AdjustmentAmount;
                    }
                    await uow.Commit();
                }

                //var url = "/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id;
                return Json("AdjustmentDone");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }




        [HttpPost]
        public async Task<IActionResult> AuthorisedReferral(AgreementViewModel clientAgreementModel)
        {
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementModel.AgreementId);
                var sheet = agreement.ClientInformationSheet;
                user = await CurrentUser();
                var premium = 0.0m;
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    foreach (var terms in agreement.ClientAgreementReferrals.Where(r => r.Status == "Pending"))
                    {
                        terms.Status = "Cleared";
                        terms.AuthorisedBy = user;
                        terms.Authorised = DateTime.UtcNow;
                    }

                    foreach (var terms in agreement.ClientAgreementTerms)
                    {
                        if (terms.BoatTerms.Any())
                        {
                            foreach (var bvterm in terms.BoatTerms)
                            {
                                premium = premium + bvterm.Premium;
                            }
                        }
                        if (terms.MotorTerms.Any())
                        {
                            foreach (var mvterm in terms.MotorTerms)
                            {
                                premium = premium + mvterm.Premium;
                            }
                        }
                    }

                    foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                    {
                        term.ReferralLoading = clientAgreementModel.RefferLodPrc;
                        term.ReferralLoadingAmount = clientAgreementModel.RefferLodAmt;
                        term.AuthorisationNotes = clientAgreementModel.AdditionalNotes;
                        if (term.MotorTerms.Count() == 0 && term.BoatTerms.Count() == 0)
                        {
                            term.Premium = term.Premium * (1 + clientAgreementModel.RefferLodPrc / 100) + clientAgreementModel.RefferLodAmt;
                            term.LastModifiedBy = user;
                            term.LastModifiedOn = DateTime.Now;
                        }
                        else
                        {
                            term.Premium = premium * (1 + clientAgreementModel.RefferLodPrc / 100) + clientAgreementModel.RefferLodAmt;
                            term.LastModifiedBy = user;
                            term.LastModifiedOn = DateTime.Now;
                        }

                    }

                    foreach (var terms in agreement.ClientAgreementTerms)
                    {
                        //foreach (var bvterm in terms.BoatTerms)
                        //{ Org Changes

                        //    if (bvterm.Boat.BoatWaterLocation != null)
                        //    {
                        //        var orgList = await _organisationService.GetAllOrganisations();
                        //        InsuranceAttribute insuranceAttribute = await _insuranceAttributeService.GetInsuranceAttributeByName("Other Marina");
                        //        if (insuranceAttribute != null)
                        //        {

                        //            orgList.Where(o => o.IsApproved == false && o.InsuranceAttributes.Contains(insuranceAttribute)).ToList();
                        //            foreach (var org in orgList)
                        //            {
                        //                InsuranceAttribute insuranceAttribute1 = await _insuranceAttributeService.GetInsuranceAttributeByName(org.Name);
                        //                if (insuranceAttribute.InsuranceAttributeName == "Other Marina")
                        //                {

                        //                    org.IsApproved = true;
                        //                }
                        //            }
                        //            //Organisation othermarine = await _OrganisationRepository.GetByIdAsync(bvterm.Boat.BoatWaterLocation.Id);
                        //        }

                        //    }

                        //}
                    }

                    if (agreement.Status == "Referred")
                    {
                        agreement.Status = "Authorised";
                        await _milestoneService.CompleteMilestoneFor("Agreement Status – Referred", user, sheet);
                    }

                    string auditLogDetail = "Agreement Referrals have been authorised by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);


                    await uow.Commit();

                }

                var url = "/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id;
                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> CancellAgreement(Guid id)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;
                ClientProgramme programme = answerSheet.Programme;
                var insuranceRoles = new List<InsuranceRoleViewModel>();
                insuranceRoles.Add(new InsuranceRoleViewModel { RoleName = "Client", Name = insured.Name, ManagedBy = "", Email = "" });

                model.InformationSheetId = answerSheet.Id;
                model.ClientAgreementId = agreement.Id;
                model.ClientProgrammeId = programme.Id;
                model.InsuranceRoles = insuranceRoles;
                model.CancellNotes = agreement.CancelledNote;
                model.StartDate = LocalizeTimeDate(agreement.InceptionDate, "dd-mm-yyyy");
                model.EndDate = LocalizeTimeDate(agreement.ExpiryDate, "dd-mm-yyyy");
                model.CancellEffectiveDate = agreement.CancelledEffectiveDate;
                model.CancelAgreementReason = agreement.CancelAgreementReason;


                foreach (var terms in agreement.ClientAgreementTerms)
                {
                    if (terms.BoatTerms.Where(bvt => bvt.DateDeleted == null).Count() > 0)
                    {
                        var boats = new List<EditTermsViewModel>();
                        foreach (var boat in terms.BoatTerms)
                        {
                            boats.Add(new EditTermsViewModel
                            {
                                VesselId = boat.Id,
                                BoatName = boat.BoatName,
                                BoatMake = boat.BoatMake,
                                BoatModel = boat.BoatModel,
                                TermLimit = boat.TermLimit,
                                Excess = Convert.ToInt32(boat.Excess),
                                Premium = boat.Premium,
                                FSL = boat.FSL
                            });
                        }
                        model.BVTerms = boats;
                    }

                    if (terms.MotorTerms.Where(mvt => mvt.DateDeleted == null).Count() > 0)
                    {
                        var motors = new List<EditTermsViewModel>();
                        foreach (var motor in terms.MotorTerms)
                        {
                            motors.Add(new EditTermsViewModel
                            {
                                VesselId = motor.Id,
                                Registration = motor.Registration,
                                Make = motor.Make,
                                Model = motor.Model,
                                TermLimit = motor.TermLimit,
                                Excess = Convert.ToInt32(motor.Excess),
                                Premium = motor.Premium,
                                FSL = motor.FSL
                            });
                        }
                        model.MVTerms = motors;
                    }
                }

                if (agreement.ClientAgreementTermsCancel != null)
                {
                    foreach (var termsCan in agreement.ClientAgreementTermsCancel)
                    {
                        if (termsCan.BoatTermsCan.Where(bvtCan => bvtCan.DateDeleted == null).Count() > 0)
                        {
                            var boatsCan = new List<EditTermsCancelViewModel>();
                            foreach (var boatCan in termsCan.BoatTermsCan)
                            {
                                boatsCan.Add(new EditTermsCancelViewModel
                                {
                                    VesselCanId = boatCan.Id,
                                    BoatNameCan = boatCan.BoatNameCan,
                                    BoatMakeCan = boatCan.BoatMakeCan,
                                    BoatModelCan = boatCan.BoatModelCan,
                                    TermLimitCan = boatCan.TermLimitCan,
                                    ExcessCan = Convert.ToInt32(boatCan.ExcessCan),
                                    PremiumCan = boatCan.PremiumCan,
                                    FSLCan = boatCan.FSLCan
                                });
                            }
                            model.BVTermsCan = boatsCan;
                        }

                        if (termsCan.MotorTermsCan.Where(mvtCan => mvtCan.DateDeleted == null).Count() > 0)
                        {
                            var motorsCan = new List<EditTermsCancelViewModel>();
                            foreach (var motorCan in termsCan.MotorTermsCan)
                            {
                                motorsCan.Add(new EditTermsCancelViewModel
                                {
                                    VesselCanId = motorCan.Id,
                                    RegistrationCan = motorCan.RegistrationCan,
                                    MakeCan = motorCan.MakeCan,
                                    ModelCan = motorCan.ModelCan,
                                    TermLimitCan = motorCan.TermLimitCan,
                                    ExcessCan = Convert.ToInt32(motorCan.ExcessCan),
                                    PremiumCan = motorCan.PremiumCan,
                                    FSLCan = motorCan.FSLCan
                                });
                            }
                            model.MVTermsCan = motorsCan;
                        }
                    }
                }


                ViewBag.Title = "Cancel Agreement ";

                return View("CancellAgreement", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CancellAgreement(AgreementViewModel clientAgreementModel)
        {
            User user = null;
            var url = "";
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementModel.AgreementId);
                user = await CurrentUser();

                if (agreement.ClientInformationSheet.Programme.BaseProgramme.CalculateCancelTerm)
                {
                    ClientAgreementTerm excaterm = agreement.ClientAgreementTerms.FirstOrDefault(cat => cat.SubTermType == "BV" && cat.DateDeleted == null);
                    if (excaterm != null)
                    {
                        if (agreement.ClientAgreementTermsCancel.FirstOrDefault(acatcan => acatcan.DateDeleted == null && acatcan.exClientAgreementTerm == excaterm) == null)
                        {
                            await _clientAgreementTermCanService.AddAgreementTermCan(user, excaterm.TermLimit, excaterm.Excess, 0m, 0m, excaterm.BrokerageRate, 0m, agreement, "BV");
                        }
                        ClientAgreementTermCancel catermcan = agreement.ClientAgreementTermsCancel.FirstOrDefault(catCancel => catCancel.SubTermTypeCan == "BV" && catCancel.DateDeleted == null);
                        catermcan.exClientAgreementTerm = excaterm;
                        catermcan.LastModifiedBy = user;
                        catermcan.LastModifiedOn = DateTime.UtcNow;

                        var excaBVTerms = excaterm.BoatTerms;
                        var excaMVTerms = excaterm.MotorTerms;

                        int expolicyperiodindaysCan = 0;
                        decimal totalBoatFslCan = 0m;
                        decimal totalBoatPremiumCan = 0m;
                        decimal totalBoatBrokerageCan = 0m;
                        decimal totalVehicleFslCan = 0m;
                        decimal totalVehiclePremiumCan = 0m;
                        decimal totalVehicleBrokerageCan = 0m;
                        expolicyperiodindaysCan = (agreement.ExpiryDate - agreement.InceptionDate).Days;

                        if (excaBVTerms != null && catermcan != null)
                        {
                            foreach (ClientAgreementBVTerm excaBVTerm in excaBVTerms)
                            {
                                int boatperiodindaysCan = 0;
                                decimal boatproratedFslCan = 0m;
                                decimal boatproratedPremiumCan = 0m;
                                decimal boatproratedBrokerageCan = 0m;

                                //Calculate BV cancel term
                                if (clientAgreementModel.CancellEffectiveDate != null && excaBVTerm.Boat.BoatInceptionDate != null &&
                                    clientAgreementModel.CancellEffectiveDate >= excaBVTerm.Boat.BoatInceptionDate)
                                {
                                    boatperiodindaysCan = (excaBVTerm.Boat.BoatExpireDate - clientAgreementModel.CancellEffectiveDate).Days;
                                    boatproratedPremiumCan = excaBVTerm.AnnualPremium * boatperiodindaysCan / expolicyperiodindaysCan - excaBVTerm.Premium;
                                    boatproratedFslCan = excaBVTerm.AnnualFSL * boatperiodindaysCan / expolicyperiodindaysCan - excaBVTerm.FSL;
                                    boatproratedBrokerageCan = boatproratedPremiumCan * excaterm.BrokerageRate / 100;
                                }

                                totalBoatPremiumCan += boatproratedPremiumCan;
                                totalBoatFslCan += boatproratedFslCan;
                                totalBoatBrokerageCan += boatproratedBrokerageCan;

                                ClientAgreementBVTermCancel excabvtermcan = catermcan.BoatTermsCan.FirstOrDefault(acabvtcan => acabvtcan.DateDeleted == null && acabvtcan.exClientAgreementBVTerm == excaBVTerm);
                                if (excabvtermcan == null)
                                {
                                    using (var uow = _unitOfWork.BeginUnitOfWork())
                                    {
                                        ClientAgreementBVTermCancel cabvtermcan = new ClientAgreementBVTermCancel(user, excaBVTerm.BoatName, excaBVTerm.YearOfManufacture, excaBVTerm.BoatMake, excaBVTerm.BoatModel,
                                            excaBVTerm.TermLimit, excaBVTerm.Excess, boatproratedPremiumCan, boatproratedFslCan, excaBVTerm.BrokerageRate, boatproratedBrokerageCan, catermcan, excaBVTerm.Boat);
                                        cabvtermcan.TermCategoryCan = "active";
                                        cabvtermcan.AnnualPremiumCan = excaBVTerm.AnnualPremium;
                                        cabvtermcan.AnnualFSLCan = excaBVTerm.AnnualFSL;
                                        cabvtermcan.AnnualBrokerageCan = excaBVTerm.AnnualBrokerage;
                                        catermcan.BoatTermsCan.Add(cabvtermcan);
                                        cabvtermcan.exClientAgreementBVTerm = excaBVTerm;

                                        await uow.Commit().ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    excabvtermcan.AnnualPremiumCan = excaBVTerm.AnnualPremium;
                                    excabvtermcan.AnnualFSLCan = excaBVTerm.AnnualFSL;
                                    excabvtermcan.AnnualBrokerageCan = excaBVTerm.AnnualBrokerage;
                                    excabvtermcan.exClientAgreementBVTerm = excaBVTerm;
                                    excabvtermcan.PremiumCan = boatproratedPremiumCan;
                                    excabvtermcan.FSLCan = boatproratedFslCan;
                                    excabvtermcan.BrokerageCan = boatproratedBrokerageCan;
                                    excabvtermcan.LastModifiedBy = user;
                                    excabvtermcan.LastModifiedOn = DateTime.UtcNow;
                                }

                            }

                            catermcan.PremiumCan += totalBoatPremiumCan;
                            catermcan.FSLCan += totalBoatFslCan;
                            catermcan.BrokerageCan += totalBoatBrokerageCan;
                        }

                        if (excaMVTerms != null && catermcan != null)
                        {
                            foreach (ClientAgreementMVTerm excaMVTerm in excaMVTerms)
                            {
                                int vehicleperiodindaysCan = 0;
                                decimal vehicleproratedFslCan = 0m;
                                decimal vehicleproratedPremiumCan = 0m;
                                decimal vehicleproratedBrokerageCan = 0m;

                                //Calculate MV cancel term
                                if (clientAgreementModel.CancellEffectiveDate != null && excaMVTerm.Vehicle.VehicleInceptionDate != null &&
                                    clientAgreementModel.CancellEffectiveDate >= excaMVTerm.Vehicle.VehicleInceptionDate)
                                {
                                    vehicleperiodindaysCan = (excaMVTerm.Vehicle.VehicleExpireDate - clientAgreementModel.CancellEffectiveDate).Days;
                                    vehicleproratedPremiumCan = excaMVTerm.AnnualPremium * vehicleperiodindaysCan / expolicyperiodindaysCan - excaMVTerm.Premium;
                                    vehicleproratedFslCan = excaMVTerm.AnnualFSL * vehicleperiodindaysCan / expolicyperiodindaysCan - excaMVTerm.FSL;
                                    vehicleproratedBrokerageCan = vehicleproratedPremiumCan * excaterm.BrokerageRate / 100;
                                }

                                totalVehiclePremiumCan += vehicleproratedPremiumCan;
                                totalVehicleFslCan += vehicleproratedFslCan;
                                totalVehicleBrokerageCan += vehicleproratedBrokerageCan;

                                ClientAgreementMVTermCancel excamvtermcan = catermcan.MotorTermsCan.FirstOrDefault(acamvtcan => acamvtcan.DateDeleted == null && acamvtcan.exClientAgreementMVTerm == excaMVTerm);
                                if (excamvtermcan == null)
                                {
                                    using (var uow1 = _unitOfWork.BeginUnitOfWork())
                                    {
                                        ClientAgreementMVTermCancel camvtermcan = new ClientAgreementMVTermCancel(user, excaMVTerm.Registration, excaMVTerm.Year, excaMVTerm.Make, excaMVTerm.Model,
                                            excaMVTerm.TermLimit, excaMVTerm.Excess, vehicleproratedPremiumCan, vehicleproratedFslCan, excaMVTerm.BrokerageRate, vehicleproratedBrokerageCan,
                                            excaMVTerm.VehicleCategory, excaMVTerm.FleetNumber, catermcan, excaMVTerm.Vehicle, excaMVTerm.BurnerPremium);
                                        camvtermcan.TermCategoryCan = "active";
                                        camvtermcan.AnnualPremiumCan = excaMVTerm.AnnualPremium;
                                        camvtermcan.AnnualFSLCan = excaMVTerm.AnnualFSL;
                                        camvtermcan.AnnualBrokerageCan = excaMVTerm.AnnualBrokerage;
                                        catermcan.MotorTermsCan.Add(camvtermcan);
                                        camvtermcan.exClientAgreementMVTerm = excaMVTerm;

                                        await uow1.Commit().ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    excamvtermcan.AnnualPremiumCan = excaMVTerm.AnnualPremium;
                                    excamvtermcan.AnnualFSLCan = excaMVTerm.AnnualFSL;
                                    excamvtermcan.AnnualBrokerageCan = excaMVTerm.AnnualBrokerage;
                                    excamvtermcan.exClientAgreementMVTerm = excaMVTerm;
                                    excamvtermcan.PremiumCan = vehicleproratedPremiumCan;
                                    excamvtermcan.FSLCan = vehicleproratedFslCan;
                                    excamvtermcan.BrokerageCan = vehicleproratedBrokerageCan;
                                    excamvtermcan.LastModifiedBy = user;
                                    excamvtermcan.LastModifiedOn = DateTime.UtcNow;
                                }

                            }

                            catermcan.PremiumCan += totalVehiclePremiumCan;
                            catermcan.FSLCan += totalVehicleFslCan;
                            catermcan.BrokerageCan += totalVehicleBrokerageCan;
                        }

                        if ((agreement.Status != "Declined by Insurer" || agreement.Status != "Declined by Insured" || agreement.Status != "Cancelled" || agreement.Status != "Cancel Pending") &&
                                (agreement.Status == "Bound" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced"))
                        {
                            using (var uow2 = _unitOfWork.BeginUnitOfWork())
                            {
                                agreement.Status = "Cancel Pending";
                                agreement.CancelledNote = clientAgreementModel.CancellNotes;
                                agreement.CancelledEffectiveDate = clientAgreementModel.CancellEffectiveDate;
                                agreement.CancelAgreementReason = clientAgreementModel.CancelAgreementReason;
                                agreement.CancelledByUserID = user;
                                agreement.CancelledDate = DateTime.UtcNow;


                                string auditLogDetail = "Agreement has been requested to cancel by " + user.FullName;
                                AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                                agreement.ClientAgreementAuditLogs.Add(auditLog);

                                await uow2.Commit().ConfigureAwait(false);
                            }
                        }

                    }

                    url = "/Agreement/CancellAgreement/" + agreement.Id;


                }
                else
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if ((agreement.Status != "Declined by Insurer" || agreement.Status != "Declined by Insured" || agreement.Status != "Cancelled" || agreement.Status != "Cancel Pending") &&
                            (agreement.Status == "Bound" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced"))
                        {
                            agreement.Status = "Cancelled";
                            agreement.CancelledNote = clientAgreementModel.CancellNotes;
                            agreement.CancelledEffectiveDate = clientAgreementModel.CancellEffectiveDate;
                            agreement.CancelAgreementReason = clientAgreementModel.CancelAgreementReason;
                            agreement.Cancelled = true;
                            agreement.CancelledByUserID = user;
                            agreement.CancelledDate = DateTime.UtcNow;
                        }

                        string auditLogDetail = "Agreement has been cancelled by " + user.FullName;
                        AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                        agreement.ClientAgreementAuditLogs.Add(auditLog);

                        await uow.Commit().ConfigureAwait(false);

                    }

                    url = "/Agreement/CancellAgreement/" + agreement.Id;
                }

                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UnbindAgreement(IFormCollection formCollection)
        {
            User user = null;
            var url = "";
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(Guid.Parse(formCollection["ClientAgreementId"]));
                user = await CurrentUser();

                ClientProgramme programme = agreement.ClientInformationSheet.Programme;

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if ((agreement.Status != "Declined by Insurer" || agreement.Status != "Declined by Insured" || agreement.Status != "Cancelled") &&
                        (agreement.Status == "Bound" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced" || agreement.Status == "Cancel Pending"))
                    {
                        agreement.Status = "Quoted";
                        agreement.IsUnbind = true;
                        agreement.UnbindNotes = formCollection["DeclineNotes"];
                        agreement.UnbindEffectiveDate = DateTime.UtcNow;
                        agreement.UnbindByUserID = user;

                    }

                    agreement.ClientInformationSheet.Status = "Submitted";
                    string auditLogDetail = "Agreement has been confirmed Unbind by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit().ConfigureAwait(false);

                }
                return Redirect("/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id);

                //url = "/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id;

                //return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BindAgreement(Guid id)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            User user = null;
            ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
            ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
            ClientProgramme programme = answerSheet.Programme;
            try
            {
                user = await CurrentUser();
                ViewBag.Title = "Bind Agreements ";
                model.InformationSheetId = answerSheet.Id;
                model.ClientProgrammeId = programme.Id;
                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }


        [HttpPost]
        public async Task<IActionResult> ConfirmCancellAgreement(AgreementViewModel clientAgreementModel)
        {
            User user = null;
            var url = "";
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementModel.AgreementId);
                user = await CurrentUser();

                ClientProgramme programme = agreement.ClientInformationSheet.Programme;
                var eGlobalSerializer = new EGlobalSerializerAPI();

                //check Eglobal parameters
                if (string.IsNullOrEmpty(programme.EGlobalClientNumber))
                {
                    throw new Exception(nameof(programme.EGlobalClientNumber) + " EGlobal client number");
                }

                string paymentType = "";
                Guid transactionreferenceid = Guid.NewGuid();

                var xmlPayload = eGlobalSerializer.SerializePolicy(programme, user, _unitOfWork, transactionreferenceid, paymentType, false, true, null);

                var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload);

                //used for eglobal request and response log 
                if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgEnableEmail)
                {
                    await _emailService.EGlobalLogEmail("marshevents@proposalonline.com", transactionreferenceid.ToString(), xmlPayload, byteResponse);
                }

                EGlobalSubmission eglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmissionByTransaction(transactionreferenceid);

                eGlobalSerializer.DeSerializeResponse(byteResponse, programme, user, _unitOfWork, eglobalsubmission);


                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if ((agreement.Status != "Declined by Insurer" || agreement.Status != "Declined by Insured" || agreement.Status != "Cancelled") &&
                        (agreement.Status == "Bound" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced" || agreement.Status == "Cancel Pending"))
                    {
                        agreement.Status = "Cancelled";
                        agreement.Cancelled = true;
                        agreement.CancelledByUserID = user;
                        agreement.CancelledDate = DateTime.UtcNow;
                    }


                    string auditLogDetail = "Agreement has been confirmed cancel by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit().ConfigureAwait(false);

                }

                url = "/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id;

                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeclineAgreement(Guid id)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;
                ClientProgramme programme = answerSheet.Programme;

                model.InformationSheetId = answerSheet.Id;
                model.ClientAgreementId = agreement.Id;
                model.ClientProgrammeId = programme.Id;

                model.DeclineNotes = agreement.InsurerDeclinedComment;

                ViewBag.Title = "Decline Agreement ";

                return View("DeclineAgreement", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> UnbindAgreement(Guid id)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;
                ClientProgramme programme = answerSheet.Programme;

                model.InformationSheetId = answerSheet.Id;
                model.ClientAgreementId = agreement.Id;
                model.ClientProgrammeId = programme.Id;

                model.DeclineNotes = agreement.InsurerDeclinedComment;

                ViewBag.Title = "Unbind Agreement ";

                return View("UnbindAgreement", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeclineAgreement(AgreementViewModel clientAgreementModel)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementModel.AgreementId);
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (agreement.Status != "Declined by Insurer" || agreement.Status != "Declined by Insured" || agreement.Status != "Cancelled" ||
                        agreement.Status != "Bound" || agreement.Status != "Bound and invoice pending" || agreement.Status != "Bound and invoiced")

                        agreement.Status = "Declined by Insurer";
                    agreement.InsurerDeclinedComment = clientAgreementModel.DeclineNotes;
                    agreement.InsurerDeclined = true;
                    agreement.InsurerDeclinedUserID = user;
                    agreement.InsurerDeclinedDate = DateTime.UtcNow;


                    string auditLogDetail = "Agreement has been declined by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit();

                }

                var url = "/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id;
                return Json(new { url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> UndeclineAgreement(Guid id)
        {
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                user = await CurrentUser();
                if (agreement != null)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (agreement.Status == "Declined by Insurer" || agreement.Status == "Declined by Insured")
                        {
                            agreement.Status = "Quoted";
                            agreement.InsurerDeclined = false;
                            agreement.InsuredDeclined = false;
                            agreement.UndeclinedUserID = user;
                            agreement.UndeclinedDate = DateTime.UtcNow;
                        }

                        string auditLogDetail = "Agreement has been undeclined by " + user.FullName;
                        AuditLog auditLog = new AuditLog(user, agreement.ClientInformationSheet, agreement, auditLogDetail);
                        agreement.ClientAgreementAuditLogs.Add(auditLog);

                        await uow.Commit();

                    }
                }

                return Redirect("/Agreement/ViewAcceptedAgreement/" + agreement.ClientInformationSheet.Programme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendReferredEmail(EmailTemplateViewModel model)
        {
            User user = null;
            try
            {
                ClientProgramme programme = await _programmeService.GetClientProgrammebyId(model.ClientProgrammeID);
                user = await CurrentUser();
                // TODO - rewrite to save templates on a per programme basis
                ClientAgreement agreement = programme.Agreements[0];
                //EmailTemplate emailTemplate = agreement.Product.EmailTemplates.FirstOrDefault (et => et.Type == "SendPolicyDocuments");
                EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == model.Type);

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
                        emailTemplate = new EmailTemplate(user, "Agreement Documents Covering Text", "SendPolicyDocuments", model.Subject, model.Body, null, programme.BaseProgramme);
                        programme.BaseProgramme.EmailTemplates.Add(emailTemplate);

                        await uow.Commit();
                    }
                }

                var docs = agreement.GetDocuments();
                var documents = new List<SystemDocument>();

                if (docs != null)
                {
                    foreach (SystemDocument doc in docs)
                    {
                        if (doc.DateDeleted == null)
                        {
                            documents.Add(doc);
                        }
                    }
                }
                else
                {
                    documents = null;
                }

                string strrecipentemail = null;
                if (model.Recipent != null)
                {
                    var userdb = await _userService.GetUserById(model.Recipent);
                    strrecipentemail = userdb.Email;
                }

                //await _emailService.SendEmailViaEmailTemplate(strrecipentemail, emailTemplate, documents, null, null);

                return Redirect("~/Home/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> EditTerms(Guid id)
        //{
        //    User user = null;
        //    ViewAgreementViewModel model = new ViewAgreementViewModel();
        //    try
        //    {
        //        ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
        //        ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null);
        //        model.ClientAgreementId = id;
        //        model.ClientProgrammeId = agreement.ClientInformationSheet.Programme.Id;
        //        foreach (var terms in agreement.ClientAgreementTerms)
        //        {
        //            if (terms.BoatTerms.Where(bvt => bvt.DateDeleted == null).Count() > 0)
        //            {
        //                var boats = new List<EditTermsViewModel>();
        //                foreach (var boat in terms.BoatTerms)
        //                {
        //                    boats.Add(new EditTermsViewModel
        //                    {
        //                        VesselId = boat.Id,
        //                        BoatName = boat.BoatName,
        //                        BoatMake = boat.BoatMake,
        //                        BoatModel = boat.BoatModel,
        //                        TermLimit = boat.TermLimit,
        //                        Excess = Convert.ToInt32(boat.Excess),
        //                        Premium = boat.Premium,
        //                        FSL = boat.FSL
        //                    });
        //                }
        //                model.BVTerms = boats;
        //            }

        //            if (terms.MotorTerms.Where(mvt => mvt.DateDeleted == null).Count() > 0)
        //            {
        //                var motors = new List<EditTermsViewModel>();
        //                foreach (var motor in terms.MotorTerms)
        //                {
        //                    motors.Add(new EditTermsViewModel
        //                    {
        //                        VesselId = motor.Id,
        //                        Registration = motor.Registration,
        //                        Make = motor.Make,
        //                        Model = motor.Model,
        //                        TermLimit = motor.TermLimit,
        //                        Excess = Convert.ToInt32(motor.Excess),
        //                        Premium = motor.Premium,
        //                        FSL = motor.FSL
        //                    });
        //                }
        //                model.MVTerms = motors;
        //            }

        //            if (terms.MotorTerms.Where(mvt => mvt.DateDeleted == null).Count() > 0)
        //            {
        //                var motors = new List<EditTermsViewModel>();
        //                foreach (var motor in terms.MotorTerms)
        //                {
        //                    motors.Add(new EditTermsViewModel
        //                    {
        //                        VesselId = motor.Id,
        //                        Registration = motor.Registration,
        //                        Make = motor.Make,
        //                        Model = motor.Model,
        //                        TermLimit = motor.TermLimit,
        //                        Excess = Convert.ToInt32(motor.Excess),
        //                        Premium = motor.Premium,
        //                        FSL = motor.FSL
        //                    });
        //                }
        //                model.MVTerms = motors;
        //            }
        //        }
        //        var plterms = new List<EditTermsViewModel>();
        //        var edterms = new List<EditTermsViewModel>();
        //        var piterms = new List<EditTermsViewModel>();
        //        var elterms = new List<EditTermsViewModel>();
        //        var clterms = new List<EditTermsViewModel>();
        //        var slterms = new List<EditTermsViewModel>();
        //        var doterms = new List<EditTermsViewModel>();

        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "PL" && t.DateDeleted == null))
        //        {
        //            plterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });
        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "ED" && t.DateDeleted == null))
        //        {
        //            edterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });


        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "PI" && t.DateDeleted == null))
        //        {
        //            piterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });


        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "EL" && t.DateDeleted == null))
        //        {
        //            elterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });


        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "CL" && t.DateDeleted == null))
        //        {
        //            clterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });


        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "SL" && t.DateDeleted == null))
        //        {
        //            slterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });


        //        }
        //        foreach (var plterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == "DO" && t.DateDeleted == null))
        //        {
        //            doterms.Add(new EditTermsViewModel
        //            {
        //                TermId = plterm.Id,
        //                TermType = plterm.SubTermType,
        //                TermLimit = plterm.TermLimit,
        //                Excess = Convert.ToInt32(plterm.Excess),
        //                Premium = plterm.Premium,
        //                BasePremium = plterm.BasePremium,
        //                PremiumDiffer = plterm.PremiumDiffer
        //            });
        //        }
        //        model.PLTerms = plterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.EDTerms = edterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.PITerms = piterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.ELTerms = elterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.CLTerms = clterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.SLTerms = slterms.OrderBy(acat => acat.TermLimit).ToList();
        //        model.DOTerms = doterms.OrderBy(acat => acat.TermLimit).ToList();
        //        ViewBag.Title = "Edit Terms ";

        //        return View("EditTerms", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> EditExtensionTerms(Guid id, String productname = null)
        {
            User user = null;
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                model.ClientAgreementId = id;
                model.ClientProgrammeId = agreement.ClientInformationSheet.Programme.Id;
                var subtypeterms = new List<EditExtensionTermsViewModel>();

                foreach (ClientAgreementTermExtension subtypeterm in agreement.ClientAgreementTermExtensions.Where(t => t.DateDeleted == null))
                {
                    subtypeterms.Add(new EditExtensionTermsViewModel
                    {
                        TermId = subtypeterm.Id,
                        TermLimit = subtypeterm.TermLimit,
                        Excess = Convert.ToInt32(subtypeterm.Excess),
                        Premium = subtypeterm.Premium,

                        //BasePremium = subtypeterm.BasePremium,
                        //PremiumDiffer = subtypeterm.PremiumDiffer
                    });
                }

                model.ExtensionTerms = subtypeterms.OrderBy(acat => acat.TermLimit).ToList();
                model.ProductName = productname;
                ViewBag.Title = "Edit Extension Terms ";

                return View("EditExtensionTerms", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditTerms(Guid id, String productname = null)
        {
            User user = null;
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null);
                model.ClientAgreementId = id;
                model.ClientProgrammeId = agreement.ClientInformationSheet.Programme.Id;
                foreach (var terms in agreement.ClientAgreementTerms)
                {
                    if (terms.BoatTerms.Where(bvt => bvt.DateDeleted == null).Count() > 0)
                    {
                        var boats = new List<EditTermsViewModel>();
                        foreach (var boat in terms.BoatTerms)
                        {
                            boats.Add(new EditTermsViewModel
                            {
                                VesselId = boat.Id,
                                BoatName = boat.BoatName,
                                BoatMake = boat.BoatMake,
                                BoatModel = boat.BoatModel,
                                TermLimit = boat.TermLimit,
                                Excess = Convert.ToInt32(boat.Excess),
                                Premium = boat.Premium,
                                FSL = boat.FSL
                            });
                        }
                        model.BVTerms = boats;
                    }

                    if (terms.MotorTerms.Where(mvt => mvt.DateDeleted == null).Count() > 0)
                    {
                        var motors = new List<EditTermsViewModel>();
                        foreach (var motor in terms.MotorTerms)
                        {
                            motors.Add(new EditTermsViewModel
                            {
                                VesselId = motor.Id,
                                Registration = motor.Registration,
                                Make = motor.Make,
                                Model = motor.Model,
                                TermLimit = motor.TermLimit,
                                Excess = Convert.ToInt32(motor.Excess),
                                Premium = motor.Premium,
                                FSL = motor.FSL
                            });
                        }
                        model.MVTerms = motors;
                    }

                    if (terms.MotorTerms.Where(mvt => mvt.DateDeleted == null).Count() > 0)
                    {
                        var motors = new List<EditTermsViewModel>();
                        foreach (var motor in terms.MotorTerms)
                        {
                            motors.Add(new EditTermsViewModel
                            {
                                VesselId = motor.Id,
                                Registration = motor.Registration,
                                Make = motor.Make,
                                Model = motor.Model,
                                TermLimit = motor.TermLimit,
                                Excess = Convert.ToInt32(motor.Excess),
                                Premium = motor.Premium,
                                FSL = motor.FSL
                            });
                        }
                        model.MVTerms = motors;
                    }
                }
                var subtypeterms = new List<EditTermsViewModel>();


                foreach (var subtypeterm in agreement.ClientAgreementTerms.Where(t => t.SubTermType == productname && t.DateDeleted == null))
                {
                    subtypeterms.Add(new EditTermsViewModel
                    {
                        TermId = subtypeterm.Id,
                        TermType = subtypeterm.SubTermType,
                        TermLimit = subtypeterm.TermLimit,
                        Excess = Convert.ToInt32(subtypeterm.Excess),
                        Premium = subtypeterm.Premium,
                        BasePremium = subtypeterm.BasePremium,
                        PremiumDiffer = subtypeterm.PremiumDiffer
                    });
                }

                model.SubtypeTerms = subtypeterms.OrderBy(acat => acat.TermLimit).ToList();
                model.ProductName = productname;
                //model.EDTerms = edterms.OrderBy(acat => acat.TermLimit).ToList();
                //model.PITerms = piterms.OrderBy(acat => acat.TermLimit).ToList();
                //model.ELTerms = elterms.OrderBy(acat => acat.TermLimit).ToList();
                //model.CLTerms = clterms.OrderBy(acat => acat.TermLimit).ToList();
                //model.SLTerms = slterms.OrderBy(acat => acat.TermLimit).ToList();
                //model.DOTerms = doterms.OrderBy(acat => acat.TermLimit).ToList();
                ViewBag.Title = "Edit Terms ";

                return View("EditTerms", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTerm(EditTermsViewModel clientAgreementBVTerm)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementBVTerm.clientAgreementId);
                ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV");

                ClientAgreementBVTerm bvTerm = null;
                if (term.BoatTerms != null)
                {
                    bvTerm = term.BoatTerms.FirstOrDefault(bvt => bvt.Boat.BoatName == clientAgreementBVTerm.BoatName);

                }
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    term.Premium -= bvTerm.Premium;
                    term.Premium += clientAgreementBVTerm.Premium;
                    bvTerm.TermLimit = clientAgreementBVTerm.TermLimit;
                    bvTerm.Excess = clientAgreementBVTerm.Excess;
                    bvTerm.Premium = clientAgreementBVTerm.Premium;
                    bvTerm.FSL = clientAgreementBVTerm.FSL;
                    await uow.Commit();
                }

                return RedirectToAction("EditTerms", new { id = clientAgreementBVTerm.clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExtensionTerm(EditExtensionTermsViewModel clientAgreemenExtensionTerm)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreemenExtensionTerm.clientAgreementId);
                ClientAgreementTermExtension term = agreement.ClientAgreementTermExtensions.FirstOrDefault(t => t.Id == clientAgreemenExtensionTerm.TermId && t.DateDeleted == null);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {

                    term.DateDeleted = DateTime.UtcNow;
                    await uow.Commit();
                }

                return RedirectToAction("EditExtensionTerms", new { id = clientAgreemenExtensionTerm.clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditExtensionTerm(Guid clientAgreementId, EditExtensionTermsViewModel clientAgreementSubTerm)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementId);
                if (clientAgreementSubTerm.TermId != Guid.Empty)
                {
                    ClientAgreementTermExtension Extensionterm = agreement.ClientAgreementTermExtensions.FirstOrDefault(t => t.Id == clientAgreementSubTerm.TermId && t.DateDeleted == null);
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        Extensionterm.Premium = clientAgreementSubTerm.Premium;
                        Extensionterm.TermLimit = clientAgreementSubTerm.TermLimit;
                        Extensionterm.Excess = clientAgreementSubTerm.Excess;
                        await uow.Commit();
                    }
                }
                else
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        decimal brokeragerate = agreement.Product.DefaultBrokerage;
                        decimal Brokerage = clientAgreementSubTerm.Premium * agreement.Product.DefaultBrokerage / 100;
                        _clientAgreementTermService.AddAgreementExtensionTerm(user, clientAgreementSubTerm.TermLimit, clientAgreementSubTerm.Excess, clientAgreementSubTerm.Premium, agreement);
                        await uow.Commit();
                    }
                }
                return RedirectToAction("EditExtensionTerms", new { id = clientAgreementId });

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditSubTerm(Guid clientAgreementId, EditTermsViewModel clientAgreementSubTerm)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementId);
                if (clientAgreementSubTerm.TermId != Guid.Empty)
                {
                    ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.Id == clientAgreementSubTerm.TermId && t.SubTermType == clientAgreementSubTerm.TermType && t.DateDeleted == null);
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        term.Premium = clientAgreementSubTerm.Premium;
                        term.TermLimit = clientAgreementSubTerm.TermLimit;
                        term.Excess = clientAgreementSubTerm.Excess;
                        term.PremiumDiffer = clientAgreementSubTerm.PremiumDiffer;
                        await uow.Commit();
                    }
                }
                else
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        decimal brokeragerate = agreement.Product.DefaultBrokerage;
                        decimal Brokerage = clientAgreementSubTerm.Premium * agreement.Product.DefaultBrokerage / 100;
                        _clientAgreementTermService.AddAgreementTerm(user, clientAgreementSubTerm.TermLimit, clientAgreementSubTerm.Excess, clientAgreementSubTerm.Premium, 0.0m, brokeragerate, Brokerage, agreement, clientAgreementSubTerm.TermType);
                        await uow.Commit();
                    }
                }
                return RedirectToAction("EditTerms", new { id = clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditMotorTerm(Guid clientAgreementId, EditTermsViewModel clientAgreementMVTerm)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementId);
                ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null);

                ClientAgreementMVTerm mvTerm = null;
                if (term.MotorTerms != null)
                {
                    mvTerm = term.MotorTerms.FirstOrDefault(bvt => bvt.Model == clientAgreementMVTerm.Model);
                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    term.Premium -= mvTerm.Premium;
                    term.Premium += clientAgreementMVTerm.Premium;
                    mvTerm.TermLimit = clientAgreementMVTerm.TermLimit;
                    mvTerm.Excess = clientAgreementMVTerm.Excess;
                    mvTerm.Premium = clientAgreementMVTerm.Premium;
                    mvTerm.FSL = clientAgreementMVTerm.FSL;
                    await uow.Commit();
                }

                return RedirectToAction("EditTerms", new { id = clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteTerm(EditTermsViewModel clientAgreementBVTerm)
        {
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementBVTerm.clientAgreementId);
                ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null);
                ClientAgreementBVTerm bvTerm = null;
                ClientAgreementMVTerm mvTerm = null;

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {

                    if (term.BoatTerms != null)
                    {

                        bvTerm = term.BoatTerms.FirstOrDefault(bvt => bvt.Id == clientAgreementBVTerm.VesselId);
                        term.BoatTerms.Remove(bvTerm);
                    }
                    if (term.MotorTerms != null)
                    {
                        mvTerm = term.MotorTerms.FirstOrDefault(bvt => bvt.Id == clientAgreementBVTerm.VesselId);
                        term.MotorTerms.Remove(mvTerm);
                    }
                    await uow.Commit();
                }

                return RedirectToAction("EditTerms", new { id = clientAgreementBVTerm.clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteSubTerm(EditTermsViewModel clientAgreementBVTerm)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(clientAgreementBVTerm.clientAgreementId);
                ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.Id == clientAgreementBVTerm.TermId && t.DateDeleted == null);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {

                    term.DateDeleted = DateTime.UtcNow;
                    await uow.Commit();
                }

                return RedirectToAction("EditTerms", new { id = clientAgreementBVTerm.clientAgreementId });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAgreement(Guid id)
        {
            var models = new BaseListViewModel<ViewAgreementViewModel>();
            User user = null;
            try
            {
                user = await CurrentUser();
                ViewAgreementViewModel model;
                ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(id);
                Organisation insured = clientProgramme.Owner;
                ClientInformationSheet sheet = clientProgramme.InformationSheet;
                var insuranceRoles = new List<InsuranceRoleViewModel>();
                ViewBag.progid = clientProgramme.Id;
                NumberFormatInfo currencyFormat = new CultureInfo(CultureInfo.CurrentCulture.ToString()).NumberFormat;
                currencyFormat.CurrencyNegativePattern = 2;

                // List Agreement Parties
                insuranceRoles.Add(new InsuranceRoleViewModel { RoleName = "Client", Name = insured.Name, ManagedBy = "", Email = "" });
                foreach (ClientAgreement agreement in clientProgramme.Agreements.Where(apa => apa.DateDeleted == null).OrderBy(apa => apa.Product.OrderNumber))
                {
                    model = new ViewAgreementViewModel(agreement, sheet, UserCulture)
                    {
                        EditEnabled = true,
                        ClientAgreementId = agreement.Id,
                        ClientProgrammeId = clientProgramme.Id,
                        SentOnlineAcceptance = agreement.SentOnlineAcceptance
                    };

                    // Populate the ViewModel
                    model.ProgrammeId = agreement.ClientInformationSheet.Programme.BaseProgramme.Id;
                    model.InsuranceRoles = insuranceRoles;
                    model.ProductName = agreement.Product.Name;
                    var prodcodesubtring = agreement.Product.UnderwritingModuleCode.Substring(agreement.Product.UnderwritingModuleCode.IndexOf("_") + 1);
                    var hjg = prodcodesubtring.IndexOf("_");
                    if (hjg > 0)
                    {
                        model.ProductCode = prodcodesubtring.Substring(0, prodcodesubtring.IndexOf("_"));
                    }
                    else
                    {
                        model.ProductCode = prodcodesubtring;
                    }
                    model.IsMultipleOption = agreement.Product.IsMultipleOption;
                    model.IsOptionalProduct = agreement.Product.IsOptionalProduct;
                    model.Status = agreement.Status;
                    model.InformationSheetStatus = sheet.Status;
                    model.IsExtentionCoverOption = agreement.Product.IsExtentionOption;
                    Boolean nextInfoSheet = false;
                    Boolean IsChange = false;

                    if (null != sheet.NextInformationSheet)
                    {
                        model.NextInfoSheet = true;
                    }
                    else
                    {
                        model.NextInfoSheet = false;
                    }

                    if (null != sheet.IsChange)
                    {
                        model.IsChange = sheet.IsChange;
                    }

                    model.StartDate = LocalizeTimeDate(agreement.InceptionDate, "dd-mm-yyyy");
                    model.EndDate = LocalizeTimeDate(agreement.ExpiryDate, "dd-mm-yyyy");
                    model.AdministrationFee = agreement.BrokerFee.ToString("C", UserCulture);
                    model.BrokerageRate = (agreement.Brokerage / 100).ToString("P2", UserCulture);
                    model.AdditionalCertFee = agreement.AdditionalCertFee.ToString("C", UserCulture);
                    model.PlacementFee = agreement.PlacementFee.ToString("C", UserCulture);
                    model.CurrencySymbol = "fa fa-dollar";
                    model.ClientInformationSheet = sheet;
                    if (sheet.Programme.BaseProgramme.UsesEGlobal &&
                        sheet.Programme.EGlobalBranchCode != null && sheet.Programme.EGlobalClientNumber != null)
                    {
                        model.ClientNumber = sheet.Programme.EGlobalBranchCode + "-" + sheet.Programme.EGlobalClientNumber;
                    }
                    else if (!sheet.Programme.BaseProgramme.UsesEGlobal && sheet.Programme.EGlobalClientNumber != null)
                    {
                        model.ClientNumber = sheet.Programme.EGlobalClientNumber;
                    }
                    else
                    {
                        model.ClientNumber = agreement.ClientNumber;
                    }
                    model.PolicyNumber = agreement.PolicyNumber;
                    model.InformationSheetId = sheet.Id;
                    model.AgreementExtensions = agreement.ClientAgreementTermExtensions;
                    models.Add(model);
                }

                ViewBag.Title = clientProgramme.BaseProgramme.Name + " Agreement for " + insured.Name;
                ViewBag.Status = sheet.Status;
                ClientAgreement masterclientagreement = clientProgramme.Agreements.Where(cpam => cpam.MasterAgreement).FirstOrDefault();
                if (clientProgramme.BaseProgramme.StopAgreement && masterclientagreement.DateCreated >= clientProgramme.BaseProgramme.StopAgreementDateTime)
                {
                    model = new ViewAgreementViewModel();
                    model.ProgrammeStopAgreement = clientProgramme.BaseProgramme.StopAgreement;
                    model.AgreementMessage = clientProgramme.BaseProgramme.StopAgreementMessage;

                    if (clientProgramme.InformationSheet.Status != "Submitted" && clientProgramme.InformationSheet.Status != "Bound")
                    {
                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {
                            clientProgramme.InformationSheet.Status = "Submitted";
                            clientProgramme.InformationSheet.SubmitDate = DateTime.UtcNow;
                            clientProgramme.InformationSheet.SubmittedBy = user;
                            await uow.Commit();
                        }
                    }

                    return PartialView("_ViewStopAgreementMessage", model);
                }

                if (clientProgramme.InformationSheet.Status == "Not Taken Up")
                {
                    model = new ViewAgreementViewModel();
                    model.InformationSheetStatus = "Not Taken Up";
                    return PartialView("_ViewNTUedAgreement", model);
                }

                //To do: check the other status later
                if (clientProgramme.BaseProgramme.HasSubsystemEnabled && clientProgramme.InformationSheet.Status != "Bound")
                {
                    return await ViewAgreementSubsystem(clientProgramme, models, user);
                }
                else
                {
                    return PartialView("_ViewAgreementList", models);
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        private async Task<IActionResult> ViewAgreementSubsystem(ClientProgramme clientProgramme, BaseListViewModel<ViewAgreementViewModel> models, User user)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            model.ProgrammeStopAgreement = true;
            model.AgreementMessage = clientProgramme.BaseProgramme.SubsystemStopAgreementMessage;
            if (string.IsNullOrWhiteSpace(model.AgreementMessage))
            {
                model.AgreementMessage = " The other advisors are now being notified so that they can login and complete their own declaration.";
            }

            var isBaseClientProgramme = await _programmeService.IsBaseClass(clientProgramme);
            if (isBaseClientProgramme)
            {
                bool isComplete;
                IList<SubClientProgramme> SubClientProgrammes;
                if (clientProgramme.InformationSheet.IsChange)
                {
                    SubClientProgrammes = clientProgramme.InformationSheet.PreviousInformationSheet.Programme.SubClientProgrammes;
                    if (!SubClientProgrammes.Any())
                    {
                        SubClientProgrammes = clientProgramme.SubClientProgrammes;
                    }
                }
                else
                {
                    SubClientProgrammes = clientProgramme.SubClientProgrammes;
                }
                if (SubClientProgrammes.Any())
                {
                    await _subsystemService.ValidateProgramme(clientProgramme.InformationSheet, user);
                    isComplete = await _programmeService.SubsystemCompleted(clientProgramme);
                }
                else
                {
                    await _subsystemService.CreateSubObjects(clientProgramme.Id, clientProgramme.InformationSheet, user);
                    isComplete = clientProgramme.SubClientProgrammes.Count == 0;
                }
                if (isComplete)
                {
                    return PartialView("_ViewAgreementList", models);
                }
                else
                {
                    return PartialView("_ViewStopAgreementMessage", model);
                }
            }
            else
            {
                //Notify broker 
                model.AgreementMessage = clientProgramme.BaseProgramme.SubsystemDeclaration;
                if (string.IsNullOrWhiteSpace(model.AgreementMessage))
                {
                    model.AgreementMessage = @" <li>I declare that the information and answers given in this proposal have been checked and are true and complete in every respect and the applicant is not aware of any other information that may be material in considering this proposal.</li>
                                            <li>I acknowledge that this proposal, declaration and any other information supplied in support of this proposal constitutes representations to, and will be relied on as the basis of contract by, insurers requested to quote on this proposal. We undertake to inform these insurers through our broker of any material alteration to this information whether occurring before or after the completion of any insurance contract.</li>
                                            <li>I acknowledge that misrepresentations or material non-disclosure of relevant information, whether made through this proposal or otherwise, may result in the insurance not being available to meet a claim and/ or cancellation of relevant insurance contract(s), in addition to other remedies.</li> ";
                }
                return PartialView("_ViewStopAgreementMessage", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveDate(Guid id, string Startdate)
        {
            User user = await CurrentUser();
            try
            {
                ClientAgreement clientAgreement = await _clientAgreementService.GetAgreement(id);
                user = await CurrentUser();
                var date = true;
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    // TODO - Convert to UTC
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone);
                    clientAgreement.InceptionDate = DateTime.Parse(Startdate, UserCulture).ToUniversalTime(tzi);
                    clientAgreement.ExpiryDate = clientAgreement.InceptionDate.AddYears(1);
                    clientAgreement.CustomInceptionDate = true;

                    //boat effective date if the date is prior to the new start date or over 30 days after the new start date
                    foreach (var boat in clientAgreement.ClientInformationSheet.Boats.Where(b => !b.Removed && b.DateDeleted == null))
                    {
                        if (boat.BoatEffectiveDate < clientAgreement.InceptionDate || boat.BoatEffectiveDate > clientAgreement.InceptionDate.AddDays(30))
                        {
                            boat.BoatEffectiveDate = clientAgreement.InceptionDate;
                        }
                    }

                    //update vehicle effective date if the date is prior to the new start date or over 30 days after the new start date
                    foreach (var vehicle in clientAgreement.ClientInformationSheet.Vehicles.Where(v => !v.Removed && v.DateDeleted == null))
                    {
                        if (vehicle.VehicleEffectiveDate < clientAgreement.InceptionDate || vehicle.VehicleEffectiveDate > clientAgreement.InceptionDate.AddDays(30))
                        {
                            vehicle.VehicleEffectiveDate = clientAgreement.InceptionDate;
                        }
                    }

                    string auditLogDetail = "Agreement start date and end date have been modified by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, clientAgreement.ClientInformationSheet, clientAgreement, auditLogDetail);
                    clientAgreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit();
                }

                var url = "/Information/EditInformation/" + clientAgreement.ClientInformationSheet.Programme.Id;
                return Json(new { url });
                //return Json(date);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> ViewAgreementDeclaration(Guid id)
        {
            var models = new BaseListViewModel<ViewAgreementViewModel>();
            User user = null;

            try
            {
                user = await CurrentUser();
                var clientProgramme = await _programmeService.GetClientProgrammebyId(id);
                Organisation insured = clientProgramme.Owner;
                ClientInformationSheet answerSheet = clientProgramme.InformationSheet;
                var isBaseSheet = await _programmeService.IsBaseClass(clientProgramme);
                ViewBag.Title = clientProgramme.BaseProgramme.Name + " Agreement for " + insured.Name;

                models.BaseProgramme = clientProgramme.BaseProgramme;

                if (!isBaseSheet)
                {
                    ViewAgreementViewModel model = new ViewAgreementViewModel();
                    model.AgreementMessage = clientProgramme.BaseProgramme.SubsystemDeclaration;
                    return PartialView("_ViewStopAgreementMessage", model);
                }
                else
                {
                    foreach (ClientAgreement agreement in clientProgramme.Agreements.Where(cagreement => cagreement.DateDeleted == null))
                    {
                        ViewAgreementViewModel model = new ViewAgreementViewModel
                        {
                            EditEnabled = true,
                            ClientAgreementId = agreement.Id,
                            ClientProgrammeId = clientProgramme.Id,
                            Declaration = clientProgramme.BaseProgramme.Declaration
                        };

                        model.Advisory = await _milestoneService.SetMilestoneFor("Agreement Status - Declined", user, answerSheet);
                        model.Status = agreement.Status;
                        model.InformationSheetId = answerSheet.Id;
                        model.CurrentUser = user;
                        models.Add(model);
                    }

                }

                try
                {
                    if (clientProgramme.BaseProgramme.StopDeclaration)
                    {
                        ViewAgreementViewModel model = new ViewAgreementViewModel();
                        model.AgreementMessage = clientProgramme.BaseProgramme.StopAgreementMessage;
                        return PartialView("_ViewStopAgreementMessage", model);
                    }
                }
                catch (Exception ex)
                {
                    clientProgramme.BaseProgramme.StopDeclaration = false;
                    await _programmeService.Update(clientProgramme.BaseProgramme);
                }

                return PartialView("_ViewAgreementDeclaration", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ViewAgreementDeclarationReport(String id)
        {
            var models = new BaseListViewModel<ViewAgreementViewModel>();
            User user = null;

            try
            {
                var clientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse("a7f2e08b-065a-4d42-95b8-ac100035a6e7"));
                Organisation insured = clientProgramme.Owner;
                ClientInformationSheet answerSheet = clientProgramme.InformationSheet;
                var isBaseSheet = await _programmeService.IsBaseClass(clientProgramme);
                ViewBag.Title = clientProgramme.BaseProgramme.Name + " Agreement for " + insured.Name;

                models.BaseProgramme = clientProgramme.BaseProgramme;
                if (!isBaseSheet)
                {
                    ViewAgreementViewModel model = new ViewAgreementViewModel();
                    model.AgreementMessage = clientProgramme.BaseProgramme.SubsystemDeclaration;
                    return PartialView("_ViewStopAgreementMessage", model);
                }
                else
                {
                    foreach (ClientAgreement agreement in clientProgramme.Agreements.Where(cagreement => cagreement.DateDeleted == null))
                    {
                        ViewAgreementViewModel model = new ViewAgreementViewModel
                        {
                            EditEnabled = true,
                            ClientAgreementId = agreement.Id,
                            ClientProgrammeId = clientProgramme.Id,
                            Declaration = clientProgramme.BaseProgramme.Declaration
                        };

                        model.Advisory = await _milestoneService.SetMilestoneFor("Agreement Status - Declined", user, answerSheet);
                        model.Status = agreement.Status;
                        model.InformationSheetId = answerSheet.Id;
                        models.Add(model);
                    }

                }

                try
                {
                    if (clientProgramme.BaseProgramme.StopDeclaration)
                    {
                        ViewAgreementViewModel model = new ViewAgreementViewModel();
                        model.AgreementMessage = clientProgramme.BaseProgramme.StopAgreementMessage;
                        return PartialView("_ViewStopAgreementMessage", model);
                    }
                }
                catch (Exception ex)
                {
                    clientProgramme.BaseProgramme.StopDeclaration = false;
                    await _programmeService.Update(clientProgramme.BaseProgramme);
                }

                return PartialView("_ViewAgreementDeclaration", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ViewPayment(Guid id)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                //need to review this code duplication
                var models = new BaseListViewModel<ViewAgreementViewModel>();

                ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(id);
                ClientInformationSheet answerSheet = clientProgramme.InformationSheet;
                NumberFormatInfo currencyFormat = new CultureInfo(CultureInfo.CurrentCulture.ToString()).NumberFormat;
                currencyFormat.CurrencyNegativePattern = 2;

                decimal totalPayable = 0M;
                foreach (ClientAgreement agreement in clientProgramme.Agreements.Where(cagreement => cagreement.DateDeleted == null))
                {
                    if (agreement.Status == "Quoted" && agreement.DateDeleted == null)
                    {
                        ViewAgreementViewModel model = new ViewAgreementViewModel
                        {
                            EditEnabled = true,
                            ClientAgreementId = agreement.Id,
                            ClientProgrammeId = clientProgramme.Id,
                            ClientInformationSheet = clientProgramme.InformationSheet
                        };

                        var riskPremiums = new List<RiskPremiumsViewModel>();
                        var extPremiums = new List<ExtensionCoverOptions>();
                        string riskname = null;

                        // List Agreement Inclusions
                        foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                        {
                            if (term.SubTermType == "MV")
                            {
                                riskname = "Motor Vehicle";
                            }
                            else if (term.SubTermType == "BV")
                            {
                                riskname = "Vessel";
                            }
                            else
                            {
                                riskname = agreement.Product.Name;
                            }
                        }

                        // List Agreement Premiums
                        foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                        {
                            if (agreement.Status == "Quoted" && agreement.DateDeleted == null && term.Bound)
                            {
                                if (answerSheet.PreviousInformationSheet == null)
                                {
                                    riskPremiums.Add(new RiskPremiumsViewModel { RiskName = riskname, Premium = (term.Premium - term.FSL).ToString("C"), FSL = term.FSL.ToString("C"), TotalPremium = term.Premium.ToString("C") });
                                    totalPayable += term.Premium;
                                }
                                else
                                {
                                    riskPremiums.Add(new RiskPremiumsViewModel { RiskName = riskname, Premium = string.Format(currencyFormat, "{0:c}", (term.PremiumDiffer - term.FSLDiffer)), FSL = string.Format(currencyFormat, "{0:c}", term.FSLDiffer), TotalPremium = string.Format(currencyFormat, "{0:c}", term.PremiumDiffer) });
                                    totalPayable += term.PremiumDiffer;
                                }
                            }

                        }

                        foreach (ClientAgreementTermExtension termext in agreement.ClientAgreementTermExtensions)
                        {
                            if (agreement.Status == "Quoted" && agreement.DateDeleted == null && termext.Bound)
                            {
                                if (answerSheet.PreviousInformationSheet == null)
                                {
                                    extPremiums.Add(new ExtensionCoverOptions { RiskName = termext.ExtentionName, TotalPremium = termext.Premium.ToString("C") });
                                    totalPayable += termext.Premium;
                                }
                                else
                                {
                                    extPremiums.Add(new ExtensionCoverOptions { RiskName = termext.ExtentionName, TotalPremium = termext.PremiumDiffer.ToString("C") });
                                    totalPayable += termext.PremiumDiffer;
                                }
                            }

                        }

                        bool isActive = true;

                        model.EGlobalIsActive = isActive;

                        // Populate the ViewModel
                        model.RiskPremiums = riskPremiums;
                        model.ExtensionCoverOptions = extPremiums;
                        //model.EGlobalIsActive = isActive;

                        // Status
                        model.ProductName = agreement.Product.Name;
                        model.Status = agreement.Status;
                        model.StartDate = LocalizeTime(agreement.InceptionDate, "d");
                        model.EndDate = LocalizeTime(agreement.ExpiryDate, "d");
                        model.AdministrationFee = agreement.BrokerFee.ToString("C");
                        model.BrokerageRate = (agreement.Brokerage / 100).ToString("P2");
                        model.CurrencySymbol = "fa fa-dollar";
                        model.ClientNumber = agreement.ClientNumber;
                        model.PolicyNumber = agreement.PolicyNumber;

                        model.NoPaymentRequiredMessage = clientProgramme.BaseProgramme.NoPaymentRequiredMessage;
                        model.IsMasterAgreement = agreement.MasterAgreement;
                        models.Add(model);
                    }
                }

                ViewBag.Title = clientProgramme.BaseProgramme.Name + " Payment for " + clientProgramme.Owner.Name;

                bool requirePayment = false;
                if ((clientProgramme.BaseProgramme.HasCCPayment || clientProgramme.BaseProgramme.HasInvoicePayment) && totalPayable > 0)
                {
                    requirePayment = true;
                }

                if (requirePayment)
                {
                    return PartialView("_ViewPaymentList", models);
                }
                else
                {
                    return PartialView("_ViewNoPaymentRequiredMsg", models);
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditAgreement(Guid id)
        {
            ViewAgreementViewModel model = new ViewAgreementViewModel();
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;
                ClientProgramme programme = answerSheet.Programme;
                user = await CurrentUser();

                model.InformationSheetId = answerSheet.Id;
                model.ClientAgreementId = agreement.Id;
                model.ClientProgrammeId = programme.Id;
                model.StartDate = LocalizeTimeDate(agreement.InceptionDate, "dd-mm-yyyy");
                model.EndDate = LocalizeTimeDate(agreement.ExpiryDate, "dd-mm-yyyy");
                model.AdministrationFee = agreement.BrokerFee.ToString("C");
                model.BrokerageRate = (agreement.Brokerage / 100).ToString("P2");
                model.CurrencySymbol = "fa fa-dollar";
                model.ClientNumber = agreement.ClientNumber;
                model.PolicyNumber = agreement.PolicyNumber;
                model.RetroactiveDate = agreement.RetroactiveDate;
                model.TerritoryLimit = agreement.TerritoryLimit;
                model.Jurisdiction = agreement.Jurisdiction;
                model.ProfessionalBusiness = agreement.ProfessionalBusiness;
                model.InsuredName = agreement.InsuredName;

                ViewBag.Title = answerSheet.Programme.BaseProgramme.Name + " Edit Agreement for " + insured.Name;

                return View("EditAgreement", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAgreement(ViewAgreementViewModel model)
        {
            User user = null;
            try
            {
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementId);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                user = await CurrentUser();
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    // TODO - Convert to UTC
                    TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone);
                    agreement.InceptionDate = DateTime.Parse(model.StartDate, CultureInfo.CreateSpecificCulture("en-NZ")).ToUniversalTime();
                    agreement.ExpiryDate = DateTime.Parse(model.EndDate, CultureInfo.CreateSpecificCulture("en-NZ")).ToUniversalTime();
                    agreement.Brokerage = Convert.ToDecimal(model.BrokerageRate.Replace("%", ""));
                    agreement.BrokerFee = Convert.ToDecimal(model.AdministrationFee.Replace("$", ""));
                    agreement.ClientNumber = model.ClientNumber;
                    agreement.PolicyNumber = model.PolicyNumber;
                    agreement.RetroactiveDate = model.RetroactiveDate;
                    agreement.Jurisdiction = model.Jurisdiction;
                    agreement.TerritoryLimit = model.TerritoryLimit;
                    agreement.ProfessionalBusiness = model.ProfessionalBusiness;
                    agreement.InsuredName = model.InsuredName;

                    string auditLogDetail = "Agreement details have been modified by " + user.FullName;
                    AuditLog auditLog = new AuditLog(user, answerSheet, agreement, auditLogDetail);
                    agreement.ClientAgreementAuditLogs.Add(auditLog);

                    await uow.Commit();
                }

                return Redirect("/Agreement/ViewAcceptedAgreement/" + answerSheet.Programme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAgreementRule(Guid id)
        {
            ViewAgreementRuleViewModel model = new ViewAgreementRuleViewModel();
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;

                //Client Agreement Rules
                model.HasRules = agreement.ClientAgreementRules.Count > 0;

                model.ClientAgreementID = id;
                model.ClientProgrammeID = answerSheet.Programme.Id;

                if (model.HasRules)
                {
                    var clientAgreementRules = new AgreementRulesViewModel();
                    var clientAgreementRulesTypeRate = new AgreementRulesViewModel();
                    foreach (ClientAgreementRule cr in agreement.ClientAgreementRules.OrderBy(cr => cr.OrderNumber))
                    {
                        clientAgreementRules.Add(new ClientAgreementRuleViewModel { ClientAgreementRuleID = cr.Id, Description = cr.Description, Value = cr.Value });
                        if (cr.RuleCategory == "uwrate")
                        {
                            clientAgreementRulesTypeRate.Add(new ClientAgreementRuleViewModel { ClientAgreementRuleID = cr.Id, Description = cr.Description, Value = cr.Value });
                        }
                    }
                    model.ClientAgreementRules = clientAgreementRules;
                    model.ClientAgreementRulesTypeRate = clientAgreementRulesTypeRate;
                }

                ViewBag.Title = answerSheet.Programme.BaseProgramme.Name + " Agreement Rule for " + insured.Name;

                return View("ViewAgreementRule", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAdvisor(Guid id)
        {
            ProgrammeInfoViewModel model = new ProgrammeInfoViewModel();
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                model.Owner = agreement.ClientInformationSheet.Organisation.Where(o => o.InsuranceAttributes.Any(i => i.Name == "Advisor") && o.Removed != true && o.DateDeleted == null).ToList();
                model.ProgId = answerSheet.Programme.Id;
                model.AgreementId = id;
                //ViewBag.Title = answerSheet.Programme.BaseProgramme.Name + " Agreement Rule for " + insured.Name;

                return View("ViewEditAdvisor", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAdvisor(IFormCollection collection)
        {
            User user = null;
            try
            {
                Guid.TryParse(collection["ProgId"].ToString(), out Guid Id);
                await _organisationService.UpdateAdvisorDates(collection);
                return Redirect("/Agreement/ViewAcceptedAgreement/" + Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> ViewAgreementRule(ViewAgreementRuleViewModel model)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementID);
                if (model.ClientAgreementRules.Any(mcr => mcr != null && mcr.Value != null))
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        foreach (ClientAgreementRuleViewModel crv in model.ClientAgreementRules.OrderBy(cr => cr.OrderNumber))
                        {
                            var clientAgreementRule = await _clientAgreementRuleService.GetClientAgreementRuleBy(crv.ClientAgreementRuleID);
                            clientAgreementRule.Value = crv.Value;
                        }
                        await uow.Commit();
                    }
                }

                return Redirect("/Information/EditInformation/" + model.ClientProgrammeID);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> ViewAgreementEndorsement(Guid id)
        {
            ViewAgreementEndorsementViewModel model = new ViewAgreementEndorsementViewModel();
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(id);
                ClientInformationSheet answerSheet = agreement.ClientInformationSheet;
                Organisation insured = answerSheet.Owner;

                //Client Agreement Endorsements
                model.HasEndorsements = agreement.ClientAgreementEndorsements.Count > 0;

                model.ClientAgreementID = id;

                model.ClientProgrammeID = answerSheet.Programme.Id;

                if (model.HasEndorsements)
                {
                    var clientAgreementEndorsements = new AgreementEndorsementsViewModel();
                    foreach (ClientAgreementEndorsement ce in agreement.ClientAgreementEndorsements.Where(ce => ce.DateDeleted == null && ce.Removed != true).OrderBy(ce => ce.OrderNumber))
                    {
                        clientAgreementEndorsements.Add(new ClientAgreementEndorsementViewModel { ClientAgreementEndorsementID = ce.Id, Name = ce.Name, Value = ce.Value });
                    }
                    model.ClientAgreementEndorsements = clientAgreementEndorsements;

                }
                else
                {
                    model.ClientAgreementEndorsements = null;
                }

                var availableEndorsementTitles = new List<SelectListItem>();

                foreach (ClientAgreementEndorsement ce in agreement.ClientAgreementEndorsements.Where(ce => ce.DateDeleted != null).OrderBy(ce => ce.OrderNumber))
                {

                    availableEndorsementTitles.Add(new SelectListItem
                    {
                        Selected = false,
                        Value = ce.Id.ToString(),
                        Text = ce.Name
                    });
                }

                model.AvailableEndorsementTitles = availableEndorsementTitles;
                ViewBag.Title = answerSheet.Programme.BaseProgramme.Name + " Agreement Endorsements for " + insured.Name;

                return View("ViewAgreementEndorsement", model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ViewAgreementEndorsement(ViewAgreementEndorsementViewModel model)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementID);
                var clientAgreementendorsement = await _clientAgreementEndorsementService.GetClientAgreementEndorsementBy(model.ClientAgreementEndorsementID);

                if (model.ClientAgreementEndorsementID == Guid.Empty)
                {
                    if (model.EndorsementNameToAdd != null && model.Content != null)
                    {
                        await _clientAgreementEndorsementService.AddClientAgreementEndorsement(user, model.EndorsementNameToAdd, "Exclusion", agreement.Product, model.Content, 100, agreement);
                    }
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (model.ClientAgreementEndorsements != null)
                        {

                            foreach (ClientAgreementEndorsementViewModel cev in model.ClientAgreementEndorsements.OrderBy(ce => ce.OrderNumber))
                            {
                                var clientAgreement = await _clientAgreementEndorsementService.GetClientAgreementEndorsementBy(cev.ClientAgreementEndorsementID);
                                cev.Value = clientAgreement.Value;
                            }
                            await uow.Commit();
                        }

                    }
                }
                else
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        clientAgreementendorsement.Name = model.EndorsementNameToAdd;
                        clientAgreementendorsement.Value = model.Content;
                        clientAgreementendorsement.DateDeleted = null;
                        await uow.Commit();

                    }
                }

                return Redirect(model.ClientAgreementID.ToString());
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAgreementEndorsement(Guid clientAgreementEndorsementID)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                //ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementID);
                var clientAgreementendorsement = await _clientAgreementEndorsementService.GetClientAgreementEndorsementBy(clientAgreementEndorsementID);
                ClientAgreementEndorsementViewModel model = new ClientAgreementEndorsementViewModel();
                if (clientAgreementendorsement != null)
                {
                    model.ClientAgreementEndorsementID = clientAgreementendorsement.Id;
                    model.Name = clientAgreementendorsement.Name;
                    model.Value = clientAgreementendorsement.Value;
                    //await _clientAgreementEndorsementService.AddClientAgreementEndorsement(user, model.EndorsementNameToAdd, "Exclusion", agreement.Product, model.Content, 100, agreement);
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
        public async Task<IActionResult> SetEndorsementRemovedStatus(Guid clientAgreementEndorsementID)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                //ClientAgreement agreement = await _clientAgreementService.GetAgreement(model.ClientAgreementID);
                var clientAgreementendorsement = await _clientAgreementEndorsementService.GetClientAgreementEndorsementBy(clientAgreementEndorsementID);

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    clientAgreementendorsement.Removed = true;

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

        [HttpGet]
        public async Task<IActionResult> AcceptAgreement(Guid Id)
        {
            User user = null;
            List<AgreementDocumentViewModel> models = new List<AgreementDocumentViewModel>();
            try
            {
                ClientProgramme programme = await _programmeService.GetClientProgrammebyId(Id);
                user = await CurrentUser();
                foreach (ClientAgreement agreement in programme.Agreements)
                {
                    if (agreement == null)
                        throw new Exception(string.Format("No Agreement found for {0}", agreement.Id));

                    var agreeDocList = agreement.GetDocuments();
                    //RenderDocs(agreement,user);
                    //foreach (SystemDocument doc in agreeDocList)
                    //{
                    //    // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                    //    if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                    //    {
                    //        doc.Delete(user);
                    //    }
                    //}

                    //foreach (SystemDocument template in agreement.Product.Documents)
                    //{
                    //    SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                    //    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                    //    agreement.Documents.Add(renderedDoc);
                    //    await _fileService.UploadFile(renderedDoc);
                    //}

                    ClientAgreement reloadedAgreement = await _clientAgreementService.GetAgreement(agreement.Id);
                    agreeDocList = reloadedAgreement.GetDocuments();
                    foreach (SystemDocument doc in agreeDocList)
                    {
                        if (doc.DocumentType == 4)
                        {
                            if (programme.EGlobalClientNumber != null)
                            {
                                models.Add(new AgreementDocumentViewModel { DisplayName = doc.Name, Url = "/File/GetDocument/" + doc.Id, RenderToPDF = doc.RenderToPDF });
                            }
                        }
                        else
                            models.Add(new AgreementDocumentViewModel { DisplayName = doc.Name, Url = "/File/GetDocument/" + doc.Id, RenderToPDF = doc.RenderToPDF });
                    }

                    if (agreement.Product.Id == new Guid("bc62172c-1e15-4e5a-8547-a7bd002121eb"))
                    { //Arcco
                        await _clientAgreementService.AcceptAgreement(agreement, user);
                    }

                }

                return PartialView("_ViewAgreementDocs", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> RerenderDocs(string ProgrammeId, string ClientProgId = null)
        {
            User user = null;
            ViewAgreementViewModel viewAgreementViewModel = new ViewAgreementViewModel();
            List<Product> listAgreementProduct = new List<Product>();
            try
            {
                user = await CurrentUser();
                viewAgreementViewModel.ProgrammeId = Guid.Parse(ProgrammeId);
                ViewBag.IsTC = user.PrimaryOrganisation.IsTC;
                ViewBag.IsInsurer = user.PrimaryOrganisation.IsInsurer;
                ViewBag.IsBroker = user.PrimaryOrganisation.IsBroker;


                if (ClientProgId == null)
                {
                    viewAgreementViewModel.ClientProgrammeId = Guid.Empty;

                    listAgreementProduct = await GetAgreementProduct(ProgrammeId);
                }
                else
                {
                    viewAgreementViewModel.ClientProgrammeId = Guid.Parse(ClientProgId);

                    listAgreementProduct = await GetAgreementProductbyClientProg(ClientProgId);
                }
                viewAgreementViewModel.AgreementProducts = listAgreementProduct.Distinct().ToList();
                return View(viewAgreementViewModel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return View(viewAgreementViewModel);
            }
        }
        [HttpPost]
        public async Task<IActionResult> PopulateTemplate(string ProductId, string ProgrammeId)
        {
            User user = null;
            ViewAgreementViewModel viewAgreementViewModel = new ViewAgreementViewModel();
            List<String> listProductTemplate = new List<String>();
            Product product = await _productService.GetProductById(Guid.Parse(ProductId));
            try
            {
                List<ClientProgramme> ClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(Guid.Parse(ProgrammeId));

                foreach (SystemDocument systemDocument in product.Documents)
                {
                    listProductTemplate.Add(systemDocument.Name);
                }
                user = await CurrentUser();
                viewAgreementViewModel.ProgrammeId = Guid.Parse(ProgrammeId);
                viewAgreementViewModel.AgreementTemplates = listProductTemplate.Distinct().ToList();
                return Json(viewAgreementViewModel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return View(viewAgreementViewModel);
            }
        }
        [HttpGet]
        public async Task<List<Product>> GetAgreementProduct(string ProgrammeId)
        {
            User user = null;
            List<Product> listProduct = new List<Product>();

            try
            {
                List<ClientProgramme> ClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(Guid.Parse(ProgrammeId));

                foreach (ClientProgramme clientprogramme in ClientProgrammes)
                {
                    foreach (ClientAgreement agreement in clientprogramme.Agreements)
                    {
                        listProduct.Add(agreement.Product);
                    }
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }
            return listProduct;
        }

        [HttpGet]
        public async Task<List<Product>> GetAgreementProductbyClientProg(string ClientProgId)
        {
            User user = null;
            List<Product> listProduct = new List<Product>();

            try
            {
                ClientProgramme ClientProgramme = await _programmeService.GetClientProgrammebyId(Guid.Parse(ClientProgId));

                foreach (ClientAgreement agreement in ClientProgramme.Agreements)
                {
                    listProduct.Add(agreement.Product);

                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }
            return listProduct;
        }

        //Rerenderalldocs flag will be true for only rerenderallfuntionality on programme level,sendUser is to define which user to send documents.
        public async void CommonRenderDocs(Guid ProgrammeId, string Action = null, string status = null, ClientInformationSheet sheet = null,
                                                                   bool Rerenderalldocs = false, bool sendUser = false, string ActionPath = null)
        {
            User user = null;
            try
            {
                var clientProgrammes = new List<ClientProgramme>();
                List<ClientProgramme> ClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(ProgrammeId);
                foreach (ClientProgramme programme in ClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    user = await CurrentUser();
                    RerenderClientProgrammes(programme, ActionPath, Action, status, Rerenderalldocs, sendUser);
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }
        }
        public async Task<IActionResult> RerenderAlldocs(Guid ProgrammeId)
        {
            bool Rerenderalldocs = true;
            CommonRenderDocs(ProgrammeId, null, null, null, Rerenderalldocs, false);
            return await RedirectToLocal();
        }


        public async Task<IActionResult> RerenderClientProgrammes(ClientProgramme programme, string ActionPath, string Action, string status, bool Rerenderalldocs, bool sendUser, string bindnotes = null)

        {
            User user = await CurrentUser();
            try
            {
                foreach (ClientAgreement agreement in programme.Agreements.Where(agree => agree.DateDeleted == null && agree.InsurerDeclined != true))
                {

                    if (agreement.Status == "Quoted" || ActionPath == "SendPolicyDocuments")
                    {
                        if (Action == "BindAgreement")
                        {
                            agreement.BindNotes = bindnotes;
                            agreement.BindByUserID = user;
                        }

                        if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null && acagreement.Bound).Count() > 0)
                        {

                            var documents = new List<SystemDocument>();
                            var documentspremiumadvice = new List<SystemDocument>();
                            var agreeTemplateList = agreement.Product.Documents;
                            var agreeDocList = agreement.GetDocuments();

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                if (agreement.Status != status && status != null)
                                {
                                    agreement.Status = status;
                                    agreement.BoundDate = DateTime.Now;
                                    if (programme.BaseProgramme.PolicyNumberPrefixString != null)//programme PolicyNumberPrefixString 
                                    {
                                        agreement.PolicyNumber = programme.BaseProgramme.PolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    if (agreement.Product.ProductPolicyNumberPrefixString != null)//product PolicyNumberPrefixString
                                    {
                                        agreement.PolicyNumber = agreement.Product.ProductPolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    await uow.Commit().ConfigureAwait(false);
                                }
                            }
                            if (ActionPath != "SendPolicyDocuments")
                                agreement.Status = status;


                            foreach (SystemDocument doc in agreeDocList)
                            {
                                // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                                if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                                {
                                    doc.Delete(user);
                                }
                            }

                            //tripleA DO use case, remove when all client set as company
                            if (agreement.Product.Id == new Guid("bdbdda02-ee4e-44f5-84a8-dd18d17287c1") &&
                                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDAOLIOptions").First().Value == "2")
                            {

                            }
                            else
                            {

                                if (!agreement.Product.IsOptionalCombinedProduct)
                                {
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType != 7))
                                    {
                                        documents.Add(await RerenderTemplate(template, agreement, programme));
                                    }
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType == 7))
                                    {
                                        documentspremiumadvice.Add(await RerenderTemplate(template, agreement, programme));
                                    }
                                    if (programme.BaseProgramme.ProgEnableEmail && !Rerenderalldocs)
                                    {
                                        if (!programme.BaseProgramme.ProgStopPolicyDocAutoRelease)
                                        {
                                            //send out policy document email
                                            EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == "SendPolicyDocuments");
                                            if (emailTemplate != null)
                                            {
                                                if (sendUser)
                                                {
                                                    await _emailService.SendEmailViaEmailTemplate(user.Email, emailTemplate, documents, agreement.ClientInformationSheet, agreement);
                                                }
                                                else
                                                {
                                                    await _emailService.SendEmailViaEmailTemplate(programme.Owner.Email, emailTemplate, documents, agreement.ClientInformationSheet, agreement);
                                                }


                                                if (!agreement.IsPolicyDocSend)
                                                {
                                                    agreement.IsPolicyDocSend = true;
                                                    agreement.DocIssueDate = DateTime.Now;
                                                }

                                            }
                                        }
                                        //send out premium advice  ///need to check for send policy doc functionality 
                                        if (programme.BaseProgramme.ProgEnableSendPremiumAdvice && !string.IsNullOrEmpty(programme.BaseProgramme.PremiumAdviceRecipent) &&
                                            agreement.Product.ProductEnablePremiumAdvice)
                                        {
                                            await _emailService.SendPremiumAdviceEmail(programme.BaseProgramme.PremiumAdviceRecipent, documentspremiumadvice, agreement.ClientInformationSheet, agreement, programme.BaseProgramme.PremiumAdviceRecipentCC);
                                        }

                                        //send out agreement bound notification email
                                        await _emailService.SendSystemEmailAgreementBoundNotify(programme.BrokerContactUser, programme.BaseProgramme, agreement, programme.Owner);
                                    }
                                }
                            }


                        }
                        else
                        {
                            agreement.DateDeleted = DateTime.Now;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }
            return NoContent();

        }

        public async Task<IActionResult> RenderNamedPartyCOC(Guid sheetId)
        {
            SystemDocument renderedDoc = null;
            ClientInformationSheet clientInformationSheet = await _customerInformationService.GetInformation(sheetId);
            //ClientAgreement agreement =  clientInformationSheet.ClientAgreement;
            ViewAgreementViewModel viewAgreementViewModel = new ViewAgreementViewModel();
            //viewAgreementViewModel.ClientAgreementId = clientInformationSheet.ClientAgreement.Id;
            viewAgreementViewModel.InformationSheetId = sheetId;
            // List<Organisation> advisororgs = new List<Organisation>(); 
            List<Organisation> Organisations = clientInformationSheet.Organisation.Where(o => o.Id != clientInformationSheet.Owner.Id && o.Removed != true).ToList();

            List<SelectListItem> advisororgs = new List<SelectListItem>();

            viewAgreementViewModel.ClientAgreementId = clientInformationSheet.ClientAgreement.Id;
            foreach (Organisation org in Organisations)
            {
                // var ous = ou.OrganisationalUnits;
                if (org.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor") != null)

                {
                    advisororgs.Add(
                                   new SelectListItem()
                                   {
                                       Text = org.Name,
                                       Value = org.Id.ToString(),
                                   });
                }

            }
            viewAgreementViewModel.Organisations = advisororgs;

            return View(viewAgreementViewModel);
        }
        [HttpPost]
        public async Task<List<SystemDocument>> RenderCOC(string[] arrdata, Guid agreementid)
        {
            //SystemDocument renderedDoc = null;
            Document renderedDoc;
            var documents = new List<SystemDocument>();
            User user = await CurrentUser();
            List<SystemDocument> templates = _documentRepository.FindAll().Where(doc => doc.DocumentType == 11).ToList();
            Organisation org = null;

            User owner = null;
            for (var index = 0; index < arrdata.Length; index++)
            {
                var check = Guid.Parse("" + arrdata[index]);
                org = await _organisationService.GetOrganisation(Guid.Parse("" + arrdata[index]));

                if (org != null)
                {
                    owner = await _userService.GetUserByEmail(org.Email);

                }
                if (owner != null)
                {
                    List<ClientInformationSheet> clientInformationSheets = await _customerInformationService.GetAllInformationFor(owner.PrimaryOrganisation);
                    ClientInformationSheet clientInformationSheet = clientInformationSheets.FirstOrDefault(s => s.SubmittedBy.Email == owner.Email);
                    ClientAgreement agreement = clientInformationSheet.ClientAgreement;
                    foreach (var template in templates)
                    {
                        renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                        documents.Add(renderedDoc);
                        await _fileService.UploadFile(renderedDoc);
                    }

                }

            }
            // ClientAgreement agreement = clientInformationSheet.ClientAgreement;
            //ViewAgreementViewModel viewAgreementViewModel = new ViewAgreementViewModel();
            // List<Organisation> Organisations = clientInformationSheet.Organisation.Where(o => o.Name == "Advisor" && o.Id != clientInformationSheet.Owner.Id && o.Removed != true).ToList();
            //var unit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");
            //foreach (Organisation ou in Organisations)
            //{
            // viewAgreementViewModel.advisorUnit = ou.OrganisationalUnits;
            //}

            return documents;
        }

        public async Task<SystemDocument> RerenderTemplate(SystemDocument template, ClientAgreement agreement, ClientProgramme programme)
        {
            Document renderedDoc;
            var documents = new SystemDocument();
            var documentspremiumadvice = new List<SystemDocument>();
            User user = await CurrentUser();
            try
            {
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    List<SystemDocument> agreeDocList = agreement.GetDocuments();
                    foreach (Document doc in agreeDocList.Where(doc => doc.Name == template.Name))
                    {
                        doc.Delete(user);
                    }

                    ClientInformationSheet sheet = agreement.ClientInformationSheet;
                    if (template.ContentType == MediaTypeNames.Application.Pdf)
                    {
                        SystemDocument notRenderedDoc = await _fileService.GetDocumentByID(template.Id);
                        agreement.Documents.Add(notRenderedDoc);
                        documents = notRenderedDoc;
                    }
                    else
                    {
                        //render docs except invoice
                        if (template.DocumentType != 4 && template.DocumentType != 6 && template.DocumentType != 9 && template.DocumentType != 12)
                        {
                            if (template.Name == "TripleA Individual TL Certificate" && !programme.BaseProgramme.IsPdfDoc)
                            {
                                if (agreement.Product.IsOptionalProductBasedSub &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);

                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    agreement.Documents.Add(renderedDoc);
                                    //documents.Add(renderedDoc);
                                    documents = renderedDoc;
                                    await _fileService.UploadFile(renderedDoc);
                                }
                            }
                            else if (template.DocumentType == 7)
                            {
                                renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                agreement.Documents.Add(renderedDoc);
                                //documents.Add(renderedDoc);
                                //documentspremiumadvice.Add(renderedDoc);
                                documents = renderedDoc;
                                await _fileService.UploadFile(renderedDoc);
                            }
                            else if (template.DocumentType == 8 && !programme.BaseProgramme.IsPdfDoc)
                            {
                                renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);

                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                agreement.Documents.Add(renderedDoc);
                                //documents.Add(renderedDoc);
                                documents = renderedDoc;

                                await _fileService.UploadFile(renderedDoc);
                            }
                            else if (programme.BaseProgramme.IsPdfDoc)
                            {
                                SystemDocument renderedDoc1 = await _fileService.RenderDocument(user, template, agreement, null, null);
                                renderedDoc = await GetInvoicePDF(renderedDoc1, template.Name);

                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                agreement.Documents.Add(renderedDoc1);
                                documents = renderedDoc;
                                await _fileService.UploadFile(renderedDoc);
                            }
                            else
                            {
                                renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);

                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                renderedDoc.RenderToPDF = template.RenderToPDF;
                                if (programme.BaseProgramme.IsPdfDoc)
                                {
                                    if (renderedDoc.IsTemplate == true)
                                    {
                                        renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
                                        renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
                                    }
                                }
                                agreement.Documents.Add(renderedDoc);
                                documents = renderedDoc;
                                await _fileService.UploadFile(renderedDoc);

                            }

                        } else if (template.DocumentType == 4 && agreement.ClientInformationSheet.Programme.PaymentType == "Credit Card" && programme.BaseProgramme.IsPdfDoc)
                        {
                            SystemDocument renderedDoc1 = await _fileService.RenderDocument(user, template, agreement, null, null);
                            renderedDoc = await GetInvoicePDF(renderedDoc1, template.Name);

                            renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                            agreement.Documents.Add(renderedDoc1);
                            documents = renderedDoc;
                            await _fileService.UploadFile(renderedDoc);

                        } else if (template.DocumentType == 12 && agreement.ClientInformationSheet.Programme.PaymentType == "Invoice" && programme.BaseProgramme.IsPdfDoc)
                        {
                            SystemDocument renderedDoc1 = await _fileService.RenderDocument(user, template, agreement, null, null);
                            renderedDoc = await GetInvoicePDF(renderedDoc1, template.Name);

                            renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                            agreement.Documents.Add(renderedDoc1);
                            documents = renderedDoc;
                            await _fileService.UploadFile(renderedDoc);
                        }

                        //render job certificate
                        if (template.DocumentType == 9 && !programme.BaseProgramme.IsPdfDoc)
                        {
                            if (sheet.Jobs.Where(sj => sj.DateDeleted == null && !sj.Removed).Count() > 0)
                            {
                                foreach (var job in sheet.Jobs.Where(sj => sj.DateDeleted == null && !sj.Removed))
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, job);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    agreement.Documents.Add(renderedDoc);
                                    documents = renderedDoc;
                                    await _fileService.UploadFile(renderedDoc);
                                }
                            }
                        }

                        //render all subsystem
                        if (template.DocumentType == 6)
                        {
                            foreach (var subSystemClient in sheet.SubClientInformationSheets)
                            {
                                if (agreement.Product.IsOptionalProductBasedSub)
                                {
                                    if (subSystemClient.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                                    {
                                        SystemDocument renderedDocSub = await _fileService.RenderDocument(user, template, agreement, subSystemClient, null);
                                        renderedDocSub.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                        agreement.Documents.Add(renderedDocSub);
                                        documents = renderedDocSub;
                                        await _fileService.UploadFile(renderedDocSub);
                                    }
                                }
                                else
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, subSystemClient, null);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    agreement.Documents.Add(renderedDoc);
                                    documents = renderedDoc;
                                    await _fileService.UploadFile(renderedDoc);
                                }

                            }
                        }
                    }


                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return documents;
            }
            return documents;

        }

        [HttpPost]
        public async Task<IActionResult> RerenderSelectedTemplate(string TemplateName, Guid ProgrammeId)
        {
            //SystemDocument template = await _documentRepository.GetByIdAsync(TemplateId);
            //Programme programme = await _programmeService.GetProgrammeById(ProgrammeId);
            User user = await CurrentUser();
            List<Document> documents = null;
            List<SystemDocument> agreeTemplateList = null;
            List<ClientAgreement> clientagreements = null;
            List<ClientProgramme> ClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(ProgrammeId);
            try
            {
                foreach (ClientProgramme programme in ClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    clientagreements = programme.Agreements.ToList(); ;
                    foreach (ClientAgreement agreement in clientagreements.Where(agree => agree.DateDeleted == null))
                    {
                        agreeTemplateList = agreement.Product.Documents.Where(doc => doc.DocumentType != 10 && doc.DateDeleted == null).ToList();


                        //var templatetype = agreement.Documents.Where(doc => doc.Name == TemplateName);
                        foreach (SystemDocument templatetypes in agreeTemplateList)
                        {
                            documents.Add(await RerenderTemplate(templatetypes, agreement, programme));

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

            var url = "/Agreement/RerenderDocs/?ProgrammeId=" + "" + ProgrammeId;
            //return Redirect("/Agreement/RerenderDocs/" + ProgrammeId);
            return Json(new { url });

        }


        [HttpPost]
        public async Task<IActionResult> RerenderSelectedTemplatebyClientProgId(string TemplateName, Guid ProgrammeId, Guid ClientProgId)
        {
            //SystemDocument template = await _documentRepository.GetByIdAsync(TemplateId);
            //Programme programme = await _programmeService.GetProgrammeById(ProgrammeId);
            User user = await CurrentUser();
            List<Document> documents = null;
            List<SystemDocument> agreeTemplateList = null;
            List<ClientAgreement> clientagreements = null;
            ClientProgramme ClientProgramme = await _programmeService.GetClientProgrammebyId(ClientProgId);
            try
            {
                clientagreements = ClientProgramme.Agreements.ToList(); ;
                foreach (ClientAgreement agreement in clientagreements.Where(agree => agree.Id == Guid.Parse("5cc65c22-9749-4d2e-80c2-ad43016ea7b3")))
                {
                    agreeTemplateList = agreement.Documents.Where(doc => doc.Name == TemplateName && doc.DateDeleted == null).ToList();


                    //var templatetype = agreement.Documents.Where(doc => doc.Name == TemplateName);
                    foreach (SystemDocument templatetypes in agreeTemplateList)
                    {
                        documents.Add(await RerenderTemplate(templatetypes, agreement, ClientProgramme));

                    }
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

            var url = "/Agreement/RerenderDocs/?ProgrammeId=" + "" + ProgrammeId;
            //return Redirect("/Agreement/RerenderDocs/" + ProgrammeId);
            return Json(new { url });

        }



        [HttpPost]
        public async Task<IActionResult> RerenderOtherType(string TemplateName, Guid ProgrammeId)
        {
            //SystemDocument template = await _documentRepository.GetByIdAsync(TemplateId);
            //Programme programme = await _programmeService.GetProgrammeById(ProgrammeId);
            User user = await CurrentUser();
            List<Document> documents = null;
            List<SystemDocument> agreeTemplateList = null;
            List<ClientAgreement> clientagreements = null;
            List<ClientProgramme> ClientProgrammes = await _programmeService.GetClientProgrammesForProgramme(ProgrammeId);
            try
            {
                foreach (ClientProgramme programme in ClientProgrammes.OrderBy(cp => cp.DateCreated).OrderBy(cp => cp.Owner.Name))
                {
                    clientagreements = programme.Agreements.ToList(); ;
                    foreach (ClientAgreement agreement in clientagreements)
                    {
                        agreeTemplateList = agreement.Documents.Where(doc => doc.Name == TemplateName).ToList();


                        //var templatetype = agreement.Documents.Where(doc => doc.Name == TemplateName);
                        if (programme.BaseProgramme.EnableFullProposalReport)
                            if (true)
                            {
                                foreach (SystemDocument templatetypes in agreeTemplateList.Where(doc => doc.Name == "Information Sheet Report"))
                                {
                                    documents.Add(await RerenderTemplate(templatetypes, agreement, programme));
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

            var url = "/Agreement/RerenderDocs/?ProgrammeId=" + "" + ProgrammeId;
            //return Redirect("/Agreement/RerenderDocs/" + ProgrammeId);
            return Json(new { url });

        }

        [HttpPost]
        public async Task<IActionResult> ByPassPayment(IFormCollection collection)
        {
            Guid sheetId = Guid.Empty;
            ClientInformationSheet sheet = null;
            User user = null;
            var Action = HttpContext.Request.Form["BindAgreement"];
            string bindnotes = HttpContext.Request.Form["BindNotes"];

            try
            {
                if (Guid.TryParse(HttpContext.Request.Form["AnswerSheetId"], out sheetId))
                {
                    sheet = await _customerInformationService.GetInformation(sheetId);
                }

                ClientProgramme programme = sheet.Programme;
                user = await CurrentUser();
                var status = "Bound";
                if (sheet.Programme.BaseProgramme.UsesEGlobal)
                {
                    status = "Bound and invoice pending";
                }

                //CommonRenderDocs(programme.BaseProgramme.Id, Action, status, sheet);
                foreach (ClientAgreement agreement in programme.Agreements)
                {
                    if (agreement.Status == "Quoted")
                    {
                        if (Action == "BindAgreement")
                        {
                            agreement.BindNotes = HttpContext.Request.Form["BindNotes"];
                            agreement.BindByUserID = user;
                        }

                        if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null && acagreement.Bound).Count() > 0)
                        {
                            var allDocs = await _fileService.GetDocumentByOwner(programme.Owner);
                            var documents = new List<SystemDocument>();
                            var documentspremiumadvice = new List<SystemDocument>();
                            var agreeTemplateList = agreement.Product.Documents;
                            var agreeDocList = agreement.GetDocuments();

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                if (agreement.Status != status)
                                {
                                    agreement.Status = status;
                                    agreement.BoundDate = DateTime.Now;
                                    if (programme.BaseProgramme.PolicyNumberPrefixString != null)//programme PolicyNumberPrefixString 
                                    {
                                        agreement.PolicyNumber = programme.BaseProgramme.PolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    if (agreement.Product.ProductPolicyNumberPrefixString != null)//product PolicyNumberPrefixString
                                    {
                                        agreement.PolicyNumber = agreement.Product.ProductPolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    await uow.Commit();
                                }
                            }

                            agreement.Status = status;

                            foreach (SystemDocument doc in agreeDocList)
                            {
                                // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                                if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                                {
                                    doc.Delete(user);
                                }
                            }

                            //tripleA DO use case, remove when all client set as company
                            if (agreement.Product.Id == new Guid("bdbdda02-ee4e-44f5-84a8-dd18d17287c1") &&
                                agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDAOLIOptions").First().Value == "2")
                            {

                            }
                            else
                            {

                                if (!agreement.Product.IsOptionalCombinedProduct)
                                {
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType != 7))
                                    {
                                        documents.Add(await RerenderTemplate(template, agreement, programme));

                                    }
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType == 7))
                                    {
                                        documentspremiumadvice.Add(await RerenderTemplate(template, agreement, programme));

                                    }



                                    if (programme.BaseProgramme.ProgEnableEmail)
                                    {
                                        if (!programme.BaseProgramme.ProgStopPolicyDocAutoRelease)
                                        {
                                            //send out policy document email
                                            EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == "SendPolicyDocuments");
                                            if (emailTemplate != null)
                                            {
                                                await _emailService.SendEmailViaEmailTemplate(programme.Owner.Email, emailTemplate, documents, agreement.ClientInformationSheet, agreement);

                                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                                {
                                                    if (!agreement.IsPolicyDocSend)
                                                    {
                                                        agreement.IsPolicyDocSend = true;
                                                        agreement.DocIssueDate = DateTime.Now;
                                                        await uow.Commit();
                                                    }
                                                }
                                            }
                                        }


                                        //send out premium advice
                                        if (programme.BaseProgramme.ProgEnableSendPremiumAdvice && !string.IsNullOrEmpty(programme.BaseProgramme.PremiumAdviceRecipent) &&
                                            agreement.Product.ProductEnablePremiumAdvice)
                                        {
                                           await _emailService.SendPremiumAdviceEmail(programme.BaseProgramme.PremiumAdviceRecipent, documentspremiumadvice, agreement.ClientInformationSheet, agreement, programme.BaseProgramme.PremiumAdviceRecipentCC);
                                        }

                                        //send out agreement bound notification email
                                        await _emailService.SendSystemEmailAgreementBoundNotify(programme.BrokerContactUser, programme.BaseProgramme, agreement, programme.Owner);
                                    }

                                }

                            }

                        }

                        else
                        {
                            agreement.DateDeleted = DateTime.Now;
                        }

                    }


                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (programme.InformationSheet.Status != status)
                    {
                        programme.InformationSheet.Status = status;
                        uow.Commit();
                    }
                }

                if (Action == "BindAgreement")
                {
                    return Redirect("/Agreement/ViewAcceptedAgreement/" + programme.Id);
                }
                else
                {
                    var url = "/Agreement/ViewAcceptedAgreement/" + programme.Id;
                    return Json(new { url });
                }

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> SendPolicyDocuments(Guid id, bool sendUser)
        {
            User user = null;
            try
            {
                ClientInformationSheet sheet = await _customerInformationService.GetInformation(id);
                user = await CurrentUser();
                var progid = sheet.Programme.Id;
                // TODO - rewrite to save templates on a per programme basis
                await RerenderClientProgrammes(sheet.Programme, "SendPolicyDocuments", null, null, false, sendUser);
                return Redirect("/Agreement/ViewAcceptedAgreement/" + progid);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> SendFullProposalReport(Guid id, bool sendUser)
        {
            User user = null;
            try
            {
                ClientInformationSheet sheet = await _customerInformationService.GetInformation(id);
                user = await CurrentUser();
                // TODO - rewrite to save templates on a per programme basis

                ClientProgramme programme = sheet.Programme;
                foreach (ClientAgreement agreement in programme.Agreements)
                {
                    if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null).Count() > 0)
                    {
                        var allDocs = await _fileService.GetDocumentByOwner(programme.Owner);
                        var document = new SystemDocument();
                        var documentspremiumadvice = new List<SystemDocument>();
                        var agreeTemplateList = agreement.Product.Documents;
                        var agreeDocList = agreement.GetDocuments();

                        foreach (SystemDocument doc in agreeDocList)
                        {
                            // Keep going until you find a HTML Information Sheet Report and convert it to PDF Information Sheet
                            if (doc.Name.EqualsIgnoreCase("Information Sheet Report") && programme.BaseProgramme.EnableFullProposalReport)
                            {
                                SystemDocument renderedDoc = await GetPdfDocument(doc.Id, programme);
                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                document = renderedDoc;
                                await _fileService.UploadFile(renderedDoc);
                            }
                        }

                        if (programme.BaseProgramme.ProgEnableEmail && agreement.MasterAgreement)
                        {
                            //send out policy document email
                            if (programme.BaseProgramme.EnableFullProposalReport)
                            {
                                if (sendUser)
                                {
                                    if (programme.BaseProgramme.FullProposalReportRecipent != null)
                                    {
                                        await _emailService.SendFullProposalReport(programme.BaseProgramme.FullProposalReportRecipent, document, agreement.ClientInformationSheet, agreement, null);
                                    }
                                    else
                                    {
                                        await _emailService.SendFullProposalReport(programme.BaseProgramme.BrokerContactUser.Email, document, agreement.ClientInformationSheet, agreement, null);
                                    }
                                }
                                else
                                {
                                    await _emailService.SendFullProposalReport(programme.Owner.Email, document, agreement.ClientInformationSheet, agreement, null);
                                }
                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    if (!agreement.IsFullProposalDocSend)
                                    {
                                        agreement.IsFullProposalDocSend = true;
                                        agreement.DocIssueDate = DateTime.Now;
                                        await uow.Commit();
                                    }
                                }
                            }
                        }
                    }
                }

                return Redirect("/Agreement/ViewAcceptedAgreement/" + programme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> SendInvoice(Guid id, bool sendUser)
        //{
        //    User user = null;
        //    try
        //    {
        //        ClientInformationSheet sheet = await _customerInformationService.GetInformation(id);
        //        user = await CurrentUser();
        //        // TODO - rewrite to save templates on a per programme basis

        //        ClientProgramme programme = sheet.Programme;
        //        foreach (ClientAgreement agreement in programme.Agreements)
        //        {
        //            if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null).Count() > 0)
        //            {
        //                var allDocs = await _fileService.GetDocumentByOwner(programme.Owner);
        //                var document = new SystemDocument();
        //                var documentspremiumadvice = new List<SystemDocument>();
        //                var agreeTemplateList = agreement.Product.Documents;
        //                var agreeDocList = agreement.GetDocuments();

        //                foreach (SystemDocument doc in agreeDocList)
        //                {
        //                    if (doc.Name.Contains("Invoice"))
        //                    {
        //                        SystemDocument renderedDoc = await GetInvoicePDF(doc.Id,  doc.Name);
        //                        renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
        //                        document = renderedDoc;
        //                        // documents.Add(renderedDoc);
        //                        await _fileService.UploadFile(renderedDoc);
        //                    }
        //                }


        //                if (programme.BaseProgramme.ProgEnableEmail && document.Name.Contains("Invoice"))
        //                {
        //                    //send out policy document email

        //                        if (sendUser)
        //                        {
        //                            await _emailService.GetInvoicePDF(user.Email, document, agreement.ClientInformationSheet, agreement, null);
        //                        }
        //                        else
        //                        {
        //                            await _emailService.GetInvoicePDF(programme.Owner.Email, document, agreement.ClientInformationSheet, agreement, null);
        //                        }
        //                        using (var uow = _unitOfWork.BeginUnitOfWork())
        //                        {
        //                            if (!agreement.IsFullProposalDocSend)
        //                            {
        //                                agreement.IsFullProposalDocSend = true;
        //                                agreement.DocIssueDate = DateTime.Now;
        //                                await uow.Commit();
        //                            }
        //                        }

        //                }
        //            }
        //        }

        //        return Redirect("/Agreement/ViewAcceptedAgreement/" + programme.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //        return RedirectToAction("Error500", "Error");
        //    }
        //}

        //public async Task<Document> ConvertHTMLToPDF(Document doc)
        //{
        //    //ClientProgramme clientprogramme = await _programmeService.GetClientProgrammebyId(ClientProgrammeId);
        //    //ClientInformationSheet clientInformationSheet = clientprogramme.InformationSheet;

        //    //SystemDocument doc = await _documentRepository.GetByIdAsync(DocumentId);
        //    // DOCX & HTML
        //    string html = _fileService.FromBytes(doc.Contents);
        //    var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
        //    htmlToPdfConv.License.SetLicenseKey(
        //       _appSettingService.NRecoUserName,
        //       _appSettingService.NRecoLicense
        //   );            // for Linux/OS-X: "wkhtmltopdf"
        //    htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
        //    htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;
        //    //htmlToPdfConv.PageHeaderHtml = "<p style='padding-top: 60px'>"
        //    //    + "</br><strong> Title:" + clientprogramme.BaseProgramme.Name + "</strong></br>"
        //    //    + " <strong> Information Sheet for :" + clientprogramme.Owner.Name + "</strong></br>"
        //    //    + " <strong> UIS No:" + clientInformationSheet.ReferenceId + "</strong></br>"
        //    //    + " <strong> Sheet Submitted On:" + clientInformationSheet.SubmitDate + "</strong></br>"
        //    //    + " <strong> Report Generated On:" + DateTime.Now + "</strong></br>"
        //    //    + " <strong> Issued To:" + clientInformationSheet.SubmittedBy.FullName + "</strong></br>"
        //    //    + "<h2> </br>  </h2> </p>";

        //    //htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";

        //    var margins = new PageMargins();
        //    margins.Bottom = 18;
        //    margins.Top = 38;
        //    margins.Left = 15;
        //    margins.Right = 15;
        //    htmlToPdfConv.Margins = margins;

        //    // Legacy Image Path Fix
        //    string badURL = "../../../images/";
        //    var newURL = "https://" + _appSettingService.domainQueryString + "/Image/";
        //    html = html.Replace(badURL, newURL);

        //    var pdfBytes = htmlToPdfConv.GeneratePdf(html);
        //    doc.Contents = pdfBytes;
        //    return doc;
        //}
        [HttpGet]
        public async Task<Document> GetPdfDocument(Guid id, ClientProgramme clientprogramme)
        {
            User user = null;

            SystemDocument doc = await _documentRepository.GetByIdAsync(id);
            string html = _fileService.FromBytes(doc.Contents);
            html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /><style>img { width: 120px; height:120px}</style></head>");
            var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
            htmlToPdfConv.License.SetLicenseKey(
              _appSettingService.NRecoUserName,
              _appSettingService.NRecoLicense
            );            // for Linux/OS-X: "wkhtmltopdf"
            if (_appSettingService.IsLinuxEnv == "True")
            {
                htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
            }
            htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;          // for Linux/OS-X: "wkhtmltopdf"

            string submittedBy = clientprogramme.InformationSheet.SubmittedBy.FullName;
            if (clientprogramme.InformationSheet.SubmittedBy.PrimaryOrganisation != null)
            {
                if (clientprogramme.InformationSheet.SubmittedBy.PrimaryOrganisation.IsTC)
                {
                    submittedBy = clientprogramme.InformationSheet.Programme.BrokerContactUser.FullName;
                }
            }

            htmlToPdfConv.PageHeaderHtml = "<p style='padding-top: 60px'>"
               + "</br><strong> Title: " + clientprogramme.BaseProgramme.Name + "</strong></br>"
               + " <strong> Information Sheet for : " + clientprogramme.Owner.Name + "</strong></br>"
               + " <strong> UIS No: " + clientprogramme.InformationSheet.ReferenceId + "</strong></br>"
               + " <strong> Sheet Submitted On: " + clientprogramme.InformationSheet.SubmitDate.ToShortDateString() + "</strong></br>"
               + " <strong> Report Generated On: " + DateTime.Now.ToShortDateString() + "</strong></br>"
               + " <strong> Submitted By: " + submittedBy + "</strong></br>"
               + "<h2> </br>  </h2> </p>";

            htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";

            var margins = new PageMargins();
            margins.Bottom = 18;
            margins.Top = 38;
            margins.Left = 15;
            margins.Right = 15;
            htmlToPdfConv.Margins = margins;
            var pdfBytes = htmlToPdfConv.GeneratePdf(html);
            Document document = new Document(user, "Information Sheet Report", "application/pdf", 99);
            document.Contents = pdfBytes;
            return document;
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

            //html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /><style>img { width: 120px; height:120px}</style></head>");
            // Test if the below 4 are even necessary by this function, setting above should make these redundant now
            html = html.Replace("“", "&quot");
            html = html.Replace("”", "&quot");
            html = html.Replace(" – ", "--");
            html = html.Replace("&nbsp;", " ");
            html = html.Replace("’", "&#146");
            html = html.Replace("‘", "&#39");

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
            Document document = new Document(user, invoicename, "application/pdf", renderedDoc.DocumentType);
            document.Contents = pdfBytes;
            return document;
        }
        [HttpPost]
        public async Task<IActionResult> SendPolicyDocuments(EmailTemplateViewModel model)
        {
            User user = null;
            try
            {
                ClientProgramme programme = await _programmeService.GetClientProgrammebyId(model.ClientProgrammeID);
                user = await CurrentUser();
                // TODO - rewrite to save templates on a per programme basis
                ClientAgreement agreement = programme.Agreements[0];
                //EmailTemplate emailTemplate = agreement.Product.EmailTemplates.FirstOrDefault (et => et.Type == "SendPolicyDocuments");
                EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == "SendPolicyDocuments");

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
                        emailTemplate = new EmailTemplate(user, "Agreement Documents Covering Text", "SendPolicyDocuments", model.Subject, model.Body, null, programme.BaseProgramme);
                        programme.BaseProgramme.EmailTemplates.Add(emailTemplate);
                        if (!agreement.IsPolicyDocSend)
                        {
                            agreement.IsPolicyDocSend = true;
                            agreement.DocIssueDate = DateTime.Now;
                        }
                        await uow.Commit();
                    }
                }

                var docs = agreement.GetDocuments();
                var documents = new List<SystemDocument>();

                if (docs != null)
                {
                    foreach (SystemDocument doc in docs)
                    {
                        if (doc.DateDeleted == null)
                        {
                            documents.Add(doc);
                        }
                    }
                }
                else
                {

                    documents = null;
                }

                string strrecipentemail = null;
                if (model.Recipent != null)
                {
                    var userDb = await _userService.GetUserById(model.Recipent);
                    strrecipentemail = user.Email;
                }

                //await _emailService.SendEmailViaEmailTemplate(strrecipentemail, emailTemplate, documents, null, null);

                return Redirect("~/Home/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePxPayment(IFormCollection collection)
        {
            Guid sheetId = Guid.Empty;
            ClientInformationSheet sheet = null;
            User user = null;
            var paymentype = HttpContext.Request.Form["AnswerSheetId"];
            try
            {
                if (Guid.TryParse(HttpContext.Request.Form["AnswerSheetId"], out sheetId))
                {
                    sheet = await _customerInformationService.GetInformation(sheetId);
                }

                ClientProgramme programme = sheet.Programme;
                programme.PaymentType = "Credit Card";

                //var active = _httpClientService.GetEglobalStatus().Result;

                //Hardcoded variables
                decimal totalPremium = 0, totalPayment, brokerFee = 0, GST = 1.15m, creditCharge = 1.015m;
                Merchant merchant = await _merchantService.GetMerchant(programme.BaseProgramme.Id);
                Payment payment = await _paymentService.GetPayment(programme.Id);
                if (payment == null)
                {
                    payment = await _paymentService.AddNewPayment(sheet.CreatedBy, programme, merchant, merchant.MerchantPaymentGateway);
                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    programme.PaymentType = "Credit Card";
                    programme.Payment = payment;
                    //programme.InformationSheet.Status = "Bound";
                    await uow.Commit();
                }

                //add check to count how many failed payments
                var ProgrammeId = sheetId;
                foreach (ClientAgreement clientAgreement in programme.Agreements)
                {
                    ProgrammeId = programme.Id;
                    brokerFee += clientAgreement.BrokerFee;
                    var terms = await _clientAgreementTermService.GetAllAgreementTermFor(clientAgreement);
                    foreach (ClientAgreementTerm clientAgreementTerm in terms.Where(agree => agree.Bound == true && agree.DateDeleted == null))
                    {
                        if (programme.InformationSheet.IsChange && programme.InformationSheet.PreviousInformationSheet != null)
                        {
                            totalPremium += clientAgreementTerm.PremiumDiffer;
                        }
                        else
                        {
                            totalPremium += clientAgreementTerm.Premium;
                        }

                        foreach (ClientAgreementTermExtension extension in clientAgreementTerm.ClientAgreement.ClientAgreementTermExtensions.Where(ext => ext.Bound == true))
                        {
                            totalPremium += extension.Premium;

                        }

                    }
                }
                //totalPayment = Math.Round(((totalPremium + brokerFee) * (GST) * (creditCharge)), 2);
                totalPayment = Math.Round(((totalPremium + brokerFee) * (GST)), 2); //for Marsh's merchant only pass in the company premium

                PxPay pxPay = new PxPay(merchant.MerchantPaymentGateway.PaymentGatewayWebServiceURL, merchant.MerchantPaymentGateway.PxpayUserId, merchant.MerchantPaymentGateway.PxpayKey);

                string domainQueryString = _appSettingService.domainQueryString;

                RequestInput input = new RequestInput
                {
                    AmountInput = totalPayment.ToString("0.00"),
                    CurrencyInput = "NZD",
                    TxnType = "Purchase",
                    UrlFail = "https://" + domainQueryString + payment.PaymentPaymentGateway.PxpayUrlFail + ProgrammeId.ToString(),
                    UrlSuccess = "https://" + domainQueryString + payment.PaymentPaymentGateway.PxpayUrlSuccess + ProgrammeId.ToString(),
                    TxnId = payment.Id.ToString("N").Substring(0, 16),
                };

                RequestOutput requestOutput = pxPay.GenerateRequest(input);
                //opens on same page - hard to return back to current process
                return Json(new { url = requestOutput.Url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ResumeGeneratePxPayment(Guid id)
        {
            Guid sheetId = Guid.Empty;
            ClientInformationSheet sheet = null;
            User user = null;
            try
            {
                sheet = await _customerInformationService.GetInformation(id);

                ClientProgramme programme = sheet.Programme;
                programme.PaymentType = "Credit Card";

                //var active = _httpClientService.GetEglobalStatus().Result;

                //Hardcoded variables
                decimal totalPremium = 0, totalPayment, brokerFee = 0, GST = 1.15m, creditCharge = 1.015m;
                Merchant merchant = await _merchantService.GetMerchant(programme.BaseProgramme.Id);
                Payment payment = await _paymentService.GetPayment(programme.Id);
                if (payment == null)
                {
                    payment = await _paymentService.AddNewPayment(sheet.CreatedBy, programme, merchant, merchant.MerchantPaymentGateway);
                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    programme.PaymentType = "Credit Card";
                    programme.Payment = payment;
                    //programme.InformationSheet.Status = "Bound";
                    await uow.Commit();
                }

                //add check to count how many failed payments
                var ProgrammeId = sheetId;
                foreach (ClientAgreement clientAgreement in programme.Agreements)
                {
                    ProgrammeId = programme.Id;
                    brokerFee += clientAgreement.BrokerFee;
                    var terms = await _clientAgreementTermService.GetAllAgreementTermFor(clientAgreement);
                    foreach (ClientAgreementTerm clientAgreementTerm in terms)
                    {
                        if (programme.InformationSheet.IsChange && programme.InformationSheet.PreviousInformationSheet != null)
                        {
                            totalPremium += clientAgreementTerm.PremiumDiffer;
                        }
                        else
                        {
                            totalPremium += clientAgreementTerm.Premium;
                        }

                    }
                }
                totalPayment = Math.Round(((totalPremium + brokerFee) * (GST) * (creditCharge)), 2);

                PxPay pxPay = new PxPay(merchant.MerchantPaymentGateway.PaymentGatewayWebServiceURL, merchant.MerchantPaymentGateway.PxpayUserId, merchant.MerchantPaymentGateway.PxpayKey);

                string domainQueryString = _appSettingService.domainQueryString;
                Guid progid = Guid.NewGuid();
                RequestInput input = new RequestInput
                {
                    AmountInput = totalPayment.ToString("0.00"),
                    CurrencyInput = "NZD",
                    TxnType = "Purchase",
                    UrlFail = "https://" + domainQueryString + payment.PaymentPaymentGateway.PxpayUrlFail + ProgrammeId.ToString(),
                    UrlSuccess = "https://" + domainQueryString + payment.PaymentPaymentGateway.PxpayUrlSuccess + ProgrammeId.ToString(),
                    TxnId = progid.ToString("N").Substring(0, 16),
                };

                RequestOutput requestOutput = pxPay.GenerateRequest(input);
                //opens on same page - hard to return back to current process
                return Redirect(requestOutput.Url);
                //return Json(new { url = requestOutput.Url });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> GenerateEGlobal(IFormCollection collection)
        {
            ClientInformationSheet sheet = null;
            User user = null;
            try
            {
                user = await CurrentUser();
                //throw new Exception("Method will need to be re-written");
                if (Guid.TryParse(HttpContext.Request.Form["AnswerSheetId"], out Guid sheetId))
                {
                    sheet = await _customerInformationService.GetInformation(sheetId);
                }
                var status = "Bound";
                if (sheet.Programme.BaseProgramme.UsesEGlobal)
                {
                    status = "Bound and invoice pending";
                }

                //Hardcoded variables
                ClientProgramme programme = sheet.Programme;
                var eGlobalSerializer = new EGlobalSerializerAPI();

                string paymentType = "Invoice";
                Guid transactionreferenceid = Guid.NewGuid();

                //check Eglobal parameters
                if (string.IsNullOrEmpty(programme.EGlobalClientNumber))
                {
                    //throw new Exception(nameof(programme.EGlobalClientNumber) + " EGlobal client number");

                    //send out notification email
                    await _emailService.SendSystemEmailClientNumberNotify(user, programme.BaseProgramme, programme.InformationSheet, programme.InformationSheet.Owner);

                } else
                {
                    var xmlPayload = eGlobalSerializer.SerializePolicy(programme, user, _unitOfWork, transactionreferenceid, paymentType, false, false, null);

                    var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload);

                    //used for eglobal request and response log
                    if (programme.BaseProgramme.ProgEnableEmail)
                    {
                        await _emailService.EGlobalLogEmail("marshevents@proposalonline.com", transactionreferenceid.ToString(), xmlPayload, byteResponse);
                    }

                    EGlobalSubmission eglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmissionByTransaction(transactionreferenceid);

                    eGlobalSerializer.DeSerializeResponse(byteResponse, programme, user, _unitOfWork, eglobalsubmission);
                }

                //binding the agreements
                //============
                //CommonRenderDocs(programme.BaseProgramme.Id, Action, status, sheet);
                foreach (ClientAgreement agreement in programme.Agreements)
                {
                    if (agreement.Status == "Quoted")
                    {
                        if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null && acagreement.Bound).Count() > 0)
                        {
                            var allDocs = await _fileService.GetDocumentByOwner(programme.Owner);
                            var documents = new List<SystemDocument>();
                            var documentspremiumadvice = new List<SystemDocument>();
                            var agreeTemplateList = agreement.Product.Documents;
                            var agreeDocList = agreement.GetDocuments();

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                if (agreement.Status != status)
                                {
                                    agreement.Status = status;
                                    agreement.BoundDate = DateTime.Now;
                                    if (programme.BaseProgramme.PolicyNumberPrefixString != null)//programme PolicyNumberPrefixString 
                                    {
                                        agreement.PolicyNumber = programme.BaseProgramme.PolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    if (agreement.Product.ProductPolicyNumberPrefixString != null)//product PolicyNumberPrefixString
                                    {
                                        agreement.PolicyNumber = agreement.Product.ProductPolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                    }
                                    programme.PaymentType = paymentType;
                                    await uow.Commit();
                                }
                            }

                            agreement.Status = status;

                            foreach (SystemDocument doc in agreeDocList)
                            {
                                // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                                if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                                {
                                    doc.Delete(user);
                                }
                            }

                            if (!agreement.Product.IsOptionalCombinedProduct)
                            {
                                foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType != 7))
                                {
                                    documents.Add(await RerenderTemplate(template, agreement, programme));

                                }
                                foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType == 7))
                                {
                                    documentspremiumadvice.Add(await RerenderTemplate(template, agreement, programme));

                                }

                                if (programme.BaseProgramme.ProgEnableEmail)
                                {
                                    if (!programme.BaseProgramme.ProgStopPolicyDocAutoRelease)
                                    {
                                        //send out policy document email
                                        EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == "SendPolicyDocuments");
                                        if (emailTemplate != null)
                                        {
                                            await _emailService.SendEmailViaEmailTemplate(programme.Owner.Email, emailTemplate, documents, agreement.ClientInformationSheet, agreement);

                                            using (var uow = _unitOfWork.BeginUnitOfWork())
                                            {
                                                if (!agreement.IsPolicyDocSend)
                                                {
                                                    agreement.IsPolicyDocSend = true;
                                                    agreement.DocIssueDate = DateTime.Now;
                                                    await uow.Commit();
                                                }
                                            }
                                        }
                                    }

                                    //send out premium advice
                                    if (programme.BaseProgramme.ProgEnableSendPremiumAdvice && !string.IsNullOrEmpty(programme.BaseProgramme.PremiumAdviceRecipent) &&
                                        agreement.Product.ProductEnablePremiumAdvice)
                                    {
                                        await _emailService.SendPremiumAdviceEmail(programme.BaseProgramme.PremiumAdviceRecipent, documentspremiumadvice, agreement.ClientInformationSheet, agreement, programme.BaseProgramme.PremiumAdviceRecipentCC);
                                    }

                                    //send out agreement bound notification email
                                    await _emailService.SendSystemEmailAgreementBoundNotify(programme.BrokerContactUser, programme.BaseProgramme, agreement, programme.Owner);
                                }

                            }

                        }

                        else
                        {
                            agreement.DateDeleted = DateTime.Now;
                        }

                    }


                }

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (programme.InformationSheet.Status != status)
                    {
                        programme.InformationSheet.Status = status;
                        uow.Commit();
                    }
                }
                //============

                //update status to bound
                if (programme.ClientAgreementEGlobalResponses.Count > 0)
                {
                    EGlobalResponse eGlobalResponse = programme.ClientAgreementEGlobalResponses.Where(er => er.DateDeleted == null && er.ResponseType == "update").OrderByDescending(er => er.VersionNumber).FirstOrDefault();
                    if (eGlobalResponse != null)
                    {

                        foreach (ClientAgreement agreement in programme.Agreements.Where(cag => cag.DateDeleted == null))
                        {
                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                agreement.Status = "Bound and invoiced";
                                programme.InformationSheet.Status = "Bound and invoiced";

                                uow.Commit();
                            }
                        }
                        //var documents = new List<SystemDocument>();
                        //foreach (ClientAgreement agreement in programme.Agreements)
                        //{
                        //    if (agreement.MasterAgreement && (agreement.ReferenceId == eGlobalResponse.MasterAgreementReferenceID))
                        //    {
                        //        foreach (SystemDocument doc in agreement.Documents.Where(d => d.DateDeleted == null && d.DocumentType == 4))
                        //        {
                        //            // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                        //            if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                        //            {
                        //                doc.Delete(user);
                        //            }
                        //        }
                        //        foreach (SystemDocument template in agreement.Product.Documents)
                        //        {
                        //            //render docs invoice
                        //            if (template.DocumentType == 4)
                        //            {
                        //                SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                        //                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                        //                agreement.Documents.Add(renderedDoc);
                        //                documents.Add(renderedDoc);
                        //                await _fileService.UploadFile(renderedDoc);
                        //            }
                        //        }
                        //    }
                        //}
                    }

                }

                //return Redirect("/Agreement/ViewAcceptedAgreement/" + programme.Id);
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
        public async Task<IActionResult> ProcessRequestConfiguration(Guid Id)
        {
            User user = null;
            try
            {
                string queryString = HttpContext.Request.Query["result"].ToString();
                var status = "Bound";
                user = await CurrentUser();

                ClientProgramme programme = await _programmeService.GetClientProgrammebyId(Id);
                Payment payment = await _paymentService.GetPayment(programme.Id);


                PxPay pxPay = new PxPay(payment.PaymentMerchant.MerchantPaymentGateway.PaymentGatewayWebServiceURL, payment.PaymentMerchant.MerchantPaymentGateway.PxpayUserId, payment.PaymentMerchant.MerchantPaymentGateway.PxpayKey);
                ResponseOutput responseOutput = pxPay.ProcessResponse(queryString.ToString());

                payment.PaymentAttempts += 1;
                payment.CreditCardType = responseOutput.CardName;
                payment.CreditCardNumber = responseOutput.CardNumber;
                payment.IsPaid = responseOutput.Success == "1" ? true : false;
                payment.PaymentAmount = Convert.ToDecimal(responseOutput.AmountSettlement);
                payment.PaymentCurrency = "NZD";
                await _paymentService.Update(payment);

                if (!payment.IsPaid)
                {
                    //Payment failed
                    status = "Bound and pending payment";

                    //Payment failed notification
                    await _emailService.SendSystemPaymentFailConfigEmailUISIssueNotify(user, programme.BaseProgramme, programme.InformationSheet, programme.InformationSheet.Owner);

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

                    }

                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (programme.InformationSheet.Status != status)

                        {
                            programme.InformationSheet.Status = status;
                            await uow.Commit();
                        }
                    }

                    return RedirectToAction("ProcessedAgreements", new { id = Id });

                }
                else
                {
                    //Payment successed notification
                    await _emailService.SendSystemPaymentSuccessConfigEmailUISIssueNotify(user, programme.BaseProgramme, programme.InformationSheet, programme.InformationSheet.Owner);

                    status = "Bound";
                    if (programme.BaseProgramme.UsesEGlobal)
                    {
                        status = "Bound and invoice pending";
                    }

                    var eGlobalSerializer = new EGlobalSerializerAPI();

                    string paymentType = "Credit";
                    Guid transactionreferenceid = Guid.NewGuid();

                    //check Eglobal parameters
                    if (string.IsNullOrEmpty(programme.EGlobalClientNumber))
                    {
                        //throw new Exception(nameof(programme.EGlobalClientNumber) + " EGlobal client number");

                        //send out notification email
                        await _emailService.SendSystemEmailClientNumberNotify(user, programme.BaseProgramme, programme.InformationSheet, programme.InformationSheet.Owner);

                    }
                    else
                    {
                        var xmlPayload = eGlobalSerializer.SerializePolicy(programme, user, _unitOfWork, transactionreferenceid, paymentType, false, false, null);

                        var byteResponse = await _httpClientService.CreateEGlobalInvoice(xmlPayload);

                        //used for eglobal request and response log
                        if (programme.BaseProgramme.ProgEnableEmail)
                        {
                            await _emailService.EGlobalLogEmail("marshevents@proposalonline.com", transactionreferenceid.ToString(), xmlPayload, byteResponse);
                        }

                        EGlobalSubmission eglobalsubmission = await _eGlobalSubmissionService.GetEGlobalSubmissionByTransaction(transactionreferenceid);

                        eGlobalSerializer.DeSerializeResponse(byteResponse, programme, user, _unitOfWork, eglobalsubmission);
                    }

                    //binding the agreements
                    //============
                    //CommonRenderDocs(programme.BaseProgramme.Id, Action, status, sheet);
                    foreach (ClientAgreement agreement in programme.Agreements)
                    {
                        if (agreement.Status == "Quoted")
                        {
                            if (agreement.ClientAgreementTerms.Where(acagreement => acagreement.DateDeleted == null && acagreement.Bound).Count() > 0)
                            {
                                var allDocs = await _fileService.GetDocumentByOwner(programme.Owner);
                                var documents = new List<SystemDocument>();
                                var documentspremiumadvice = new List<SystemDocument>();
                                var agreeTemplateList = agreement.Product.Documents;
                                var agreeDocList = agreement.GetDocuments();

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    if (agreement.Status != status)
                                    {
                                        agreement.Status = status;
                                        agreement.BoundDate = DateTime.Now;
                                        if (programme.BaseProgramme.PolicyNumberPrefixString != null)//programme PolicyNumberPrefixString 
                                        {
                                            agreement.PolicyNumber = programme.BaseProgramme.PolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                        }
                                        if (agreement.Product.ProductPolicyNumberPrefixString != null)//product PolicyNumberPrefixString
                                        {
                                            agreement.PolicyNumber = agreement.Product.ProductPolicyNumberPrefixString + agreement.ClientInformationSheet.ReferenceId;
                                        }
                                        //programme.PaymentType = paymentType;
                                        await uow.Commit();
                                    }
                                }

                                agreement.Status = status;

                                foreach (SystemDocument doc in agreeDocList)
                                {
                                    // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                                    if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                                    {
                                        doc.Delete(user);
                                    }
                                }

                                if (!agreement.Product.IsOptionalCombinedProduct)
                                {
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType != 7))
                                    {
                                        documents.Add(await RerenderTemplate(template, agreement, programme));

                                    }
                                    foreach (SystemDocument template in agreeTemplateList.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10 && atl.DocumentType == 7))
                                    {
                                        documentspremiumadvice.Add(await RerenderTemplate(template, agreement, programme));

                                    }

                                    if (programme.BaseProgramme.ProgEnableEmail)
                                    {
                                        if (!programme.BaseProgramme.ProgStopPolicyDocAutoRelease)
                                        {
                                            //send out policy document email
                                            EmailTemplate emailTemplate = programme.BaseProgramme.EmailTemplates.FirstOrDefault(et => et.Type == "SendPolicyDocuments");
                                            if (emailTemplate != null)
                                            {
                                                await _emailService.SendEmailViaEmailTemplate(programme.Owner.Email, emailTemplate, documents, agreement.ClientInformationSheet, agreement);

                                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                                {
                                                    if (!agreement.IsPolicyDocSend)
                                                    {
                                                        agreement.IsPolicyDocSend = true;
                                                        agreement.DocIssueDate = DateTime.Now;
                                                        await uow.Commit();
                                                    }
                                                }
                                            }
                                        }

                                        //send out premium advice
                                        if (programme.BaseProgramme.ProgEnableSendPremiumAdvice && !string.IsNullOrEmpty(programme.BaseProgramme.PremiumAdviceRecipent) &&
                                            agreement.Product.ProductEnablePremiumAdvice)
                                        {
                                            await _emailService.SendPremiumAdviceEmail(programme.BaseProgramme.PremiumAdviceRecipent, documentspremiumadvice, agreement.ClientInformationSheet, agreement, programme.BaseProgramme.PremiumAdviceRecipentCC);
                                        }

                                        //send out agreement bound notification email
                                        await _emailService.SendSystemEmailAgreementBoundNotify(programme.BrokerContactUser, programme.BaseProgramme, agreement, programme.Owner);
                                    }

                                }

                            }

                            else
                            {
                                agreement.DateDeleted = DateTime.Now;
                            }

                        }


                    }

                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        if (programme.InformationSheet.Status != status)
                        {
                            programme.InformationSheet.Status = status;
                            uow.Commit();
                        }
                    }
                    //============

                    //update status to bound
                    if (programme.ClientAgreementEGlobalResponses.Count > 0)
                    {
                        EGlobalResponse eGlobalResponse = programme.ClientAgreementEGlobalResponses.Where(er => er.DateDeleted == null && er.ResponseType == "update").OrderByDescending(er => er.VersionNumber).FirstOrDefault();
                        if (eGlobalResponse != null)
                        {
                            foreach (ClientAgreement agreement in programme.Agreements.Where(cag => cag.DateDeleted == null))
                            {
                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    agreement.Status = "Bound and invoiced";
                                    programme.InformationSheet.Status = "Bound and invoiced";

                                    uow.Commit();
                                }
                            }
                            //var documents = new List<SystemDocument>();
                            //foreach (ClientAgreement agreement in programme.Agreements)
                            //{
                            //    if (agreement.MasterAgreement && (agreement.ReferenceId == eGlobalResponse.MasterAgreementReferenceID))
                            //    {
                            //        foreach (SystemDocument doc in agreement.Documents.Where(d => d.DateDeleted == null && d.DocumentType == 4))
                            //        {
                            //            // The PDF document will skip rendering so we don't delete it here but all others are getting regenerated so we delete the old ones
                            //            if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0))
                            //            {
                            //                doc.Delete(user);
                            //            }
                            //        }
                            //        foreach (SystemDocument template in agreement.Product.Documents)
                            //        {
                            //            //render docs invoice
                            //            if (template.DocumentType == 4)
                            //            {
                            //                SystemDocument renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                            //                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                            //                agreement.Documents.Add(renderedDoc);
                            //                documents.Add(renderedDoc);
                            //                await _fileService.UploadFile(renderedDoc);
                            //            }
                            //        }
                            //    }
                            //}
                        }

                    }

                    //commoned out need to add a flag to check  if required for programme
                    //string BindType = "";

                    //if (programme.Agreements.Where(a => a.MasterAgreement).FirstOrDefault().ClientInformationSheet.IsChange && programme.Agreements.Where(a => a.MasterAgreement).FirstOrDefault().ClientInformationSheet.PreviousInformationSheet != null)
                    //{
                    //    BindType = "CHANGE";
                    //}
                    //else
                    //{
                    //    BindType = "NEW";
                    //}

                    //Data data = await _dataService.Add(user);
                    //data = await _dataService.Update(data, Id, BindType);
                    //await _dataService.ToJson(data, "Not yet implemented - just pass in empty string is fine.", Id);
                    //if (programme.BaseProgramme.ProgEnableEmail)
                    //{
                    //    await _emailService.SendDataEmail("staff@techcertain.com", data);
                    //    await _emailService.SendDataEmail("Warren.J.Blomquist@marsh.com", data);
                    //}

                    //return RedirectToAction("ProcessedAgreements", new { id = Id });
                    return Redirect("/Agreement/ViewAcceptedAgreement/" + programme.Id);
                }
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProcessedAgreements(Guid id)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                PartialViewResult result = (PartialViewResult)await ViewAgreement(id);
                var models = (BaseListViewModel<ViewAgreementViewModel>)result.Model;
                var agreeDocList = new List<Document>();
                foreach (ViewAgreementViewModel model in models)
                {
                    model.EditEnabled = false;
                    model.Documents = new List<AgreementDocumentViewModel>();

                    ClientProgramme programme = await _programmeService.GetClientProgrammebyId(id);
                    model.InformationSheetId = programme.InformationSheet.Id;
                    model.ClientProgrammeId = id;
                    foreach (ClientAgreement agreement in programme.Agreements)
                    {
                        model.ClientAgreementId = agreement.Id;
                        agreeDocList = agreement.GetDocuments();
                        foreach (Document doc in agreeDocList)
                        {
                            model.Documents.Add(new AgreementDocumentViewModel { DisplayName = doc.Name, Url = "/File/GetDocument/" + doc.Id, RenderToPDF = doc.RenderToPDF });
                        }
                    }
                }
                return View("ViewProccessedAgreementList", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAcceptedAgreement(Guid id)
        {
            User user = null;

            try
            {
                PartialViewResult result = (PartialViewResult)await ViewAgreement(id);
                var models = (BaseListViewModel<ViewAgreementViewModel>)result.Model;
                user = await CurrentUser();
                var agreeDocList = new List<Document>();

                foreach (ViewAgreementViewModel model in models)
                {
                    model.EditEnabled = false;
                    model.Documents = new List<AgreementDocumentViewModel>();
                    model.CurrentUser = user;

                    ClientProgramme programme = await _programmeService.GetClientProgrammebyId(id);
                    model.ClientInformationSheet = programme.InformationSheet;
                    model.InformationSheetId = programme.InformationSheet.Id;
                    model.ProgrammeName = programme.BaseProgramme.Name;
                    model.ProgrammeNamedPartyName = programme.BaseProgramme.NamedPartyUnitName;
                    model.UsesEglobal = programme.BaseProgramme.UsesEGlobal;
                    ViewBag.Ispdfenable = "" + programme.BaseProgramme.EnableFullProposalReport;
                    model.ClientProgrammeId = id;
                    foreach (ClientAgreement agreement in programme.Agreements.Where(a => a.DateDeleted == null && a.InsurerDeclined !=true))
                    {
                        agreeDocList = agreement.GetDocuments();
                        foreach (Document doc in agreeDocList)
                        {
                            if ((!doc.Name.EqualsIgnoreCase("Information Sheet Report") && doc.DocumentType != 8) && !programme.BaseProgramme.IsPdfDoc)
                            {
                                model.Documents.Add(new AgreementDocumentViewModel { DisplayName = doc.Name, Url = "/File/GetDocument/" + doc.Id, ClientAgreementId = agreement.Id, DocType = doc.DocumentType, RenderToPDF = doc.RenderToPDF });
                            }
                            else if (doc.DocumentType == 8)//.Name.Contains("Invoice"))
                            {
                                model.Documents.Add(new AgreementDocumentViewModel { DisplayName = doc.Name + ".pdf", Url = "/File/GetInvoicePDF/" + doc.Id + "?ClientProgrammeId=" + programme.Id + "&invoicename=Invoice", ClientAgreementId = agreement.Id, DocType = doc.DocumentType, RenderToPDF = doc.RenderToPDF });
                            }
                            else if (doc.DocumentType == 7)
                            {
                                model.Documents.Add(new AgreementDocumentViewModel { DisplayName = doc.Name, Url = "/File/GetDocument/" + doc.Id, ClientAgreementId = agreement.Id, DocType = doc.DocumentType, RenderToPDF = doc.RenderToPDF });
                            }
                            else
                            {
                                ViewBag.IsPDFgenerated = "" + agreement.IsPDFgenerated;
                                ViewBag.IsReportSend = "" + agreement.IsFullProposalDocSend;
                                if (programme.BaseProgramme.IsPdfDoc)
                                {
                                    model.Documents.Add(new AgreementDocumentViewModel { DisplayName = doc.Name + ".pdf", Url = "/File/GetInvoicePDF/" + doc.Id + "?ClientProgrammeId=" + programme.Id + "&invoicename=" + doc.Name, ClientAgreementId = agreement.Id, DocType = doc.DocumentType, RenderToPDF = doc.RenderToPDF });

                                }
                            }
                        }
                    }
                }
                ViewBag.Id = id;
                ViewBag.ClientProgrammeId = id;
                ViewBag.IsBroker = user.PrimaryOrganisation.IsBroker;
                ViewBag.IsTC = user.PrimaryOrganisation.IsTC;
                ViewBag.IsInsurer = user.PrimaryOrganisation.IsInsurer;

                ClientProgramme programme1 = await _programmeService.GetClientProgrammebyId(id);
                ViewBag.Sheetstatus = programme1.InformationSheet.Status;
                if (programme1.IsDocsApproved && programme1.BaseProgramme.ProgEnableHidedoctoClient)
                {
                    ViewBag.showDocs = true;
                }
                else
                {
                    ViewBag.showDocs = false;
                }
                return View("ViewAcceptedAgreementList", models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        public async Task<IActionResult> CreateDefaultAgreementRules()
        {
            User user = null;
            try
            {
                var productList = await _productService.GetAllProducts();
                Product product = productList.FirstOrDefault(p => p.IsBaseProduct == false);
                user = await CurrentUser();

                using (IUnitOfWork uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (product != null)
                    {
                        //
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY1CRate", "North Island City Rate for Category 1C", product, "2.75") { OrderNumber = 5 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN1CRate", "North Island Town Rate for Category 1C", product, "2.5") { OrderNumber = 6 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY1CRate", "South Island City Rate for Category 1C", product, "2.225") { OrderNumber = 7 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN1CRate", "South Island Town Rate for Category 1C", product, "2") { OrderNumber = 8 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY1UAPRate", "North Island City Rate for Category 1UAP", product, "4") { OrderNumber = 9 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN1UAPRate", "North Island Town Rate for Category 1UAP", product, "4") { OrderNumber = 10 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY1UAPRate", "South Island City Rate for Category 1UAP", product, "4") { OrderNumber = 11 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN1UAPRate", "South Island Town Rate for Category 1UAP", product, "4") { OrderNumber = 12 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY1PRate", "North Island City Rate for Category 1P", product, "2") { OrderNumber = 13 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN1PRate", "North Island Town Rate for Category 1P", product, "1.5") { OrderNumber = 14 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY1PRate", "South Island City Rate for Category 1P", product, "1.5") { OrderNumber = 15 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN1PRate", "South Island Town Rate for Category 1P", product, "1") { OrderNumber = 16 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY1RRate", "North Island City Rate for Category 1R", product, "4.75") { OrderNumber = 17 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN1RRate", "North Island Town Rate for Category 1R", product, "4.75") { OrderNumber = 18 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY1RRate", "South Island City Rate for Category 1R", product, "4.75") { OrderNumber = 19 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN1RRate", "South Island Town Rate for Category 1R", product, "4.75") { OrderNumber = 20 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY2Rate", "North Island City Rate for Category 2", product, "1.5") { OrderNumber = 21 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN2Rate", "North Island Town Rate for Category 2", product, "1.25") { OrderNumber = 22 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY2Rate", "South Island City Rate for Category 2", product, "1.25") { OrderNumber = 23 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN2Rate", "South Island Town Rate for Category 2", product, "1") { OrderNumber = 24 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITY3Rate", "North Island City Rate for Category 3", product, "1.75") { OrderNumber = 25 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWN3Rate", "North Island Town Rate for Category 3", product, "1.25") { OrderNumber = 26 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITY3Rate", "South Island City Rate for Category 3", product, "1.25") { OrderNumber = 27 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWN3Rate", "South Island Town Rate for Category 3", product, "1") { OrderNumber = 28 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "NICITYSVRate", "North Island City Rate for Category SV", product, "0.25") { OrderNumber = 29 });
                        await _ruleRepository.AddAsync(new Rule(user, "NITOWNSVRate", "North Island Town Rate for Category SV", product, "0.25") { OrderNumber = 30 });
                        await _ruleRepository.AddAsync(new Rule(user, "SICITYSVRate", "South Island City Rate for Category SV", product, "0.25") { OrderNumber = 31 });
                        await _ruleRepository.AddAsync(new Rule(user, "SITOWNSVRate", "South Island Town Rate for Category SV", product, "0.25") { OrderNumber = 32 });
                        ///
                        await _ruleRepository.AddAsync(new Rule(user, "FSLUNDERFee", "FSL Fee for Vehicle under 3.5T", product, "6.08") { OrderNumber = 33 });
                        await _ruleRepository.AddAsync(new Rule(user, "FSLOVER3Rate", "FSL Rate for Vehicle over 3.5T", product, "0.076") { OrderNumber = 34 });
                        await _ruleRepository.AddAsync(new Rule(user, "FSLUNDERFeeAfter1July", "FSL Fee for Vehicle under 3.5T After 1 July", product, "6.08") { OrderNumber = 35 });
                        await _ruleRepository.AddAsync(new Rule(user, "FSLOVER3RateAfter1July", "FSL Rate for Vehicle over 3.5T After 1 July", product, "0.076") { OrderNumber = 36 });
                        await _ruleRepository.AddAsync(new Rule(user, "PaymentPremium", "Premium Payment", product, "Monthly") { OrderNumber = 40 });

                        await uow.Commit();
                    }
                }

                return Redirect("~/Home/Index");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        public async Task<IActionResult> VerifyDocuments(Guid id)
        {
            User user = null;
            try
            {
                if (id == Guid.Empty)
                    throw new ArgumentNullException(nameof(id));
                ClientInformationSheet answerSheet = await _customerInformationService.GetInformation(id);
                if (answerSheet == null)
                    throw new Exception(string.Format("VerifyDocuments: No Answer Sheet found for [{0}]", id));
                ClientProgramme clientProgramme = answerSheet.Programme;
                if (clientProgramme == null)
                    throw new Exception(string.Format("VerifyDocuments: No Client Programme found for information sheet [{0}]", id));

                user = await CurrentUser();
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    clientProgramme.IsDocsApproved = true;
                    await uow.Commit();
                }
                return Redirect("/Agreement/ViewAcceptedAgreement/" + clientProgramme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        //        public async void Renderdocs1(ClientAgreement agreement , ClientInformationSheet answerSheet)
        //        {
        //            User user = null;
        //            user = await CurrentUser();
        //            var agreeDocList = new List<Document>();
        //            Document renderedDoc;
        //            agreeDocList = agreement.GetDocuments();
        //            foreach (Document doc in agreeDocList)
        //            {
        //                // The PDF document will skip rendering so we don't delete the old document here (as re-rending will make new doc) and all others are getting regenerated so we delete the old ones
        //                if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0) && !(doc.DocumentType == 99))
        //                {
        //                    doc.Delete(user);
        //                }
        //            }

        //            if (agreement.Product.Id == new Guid("bdbdda02-ee4e-44f5-84a8-dd18d17287c1") &&
        //                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDAOLIOptions").First().Value == "2")
        //            {

        //            }
        //            else
        //            {

        //                if (!agreement.Product.IsOptionalCombinedProduct)
        //                {
        //                    foreach (Document template in agreement.Product.Documents)
        //                    {
        //                        if (template.DocumentType == 6)
        //                        {
        //<<<<<<< HEAD
        //                            foreach (var subsheet in agreement.ClientInformationSheet.SubClientInformationSheets)
        //=======
        //                            foreach (Document template in agreement.Product.Documents.Where(apd => apd.DateDeleted == null && apd.DocumentType != 10))
        //>>>>>>> 47a043fb023b9bb1e37b13b9aef4e27db8e42699
        //                            {
        //                                if (agreement.Product.IsOptionalProductBasedSub)
        //                                {
        //                                    if (subsheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
        //                                    {
        //                                        renderedDoc = await _fileService.RenderDocument(user, template, agreement, subsheet, null);
        //                                        renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
        //                                        agreement.Documents.Add(renderedDoc);
        //                                        await _fileService.UploadFile(renderedDoc);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, subsheet, null);
        //                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
        //<<<<<<< HEAD
        //=======
        //                                    renderedDoc.RenderToPDF = template.RenderToPDF;
        //                                    if (answerSheet.Programme.BaseProgramme.IsPdfDoc)
        //                                    {
        //                                        if (renderedDoc.IsTemplate == true)
        //                                        {
        //                                            renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
        //                                            renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
        //                                        }
        //                                    }
        //>>>>>>> 47a043fb023b9bb1e37b13b9aef4e27db8e42699
        //                                    agreement.Documents.Add(renderedDoc);
        //                                    await _fileService.UploadFile(renderedDoc);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
        //                            renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
        //                            renderedDoc.RenderToPDF = template.RenderToPDF;
        //                            if (answerSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG Programme" || answerSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG ML Programme" ||
        //                                 answerSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG Run Off Programme")
        //                            {
        //                                if (renderedDoc.IsTemplate == true)
        //                                {
        //                                    renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
        //                                    renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
        //                                }
        //                            }
        //                            agreement.Documents.Add(renderedDoc);
        //                            await _fileService.UploadFile(renderedDoc);
        //                        }
        //                    }
        //                }
        //            }

        //        }

        public async Task<IActionResult> RenderDocuments(Guid id)
        {
            User user = null;
            try
            {
                if (id == Guid.Empty)
                    throw new ArgumentNullException(nameof(id));
                ClientInformationSheet answerSheet = await _customerInformationService.GetInformation(id);
                if (answerSheet == null)
                    throw new Exception(string.Format("RenderDocuments: No Answer Sheet found for [{0}]", id));
                ClientProgramme clientProgramme = answerSheet.Programme;
                if (clientProgramme == null)
                    throw new Exception(string.Format("RenderDocuments: No Client Programme found for information sheet [{0}]", id));
                //ClientAgreement agreement = answerSheet.ClientAgreement;
                //if (agreement == null)
                //	throw new Exception (string.Format ("No Information found for {0}", id));
                user = await CurrentUser();
                var agreeDocList = new List<Document>();
                Document renderedDoc;
                foreach (ClientAgreement agreement in clientProgramme.Agreements)
                {
                    agreeDocList = agreement.GetDocuments();
                    foreach (Document doc in agreeDocList)
                    {
                        // The PDF document will skip rendering so we don't delete the old document here (as re-rending will make new doc) and all others are getting regenerated so we delete the old ones
                        if (!(doc.Path != null && doc.ContentType == "application/pdf" && doc.DocumentType == 0) && !(doc.DocumentType == 99))
                        {
                            doc.Delete(user);
                        }
                    }

                    if (agreement.Product.Id == new Guid("bdbdda02-ee4e-44f5-84a8-dd18d17287c1") &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "DAOLIViewModel.HasDAOLIOptions").First().Value == "2")
                    {

                    }
                    else
                    {

                        if (!agreement.Product.IsOptionalCombinedProduct && agreement.DateDeleted == null &&
                (agreement.Status == "Bound and pending payment" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced" || agreement.Status == "Bound"))
                        {
                            foreach (Document template in agreement.Product.Documents.Where(atl => atl.DateDeleted == null && atl.DocumentType != 10))
                            {
                                if (template.DocumentType == 6)
                                {
                                    foreach (var subsheet in agreement.ClientInformationSheet.SubClientInformationSheets)
                                    {
                                        if (agreement.Product.IsOptionalProductBasedSub)
                                        {
                                            if (subsheet.Answers.Where(sa => sa.ItemName == agreement.Product.OptionalProductRequiredAnswer).First().Value == "1")
                                            {
                                                renderedDoc = await _fileService.RenderDocument(user, template, agreement, subsheet, null);
                                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                                agreement.Documents.Add(renderedDoc);
                                                await _fileService.UploadFile(renderedDoc);
                                            }
                                        }
                                        else
                                        {
                                            renderedDoc = await _fileService.RenderDocument(user, template, agreement, subsheet, null);
                                            renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                            agreement.Documents.Add(renderedDoc);
                                            await _fileService.UploadFile(renderedDoc);
                                        }
                                    }
                                } else if (template.DocumentType == 4 && agreement.ClientInformationSheet.Programme.PaymentType == "Credit Card")
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    renderedDoc.RenderToPDF = template.RenderToPDF;
                                    if (answerSheet.Programme.BaseProgramme.IsPdfDoc)
                                    {
                                        if (renderedDoc.IsTemplate == true)
                                        {
                                            renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
                                            renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
                                        }
                                    }
                                    agreement.Documents.Add(renderedDoc);
                                    await _fileService.UploadFile(renderedDoc);
                                } else if (template.DocumentType == 12 && agreement.ClientInformationSheet.Programme.PaymentType == "Invoice")
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    renderedDoc.RenderToPDF = template.RenderToPDF;
                                    if (answerSheet.Programme.BaseProgramme.IsPdfDoc)
                                    {
                                        if (renderedDoc.IsTemplate == true)
                                        {
                                            renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
                                            renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
                                        }
                                    }
                                    agreement.Documents.Add(renderedDoc);
                                    await _fileService.UploadFile(renderedDoc);
                                }
                                else if (template.DocumentType != 4 && template.DocumentType != 6 && template.DocumentType != 9 && template.DocumentType != 12)
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, null);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    renderedDoc.RenderToPDF = template.RenderToPDF;
                                    if (answerSheet.Programme.BaseProgramme.IsPdfDoc)
                                    {
                                        if (renderedDoc.IsTemplate == true)
                                        {
                                            renderedDoc = await _fileService.FormatCKHTMLforConversion(renderedDoc);
                                            renderedDoc = await _fileService.ConvertHTMLToPDF(renderedDoc);
                                        }
                                    }
                                    agreement.Documents.Add(renderedDoc);
                                    await _fileService.UploadFile(renderedDoc);
                                }
                            }
                        }
                    }

                }

                return Redirect("/Agreement/ViewAcceptedAgreement/" + clientProgramme.Id);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        private string _MPCourAttendanceEndor = @"<p><strong>Court Attendance</strong></p>
<p></p>
<p>[[InsurerCompanyShort]] will indemnify the Insured where the Insured attends Court as a witness in connection with a Valid Claim Covered by this Policy.</p>
<p>include the following rates per day for each and every day on which Your attendance in court has been required:</p>
<p>(a) for any of Your principals, partners, or directors $250 per day</p>
<p>(b) for any of Your employees or contractors $100 per day</p>
<p>Provided that the maximum [[InsurerCompanyShort]] will pay is $100,000 in the aggregate in respect of all Claims by all Insured members of the Wellness and Health Associated Professionals Insurance facility.</p>";

        private string _MPSplitLimitofIndemnityEndor = @"<p><strong>Split Limit of Indemnity</strong></p>
<p></p>
<p>Notwithstanding anything contained in the policy Schedule and wording to the contrary it is agreed that;</p>
<p>1.  The Limit of Indemnity is divided according to the following definitions:</p>
<p>Defence Costs Limit of Indemnity:         $250,000</p>
<p>Loss Limit of Indemnity:                           $250,000</p>
<p>Policy Limit of Indemnity:                       $500,000</p>
<p>2.  The following Definitions are included within the Policy wording:</p>
<p>Defence Costs Limit of Indemnity</p>
<p>'Defence Costs Limit of Indemnity' means [[InsurerCompanyShort]]’s maximum limit of liability for payment of all Defence Costs incurred by or on behalf on an Insured as specified in this Endorsement.</p>
<p>Loss Limit of Indemnity</p>
<p>'Loss Limit of Indemnity' means [[InsurerCompanyShort]]'s maximum limit of liability for payment of all amounts (excluding Defence Costs) incurred by or on behalf of an Insured as specified in this Endorsement.</p>
<p>Policy Limit of Indemnity</p>
<p>'Policy Limit of Indemnity' means the combined total of the Defence Costs Limit of Indemnity and the Loss Limit of Indemnity as specified in the Schedule as the Policy Limit of Indemnity.</p>
<p>3.  If any Claim is subject to a sub-limit which is less than the Policy Limit of Indemnity, then the Defence Costs Limit of Indemnity and the Loss Limit of Indemnity shall be divided to the extent of the sub-limit in the same proportions as they bear to the Policy Limit of Indemnity.</p>
<p>Nothing in this Endorsement shall be held to vary, alter, waive or extend any of the terms, conditions, provisions, agreements or limitations of the Policy other than as stated in this Endorsement.</p>";

        private string _GLBusinessAdviceorServiceExclusion2Endor = @"<p><strong>Business Advice or Service Exclusion 2</strong></p>
<p></p>
<p>It is agreed that the Automatic Coverage Clause 3.1,  Business Advice or Service is deleted.</p>
<p>Nothing herein contained shall be held to vary, alter, waive or extend any of the terms, conditions, provisions, agreements or limitations of the above mentioned Policy other than as above stated.</p>";

        private string _SLAmendmentstoDefinitionEndor = @"<p><strong>Amendments to Definition 1.1</strong></p>
<p></p>
<p>Notwithstanding anything in the Policy to the contrary it is agreed that the Policy is amended as follows.</p>
<p>Definition 1.1 Act of Parliament is deleted and replaced by the following.</p>
<p>1.1  Act of Parliament</p>
<p>'Act of Parliament' means any Act of the New Zealand Parliament, including any amendment, enactment, re-enactment or replacement legislation or any Code, Rules or Regulations under such Act;</p>
<p>In all other respects this Policy remains unaltered other than as stated above.</p>";

        private string _SLExclusionEndor = @"<p><strong>Exclusion 4.7</strong></p>
<p></p>
<p>The Land Transport Act 1998 is added to Exclusion 4.7 as an Excluded Act.</p>
<p>In all other respects this Policy remains unaltered other than as stated above.</p>";

        private string _SLSplitLimitofIndemnityEndor = @"<p><strong>Split Limit of Indemnity</strong></p>
<p></p>
<p>Notwithstanding anything contained in the policy Schedule and wording to the contrary it is agreed that;</p>
<p>1.  The Limit of Indemnity is divided according to the following definitions:</p>
<p>Defence Costs Limit of Indemnity:         $250,000</p>
<p>Loss Limit of Indemnity:                           $250,000</p>
<p>Policy Limit of Indemnity:                       $500,000</p>
<p>2.  The following Definitions are included within the Policy wording:</p>
<p>Defence Costs Limit of Indemnity</p>
<p>'Defence Costs Limit of Indemnity' means [[InsurerCompanyShort]]’s maximum limit of liability for payment of all Defence Costs incurred by or on behalf on an Insured as specified in this Endorsement.</p>
<p>Loss Limit of Indemnity</p>
<p>'Loss Limit of Indemnity' means [[InsurerCompanyShort]]'s maximum limit of liability for payment of all amounts (excluding Defence Costs) incurred by or on behalf of an Insured as specified in this Endorsement.</p>
<p>Policy Limit of Indemnity</p>
<p>'Policy Limit of Indemnity' means the combined total of the Defence Costs Limit of Indemnity and the Loss Limit of Indemnity as specified in the Schedule as the Policy Limit of Indemnity.</p>
<p>3.  If any Claim is subject to a sub-limit which is less than the Policy Limit of Indemnity, then the Defence Costs Limit of Indemnity and the Loss Limit of Indemnity shall be divided to the extent of the sub-limit in the same proportions as they bear to the Policy Limit of Indemnity.</p>
<p>Nothing in this Endorsement shall be held to vary, alter, waive or extend any of the terms, conditions, provisions, agreements or limitations of the Policy other than as stated in this Endorsement.</p>";

    }
}