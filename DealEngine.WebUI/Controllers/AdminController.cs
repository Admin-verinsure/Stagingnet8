using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using File = System.IO.File ;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;
using SystemDocument = DealEngine.Domain.Entities.Document;
using Document = DealEngine.Domain.Entities.Document;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models;
using DealEngine.Infrastructure.FluentNHibernate;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
using Microsoft.AspNetCore.Identity;
using UpdateType = DealEngine.Domain.Entities.UpdateType;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class AdminController : BaseController
	{
        IMilestoneService _milestoneService;
		IPrivateServerService _privateServerService;
        IPaymentGatewayService _paymentGatewayService;
        IMerchantService _merchantService;
        IFileService _fileService;
		IDeveloperToolService _developerToolService;		
		IUnitOfWork _unitOfWork;
		IInformationTemplateService _informationTemplateService;
        IClientInformationService _clientInformationService;
		IProgrammeService _programmeService;
		IVehicleService _vehicleService;
        ISystemEmailService _systemEmailService;
        IReferenceService _referenceService;
        IMapper _mapper;
        ILogger<AdminController> _logger;
        IApplicationLoggingService _applicationLoggingService;
        IImportService _importService;
        ISerializerationService _serializerationService;
        IOrganisationService _organisationService;
        SignInManager<IdentityUser> _signInManager;
        UserManager<IdentityUser> _userManager;
        // IUpdateTypeService _updateTypeService;
        IUpdateTypeService _updateTypeServices;
        public AdminController(
            IUpdateTypeService updateTypeService,
            IOrganisationService organisationService,
            ISerializerationService serializerationService,
            IMilestoneService milestoneService,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IImportService importService,
            IApplicationLoggingService applicationLoggingService,
            ILogger<AdminController> logger,
            IUserService userRepository, 
            IPrivateServerService privateServerService, 
            IFileService fileService,
            IDeveloperToolService developerToolService, 
            IUnitOfWork unitOfWork, 
            IInformationTemplateService informationTemplateService,
            IClientInformationService clientInformationService, 
            IProgrammeService programeService, 
            IVehicleService vehicleService, 
            IMapper mapper, 
            IPaymentGatewayService paymentGatewayService,
            IMerchantService merchantService, 
            ISystemEmailService systemEmailService, 
            IReferenceService referenceService)
			: base (userRepository)
		{
            _organisationService = organisationService;
            _serializerationService = serializerationService;
            _milestoneService = milestoneService;
            _userManager = userManager;
            _signInManager = signInManager;
            _importService = importService;
            _applicationLoggingService = applicationLoggingService;
            _logger = logger;
			_privateServerService = privateServerService;
			_fileService = fileService;
			_unitOfWork = unitOfWork;
			_informationTemplateService = informationTemplateService;
			_clientInformationService = clientInformationService;
			_programmeService = programeService;
			_vehicleService = vehicleService;
			_mapper = mapper;
            _paymentGatewayService = paymentGatewayService;
            _merchantService = merchantService;
            _systemEmailService = systemEmailService;
            _referenceService = referenceService;
            _developerToolService = developerToolService;
            //
            _updateTypeServices = updateTypeService;
        }

		[HttpGet]
		public async Task<IActionResult> Index ()
		{
            AdminViewModel model = new AdminViewModel();
            var user = await CurrentUser();

            if (user.PrimaryOrganisation.IsTC)
            {
                try
                {
                    var privateServers = await _privateServerService.GetAllPrivateServers();
                    var paymentGateways = await _paymentGatewayService.GetAllPaymentGateways();
                    var merchants = await _merchantService.GetAllMerchants();
                    var users = _userManager.Users.ToList();

                    model.PrivateServers = _mapper.Map<IList<PrivateServer>, IList<PrivateServerViewModel>>(privateServers);
                    model.PaymentGateways = _mapper.Map<IList<PaymentGateway>, IList<PaymentGatewayViewModel>>(paymentGateways);
                    model.Merchants = _mapper.Map<IList<Merchant>, IList<MerchantViewModel>>(merchants);
                    model.Users = users;
                    return View(model);
                }
                catch (Exception ex)
                {
                    await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                    return RedirectToAction("Error500", "Error");
                }
            }
            else
            {
                return RedirectToAction("Error404", "Error");
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> AbbottImportOwners()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAbbottImportOwners(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApolloImportOwners()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportApolloImportOwners(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FANZImportOwners()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFanzOwners(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> FANZImportAdvisors()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFanzAdvisors(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        public async Task<IActionResult> FANZImportML()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFANZImportML(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        public async Task<IActionResult> FANZImportRO()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFANZImportRO(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        public async Task<IActionResult> FANZPIPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFANZPIPreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        public async Task<IActionResult> FANZMLPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFANZMLPreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        public async Task<IActionResult> FANZROPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportFANZROPreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> NZPIImportPlanners()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZPIImportPlanners(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZPIImportContractors()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZPIImportContractors(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AAAImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAAAServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZPIImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZPIServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApolloImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportApolloServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ApolloSetELDefaultVale()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportApolloSetELDefaultVale(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AbbottImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAbbottServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportPIUsers()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServicePI(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> NZFSGImportPInewUsers()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.NZFSGImportPInewUsers(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportPIUsersNewCompany()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServicePINewCompany(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportPIUsersNewAll()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServicePINewAll(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportMLUsers()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServiceML(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportMLUsersNewCompany()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServiceMLNewCompany(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportMLUsersNewAll()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServiceMLNewAll(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> NZFSGImportROUsers()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportNZFSGServiceRO(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> CEASImportClaims()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportCEASServiceClaims(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CEASImportContracts()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportCEASServiceContract(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PMINZImportContracts()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportPMINZServiceContract(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AONImportPrincipals()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAOEServicePrincipals(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CEASImportPrincipals()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportCEASServicePrincipals(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PMINZImportPrincipals()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportPMINZServicePrincipals(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PMINZImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportPMINZServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DANZImportUsers()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportDANZServiceIndividuals(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DANZImportPersonnel()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportDANZServicePersonnel(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DANZImportClaims()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportDANZServiceClaims(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DANZImportPreRenewData()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportDANZServicePreRenewData(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AONImportContracts()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAOEServiceContract(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AONImportClaims()
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                await _importService.ImportAOEServiceClaims(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrivateServerList()
        {
			var privateServers = await _privateServerService.GetAllPrivateServers();

			return PartialView ("_PrivateServerList", _mapper.Map<IList<PrivateServer>, IList<PrivateServerViewModel>> (privateServers));
			//return Json(privateServers, JsonRequestBehavior.AllowGet) ;
        }

		[HttpPost]
        public async Task<IActionResult> AddPrivateServer(PrivateServerViewModel privateServer)
        {
			var privateServers = await _privateServerService.GetAllPrivateServers();
            User user = null;
            try
            {
                user = await CurrentUser();
                // check to see if we are updating a private server
                if (privateServers.Any(ps => ps.ServerAddress == privateServer.ServerAddress))
					await _privateServerService.RemoveServer(user, privateServer.ServerAddress);  

				await _privateServerService.AddNewServer(user, privateServer.ServerName, privateServer.ServerAddress);
				// reload servers
				privateServers = await _privateServerService.GetAllPrivateServers();
				return PartialView ("_PrivateServerList", _mapper.Map<IList<PrivateServer>, IList<PrivateServerViewModel>> (privateServers));
            }
			catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);                
                return RedirectToAction("Error500", "Error");
            }
        }

		[HttpPost]
		public async Task<IActionResult> DeletePrivateServer(string id)
		{            
            await _privateServerService.RemoveServer(await CurrentUser(), id);
			return await PrivateServerList();
		}

        [HttpGet]
        public async Task<IActionResult> PaymentGatewayList()
        {
            var paymentGateways = await _paymentGatewayService.GetAllPaymentGateways();

            return PartialView("_PaymentGatewayList", _mapper.Map<IList<PaymentGateway>, IList<PaymentGatewayViewModel>>(paymentGateways));
        }

        [HttpPost]
        public async Task<IActionResult> AddPaymentGateway(PaymentGatewayViewModel paymentGateway)
        {
            var paymentGateways = await _paymentGatewayService.GetAllPaymentGateways();
            var user = await CurrentUser();
            try
            {
                // check to see if we are updating a payment gateway
                if (paymentGateways.Any(pgws => pgws.PaymentGatewayWebServiceURL == paymentGateway.PaymentGatewayWebServiceURL))
                    await _paymentGatewayService.RemovePaymentGateway(user, paymentGateway.PaymentGatewayWebServiceURL);

                await _paymentGatewayService.AddNewPaymentGateway(user, paymentGateway.PaymentGatewayName, paymentGateway.PaymentGatewayWebServiceURL, paymentGateway.PaymentGatewayResponsePageURL,
                    paymentGateway.PaymentGatewayType);
                // reload payment gateways
                paymentGateways = await _paymentGatewayService.GetAllPaymentGateways();
                return PartialView("_PaymentGatewayList", _mapper.Map<IList<PaymentGateway>, IList<PaymentGatewayViewModel>>(paymentGateways));
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePaymentGateway(string id)
        {
            await _paymentGatewayService.RemovePaymentGateway(await CurrentUser(), id);            
            return await PaymentGatewayList();
        }

        [HttpGet]
        public async Task<IActionResult> MerchantList()
        {
            
            MerchantViewModel merchantModel = new MerchantViewModel();
            var allPaymentGateways = new List<PaymentGatewayViewModel>();
            var user = await CurrentUser();
            try
            {
                var merchants = await _merchantService.GetAllMerchants();
                var dbPaymentGateways = await _paymentGatewayService.GetAllPaymentGateways();
                foreach (PaymentGateway pg in dbPaymentGateways)
                {
                    allPaymentGateways.Add(PaymentGatewayViewModel.FromEntity(pg));
                }
                merchantModel.AllPaymentGateways = allPaymentGateways;

                return PartialView("_MerchantList", _mapper.Map<IList<Merchant>, IList<MerchantViewModel>>(merchants));
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddMerchant(MerchantViewModel merchant)
        {
            var merchants = await _merchantService.GetAllMerchants();
            var user = await CurrentUser();
            try
            {
                if (merchants.Any(ms => ms.MerchantKey == merchant.MerchantKey))
                    await _merchantService.RemoveMerchant(user, merchant.MerchantKey);

                await _merchantService.AddNewMerchant(user, merchant.MerchantUserName, merchant.MerchantPassword, merchant.MerchantKey, 
                    merchant.MerchantReference);
                // reload merchants
                merchants = await _merchantService.GetAllMerchants();
                return PartialView("_MerchantList", _mapper.Map<IList<Merchant>, IList<MerchantViewModel>>(merchants));
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMerchant(string id)
        {
            await _merchantService.RemoveMerchant(await CurrentUser(), id);                
            return await MerchantList();
        }        

        [HttpPost]
		public async Task<IActionResult> UnlockUser(string UserId)
		{            
            var user = await _userService.GetUserById(Guid.Parse(UserId));
            user.Unlock();
            await _userService.Update(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ManageRsaUsers()
        {
            AdminViewModel model = new AdminViewModel();
            var user = await CurrentUser();
            try
            {
                var lockedUsers = await _userService.GetLockedUsers();

                if (lockedUsers.Count != 0)
                {
                    model.LockedUsers = new List<SelectListItem>();
                    foreach (var lockedUser in lockedUsers)
                    {
                        model.LockedUsers.Add(new SelectListItem
                        {
                            Value = lockedUser.Id.ToString(),
                            Text = lockedUser.LastName + " username: " + lockedUser.UserName
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
         [HttpGet]
        public async Task<IActionResult> UpdateType()
        {
            User user = null;
            UpdateTypesViewModel model = new UpdateTypesViewModel();

            try
            {
                user = await CurrentUser();
                var dbUpdatemodelTypes = await _updateTypeServices.GetAllUpdateTypes();
                var updateTypeModel = new List<UpdateTypesViewModel>();
                model.Programme = await _programmeService.GetAllProgrammes();

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
                        //ProgrammeIsFanz = updateType.ProgrammeIsFanz,
                        //ProgrammeIsFmc = updateType.ProgrammeIsFmc
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
        public async Task<IActionResult> DeleteUpdateType(UpdateTypesViewModel updateType)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                UpdateType UpdateType = await _updateTypeServices.GetUpdateType(updateType.Id);

                UpdateType updatetype = null;

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if(UpdateType.Id != null)
                    {
                        UpdateType.DateDeleted = DateTime.UtcNow;
                        await uow.Commit();

                    }


                }
                return RedirectToAction("UpdateType");

            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditUpdateType( UpdateTypesViewModel updateType)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                UpdateType updatetype = await _updateTypeServices.GetUpdateType(updateType.Id);
                if (updateType.Id != Guid.Empty)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                            updatetype.TypeName = updateType.NameType;
                        updatetype.TypeValue = updateType.ValueType;
                        updatetype.TypeIsTc = updateType.TypeIsTc;
                        updatetype.TypeIsBroker = updateType.TypeIsBroker;
                        updatetype.TypeIsClient = updateType.TypeIsClient;
                        updatetype.TypeIsInsurer = updateType.TypeIsInsurer;
                        //updatetype.ProgrammeIsFmc = updateType.ProgrammeIsFmc;
                        //updatetype.ProgrammeIsFanz = updateType.ProgrammeIsFanz;

                        await uow.Commit();

                    }
                }
                else
                {

                    await _updateTypeServices.AddUpdateType(user, updateType.NameType, updateType.ValueType, updateType.TypeIsTc, updateType.TypeIsBroker, updateType.TypeIsInsurer, updateType.TypeIsClient);
                    
                }
                return RedirectToAction("UpdateType");


            }
            catch (Exception ex)
            {
               await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
               return RedirectToAction("Error500", "Error");
            }
        }




        [HttpGet]
        public async Task<IActionResult> SysEmailTemplate(String systemEmailType, String internalNotes)
        {
            
             
            SystemEmailTemplateViewModel model = new SystemEmailTemplateViewModel();
            var user = await CurrentUser();
            try
            {
                SystemEmail systemEmailTemplate = await _systemEmailService.GetSystemEmailByType(systemEmailType);
                model.InternalNotes = internalNotes;
                model.SystemEmailType = systemEmailType;

                if (systemEmailTemplate != null)
                {
                    model.SystemEmailName = systemEmailTemplate.SystemEmailName;
                    model.Subject = systemEmailTemplate.Subject;
                    model.Body = System.Net.WebUtility.HtmlDecode(systemEmailTemplate.Body);

                }
                else
                {
                    model.SystemEmailName = "";
                    model.Subject = "";
                    model.Body = "";
                }

                ViewBag.Title = "Add/Edit System Email Template";

                return View("SysEmailTemplate", model);
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SysEmailTemplate(SystemEmailTemplateViewModel model)
        {
            var user = await CurrentUser();
            try
            {
                SystemEmail systemEmailTemplate = await _systemEmailService.GetSystemEmailByType(model.SystemEmailType);
                string systememailtemplatename = null;

                switch (model.SystemEmailType)
                {
                    case "LoginEmail":
                        {
                            systememailtemplatename = "Login Instruction Email";
                            break;
                        }
                    case "UISIssueNotificationEmail":
                        {
                            systememailtemplatename = "Information Sheet Issue Notification Email";
                            break;
                        }
                    case "InvoiceSuccessConfig":
                        {
                            systememailtemplatename = "Invoice Success Configuration Notification Email";
                            break;
                        }
                    case "InvoiceFailConfig":
                        {
                            systememailtemplatename = "Invoice Fail Configuration Notification Email";
                            break;
                        }
                    case "PaymentSuccessConfig":
                        {
                            systememailtemplatename = "Payment Success Configuration Notification Email";
                            break;
                        }
                    case "PaymentFailConfig":
                        {
                            systememailtemplatename = "Payment Fail Configuration Notification Email";
                            break;
                        }
                    case "UISSubmissionConfirmationEmail":
                        {
                            systememailtemplatename = "Information Sheet Submission Confirmation Email";
                            break;
                        }
                    case "UISSubmissionNotificationEmail":
                        {
                            systememailtemplatename = "Information Sheet Submission Notification Email";
                            break;
                        }
                    case "AgreementReferralNotificationEmail":
                        {
                            systememailtemplatename = "Agreement Referral Notification Email";
                            break;
                        }
                    case "AgreementIssueNotificationEmail":
                        {
                            systememailtemplatename = "Agreement Issue Notification Email";
                            break;
                        }
                    case "AgreementBoundNotificationEmail":
                        {
                            systememailtemplatename = "Agreement Bound Notification Email";
                            break;
                        }
                    case "OtherMarinaTCNotifyEmail":
                        {
                            systememailtemplatename = "Create Other Marina Notification Email";
                            break;
                        }
                    case "OneTimePasswordEmail":
                        {
                            systememailtemplatename = "One Time Password Email";
                            break;
                        }
                    case "RSANotificationEmail":
                        {
                            systememailtemplatename = "RSA Notification Email";
                            break;
                        }
                    default:
                        {
                            throw new Exception(string.Format("Invalid System Email Template Type for ", model.SystemEmailType));
                        }
                }

                if (systemEmailTemplate != null)
                {
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        systemEmailTemplate.Subject = model.Subject;
                        systemEmailTemplate.Body = model.Body;
                        systemEmailTemplate.LastModifiedBy = await CurrentUser();
                        systemEmailTemplate.LastModifiedOn = DateTime.UtcNow;

                        await uow.Commit();
                    }
                }
                else
                {
                    await _systemEmailService.AddNewSystemEmail(await CurrentUser(), systememailtemplatename, model.InternalNotes, model.Subject, model.Body, model.SystemEmailType);
                }

                return Redirect("~/Admin/Index");
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
            

        }


        [HttpGet]
        public async Task<IActionResult> SysExchange()
        {
            return View("SysExchange");
        }

        [HttpPost]
        public async Task<IActionResult> SysExchange(OrganisationViewModel model)
        {
            throw new Exception("new organisation method");
            //Organisation org = await _organisationService.GetOrganisationByEmail(model.Email);            
            //User user = await  _userService.GetUserByEmail(model.Email);

            //return Redirect("~/Admin/Index");

        }

        [HttpPost]
        public async Task<IActionResult> ImpersonateUser(IFormCollection form)
        {
            await _signInManager.SignOutAsync();
            var deUser = await _userManager.FindByNameAsync(form["username"].ToString());
            await _signInManager.SignInAsync(deUser, true);

            return Redirect("~/Home/Index");

        }

        [HttpGet]
        public async Task<IActionResult> DeveloperTool()
        {
            var Users =await _userService.GetAllUsers();
            foreach(User user in Users)
            {
                if (user.UserTasks.Any())
                {
                    user.UserTasks.Clear();
                    await _userService.Update(user);
                }
            }
            return Redirect("~/Home/Index");
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            UserViewModel userViewModel = new UserViewModel();
            return View(userViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ModifyCKEditor()
        {
            CKEditorViewModel model = new CKEditorViewModel();

            // Get directories that begin with ckeditor5-techcertain in ckeditor folder
            string[] directoriesArr = Directory.GetDirectories("wwwroot/ckeditor/", "ckeditor5-techcertain*", SearchOption.AllDirectories);
            List<string> directories = new List<string>(directoriesArr);

            IList<CKEditorViewModel.CKEditorBuild> directoryList = new List<CKEditorViewModel.CKEditorBuild>();

            foreach (string dir in directories)
            {
                CKEditorViewModel.CKEditorBuild CkBuild = new CKEditorViewModel.CKEditorBuild();
                CkBuild.Name = dir.Substring(27);
                CkBuild.Path = dir;
                CkBuild.Placeholders = GetPlaceholders(dir);
                directoryList.Add(CkBuild);
            }
            model.DirectoryList = directoryList;

            return View(model);
        }

        [HttpGet]
        public string GetPlaceholders(string Path)
        {
            // This will be computationally intensive if we end up with a LOT of different ckeditors, doubt it will happen but if it does.
            // You'll want to rewrite it so the placeholders are saved into an object and loaded from there rather than checking the .js file.

            string placeholders = "";

            string[] ckBuild = System.IO.File.ReadAllLines(Path + "/build/ckeditor.js").ToArray();

            int lengthOfFile = ckBuild.Length;
            int stoppingPoint = ckBuild.Length - 100; // We only want to reverse iterate about 100 places MAX

            while (lengthOfFile > stoppingPoint)
            {
                if (ckBuild[lengthOfFile - 1].Contains("placeholderConfig: {"))
                {
                    string lineWithPlaceholders = ckBuild[lengthOfFile];
                    // Create an array from actual array in string, get index of [ and index of ] get that string between those two then return that string.
                    placeholders = GetStringBetweenCharacters(lineWithPlaceholders, '[', ']');
                    break;
                }
                lengthOfFile = lengthOfFile - 1;
            }

            return placeholders;
        }

        [HttpPost]
        public async Task<IActionResult> AddPlaceholder(CKEditorViewModel model)
        {
            // Wasn't a way to just update one line efficiently, need to either process whole file or store file in memory.
            // string currentPlaceholders = System.IO.File.ReadLines(model.Path + "/build/ckeditor.js").Reverse().Take(38).ToList()[37];

            if (model != null)
            {
                string[] ckBuild = System.IO.File.ReadAllLines(model.Path + "/build/ckeditor.js").ToArray();

                // Its n long, we only care about the very end so just search the file backwards until we get the index of the line containing 
                // PlaceholderConfig, the types are on the next line following this so we add the new placeholder there.

                int lengthOfFile = ckBuild.Length;
                int stoppingPoint = ckBuild.Length - 100; // We only want to reverse iterate about 100 places MAX

                while (lengthOfFile > stoppingPoint)
                {
                    if (ckBuild[lengthOfFile - 1].Contains("placeholderConfig: {"))
                    {
                        ckBuild[lengthOfFile] = ckBuild[lengthOfFile].Substring(0, ckBuild[lengthOfFile].Length - 1) + ", '" + model.Placeholder + "']";
                        break;
                    }
                    lengthOfFile = lengthOfFile - 1;
                }
                System.IO.File.WriteAllLines(model.Path + "/build/ckeditor.js", ckBuild);
            }

            return Redirect("~/Admin/ModifyCKEditor");
        }

        [HttpPost]
        public async Task<IActionResult> GetCreateUser(IFormCollection form)
        {
            var user = await _userService.GetUserByEmail(form["UserEmail"]);

            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            if (user != null)
            {
                JsonObjects.Add("User", user);
                JsonObjects.Add("Organisation", user.PrimaryOrganisation);
                var jsonObj = await _serializerationService.GetSerializedObject(JsonObjects);
                return Json(jsonObj);                
            }
            return Json(null);
        }        

        [HttpPost]
        public async Task<IActionResult> PostCreateUser(IFormCollection form)
        {
            var currentUser = await CurrentUser();
            var jsonUser = (User)await _serializerationService.GetDeserializedObject(typeof(User), form);
            var user = await _userService.PostCreateUser(jsonUser, currentUser, form);                                                          
            var deUser = await _userManager.FindByEmailAsync(user.Email);
            if(deUser == null)
            {
                deUser = new IdentityUser
                {
                    UserName = user.UserName,
                    Email = user.Email
                };
                await _userManager.CreateAsync(deUser, "defaultPassword");
            }
            return Redirect("~/Home/Index");
        }

        public static string GetStringBetweenCharacters(string input, char charFrom, char charTo)
        {
            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
    }
}
