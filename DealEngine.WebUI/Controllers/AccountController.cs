#region Using
using System;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Domain.Exceptions;
using System.Collections.Generic;
using DealEngine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using DealEngine.Infrastructure.Ldap.Interfaces;
using IAuthenticationService = DealEngine.Services.Interfaces.IAuthenticationService;
using DealEngine.Infrastructure.AuthorizationRSA;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
using IdentityRole = NHibernate.AspNetCore.Identity.IdentityRole;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using DealEngine.Infrastructure.FluentNHibernate;
#endregion

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        IAuthenticationService _authenticationService;        
        IEmailService _emailService;
		IFileService _fileService;
        IOrganisationService _organisationService;
        SignInManager<IdentityUser> _signInManager;
        UserManager<IdentityUser> _userManager;
        RoleManager<IdentityRole> _roleManager;
        ILdapService _ldapService;
        IOrganisationalUnitService _organisationalUnitService;
        ILogger<AccountController> _logger;
        IApplicationLoggingService _applicationLoggingService;        
        IHttpClientService _httpClientService;
        IAppSettingService _appSettingService;
        IImportService _importService;
        IUnitOfWork _unitOfWork;

        public AccountController(
            IUnitOfWork unitOfWork,
            IImportService importService,
            IApplicationLoggingService applicationLoggingService,
            IAuthenticationService authenticationService,
			SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,            
            IHttpClientService httpClientService,
            IOrganisationService organisationService,
            ILdapService ldapService,
            IUserService userService,
			IEmailService emailService, 
            IFileService fileService,   
            IOrganisationalUnitService organisationalUnitService, 
            IAppSettingService appSettingService) : base (userService)
		{
            _unitOfWork = unitOfWork;
            _organisationService = organisationService;
            _importService = importService;
            _applicationLoggingService = applicationLoggingService;
            _authenticationService = authenticationService;
            _ldapService = ldapService;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;            
			_httpClientService = httpClientService;
			_logger = logger;
			_userService = userService;
            _emailService = emailService;
			_fileService = fileService;
            _organisationalUnitService = organisationalUnitService;
            _appSettingService = appSettingService;
        }

		// GET: /account/forgotpassword
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword()
		{
			//if (Request.IsAuthenticated)
				return await RedirectToLocal();
			
			// TODO - need to somehow call ResetPassword and return its view so we don't have to duplicate it here.
			// We do not want to use any existing identity information
			//EnsureLoggedOut();

			//return View ("ResetPassword");
		}

		// GET: /account/resetpassword
		[HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword()
		{
            if (User.Identity.IsAuthenticated)
                return await RedirectToLocal();

            // We do not want to use any existing identity information
            EnsureLoggedOut();

            return View();
        }

		// POST: /account/resetpassword
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(AccountResetPasswordModel viewModel)
		{
            User user = null;
			string errorMessage = @"We have sent you an email to the email address we have recorded in the system, that email address is different from the one you supplied. 
				Please check the other email addresses you may have used. If you cannot locate our email, 
				please email support@techcertain.com with your contact details, we can re-establish your account with your broker.";
			try
			{
				if (!string.IsNullOrWhiteSpace (viewModel.Email))
				{
                    //System Email Testing
                    //var testuser = _userService.GetUserByEmail("mcgtestuser2@techcertain.com");
                    //var programme = _programmeService.GetAllProgrammes().FirstOrDefault(p => p.Name == "Demo Coastguard Programme");
                    //var organisation = _organisationService.GetOrganisationByEmail("mcgtestuser2@techcertain.com");
                    //var sheet = _clientInformationService.GetInformation(new Guid("bc3c9972-1733-41a1-8786-fa22229c66f8"));
                    //_emailService.SendSystemEmailLogin("support@techcertain.com");

                    SingleUseToken token = await _authenticationService.GenerateSingleUseToken(viewModel.Email);
                    user = await _userService.GetUserById(token.UserID);
                    if (user != null)
                    {
                        //change the users password to an intermediate
                        try
                        {
                            _ldapService.ChangePassword(user.UserName, "", _appSettingService.IntermediatePassword);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        var deUser = await _userManager.FindByNameAsync(user.UserName);
                        if (deUser != null)
                        {
                            var removePasswordResult = await _userManager.RemovePasswordAsync(deUser);
                            var addPasswordResult = await _userManager.AddPasswordAsync(deUser, _appSettingService.IntermediatePassword);
                            if (addPasswordResult.Succeeded)
                            {
                                
                            }
                        }

                        //get local domain
                        string domain = "https://" + _appSettingService.domainQueryString; //HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                        await _emailService.SendPasswordResetEmail(viewModel.Email, token.Id, domain);
                        ViewBag.EmailSent = true;
                    }
                    
                }
            }
            catch (System.Net.Mail.SmtpFailedRecipientsException exception) {
                await _applicationLoggingService.LogWarning(_logger, exception, user, HttpContext);
                ModelState.AddModelError ("FailureMessage", errorMessage);
                return View(viewModel);
            }
            catch (MailKit.Net.Smtp.SmtpCommandException ex) {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                ModelState.AddModelError ("FailureMessage", "Oops, Email services are currently unavailable. The technical support staff have also been notified, and your password reset email will be sent once services have been restored.");
				return View (viewModel);
			}
			catch (Exception ex)
			{
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                Exception exception = ex;
				while (exception.InnerException != null) exception = exception.InnerException;

				await _emailService.ContactSupport (_emailService.DefaultSender, exception.GetType().Name + ": " + exception.Message, "");

				ModelState.AddModelError("FailureMessage", errorMessage);
				if (exception is MultipleUsersFoundException)
					ModelState.AddModelError ("FailureMessage", "We were unable to generate a password reset email for you.");
				return View(viewModel);
			}

			return View ();
		}

        // GET: /account/changepassword
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(Guid id)
        {
            if (id != Guid.Empty && _authenticationService.GetToken(id) != null)
            {
                var result = await _authenticationService.ValidSingleUseToken(id);
                if (result)
                    return View();

                return Redirect("~/Error/InvalidPasswordReset");
            }

            return PageNotFound();
        }

        [HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(Guid id, AccountChangePasswordModel viewModel)
		{
			try
			{
                if (id == Guid.Empty)
					// if we get here - either invalid guid or invalid token - 404
					return PageNotFound ();

				if (viewModel.Password != viewModel.PasswordConfirm) {
					ModelState.AddModelError ("passwordConfirm", "Passwords do not match");
					return View ();
				}
                SingleUseToken st = _authenticationService.GetToken(id);
                User user = await _userService.GetUserById(st.UserID);
                if (user == null)
                    // in theory, we should never get here. Reason being is that a reset request should not be created without a valid user
                    throw new Exception(string.Format("Could not find user with ID {0}", st.UserID));


                if (user != null)
                {
                    string username = user.UserName;

                    //change the users password to an intermediate
                    if (_ldapService.ChangePassword(user.UserName, _appSettingService.IntermediatePassword, viewModel.Password))
                    {
                        var deUser = await _userManager.FindByNameAsync(user.UserName);
                        if (deUser != null)
                        {
                            var removePasswordResult = await _userManager.RemovePasswordAsync(deUser);
                            var addPasswordResult = await _userManager.AddPasswordAsync(deUser, viewModel.Password);
                            if (addPasswordResult.Succeeded)
                            {
                                _authenticationService.UseSingleUseToken(st.Id);
                                return RedirectToAction("PasswordChanged", "Account");
                            }
                        }
                        else
                        {
                            //assume user hasnt logged in yet and wants to change password for first time
                            _authenticationService.UseSingleUseToken(st.Id);
                            return RedirectToAction("PasswordChanged", "Account");
                        }
                        //} else
                        //{
                        //    _authenticationService.UseSingleUseToken(st.Id);
                        //    return RedirectToAction("PasswordChanged", "Account");
                        //}

                    }
                    else
                    {
                        ModelState.AddModelError("passwordConfirm", "The password change has failed. Is your new password complex enough?");
                        return View();
                    }

                }

            }
			catch (AuthenticationException ex) 
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                ModelState.AddModelError ("passwordConfirm", "Your chosen password does not meet the requirements of our password policy. Please refer to the policy above to assist with creating an appropriate password.");				
			}
			catch (Exception ex) {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                ModelState.AddModelError ("passwordConfirm", "There was an error while trying to change your password.");				
			}

			return View ();
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> PasswordChanged ()
		{
			return View ();
		}

		// GET: /account/login
		[HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var viewModel = new AccountLoginModel
            {
                ReturnUrl = returnUrl,
                DomainString = _appSettingService.domainQueryString,
            };

            string nameExtension = _appSettingService.RequireRSA;

            return View("Login"+nameExtension, viewModel);
        }

        // POST: /account/login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLoginModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
			}

			if (User.Identity.IsAuthenticated)
				return await RedirectToLocal();

            var userName = viewModel.Username.Trim();
            
            try
            {                                                
                string password = viewModel.Password.Trim();
                var user = await _userService.GetUser(userName);
                int resultCode = -1;
                string resultMessage = "";
                IdentityUser deUser;

                // Step 1 validate in  LDap 
                _ldapService.Validate(userName, password, out resultCode, out resultMessage);
                if (resultCode == 0)
                {
                    var identityResult = await DealEngineIdentityUserLogin(user, password);
                    if (!identityResult.Succeeded)
                    {
                        deUser = await _userManager.FindByNameAsync(userName);
                        await _userManager.RemovePasswordAsync(deUser);
                        await _userManager.AddPasswordAsync(deUser, password);                        
                    }
                    else
                    {
                        deUser = await _userManager.FindByNameAsync(userName);
                        await _signInManager.SignOutAsync();
                        deUser = await _userManager.FindByNameAsync(userName);
                        
                    }
                    var result = await _signInManager.PasswordSignInAsync(deUser, password, viewModel.RememberMe, lockoutOnFailure: true);

                    return LocalRedirect("~/Home/Index");
                }
/*
                else if (resultCode == 49) //	LDAP_INVALID_CREDENTIALS               
                {
                    deUser = await _userManager.FindByNameAsync(userName);
                    var result = await _signInManager.PasswordSignInAsync(deUser, password, true, lockoutOnFailure: true);                    
                    // AccessFailedCount = MaxFailedAccessAttempts                   
                    if (result.IsLockedOut == true)
                    {
                        // tell them they've been locked out
                        ModelState.AddModelError(string.Empty, "You are locked out. You can try again in 5 minutes (maybe).");
                        // Update record so that we know they're locked out for next time? Should be automatic.
                        // what else?
                        return View(viewModel);
                    }
                    // AccessFailedCount < MaxFailedAccessAttempts
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Incorrect password. Please try entering your username or password again, or email support@techcertain.com.");
                        return View(viewModel);
                    }
                }
*/
                else // ANY OTHER LDAP CODE https://ldapwiki.com/wiki/LDAP%20Result%20Codes 
                {
                    ModelState.AddModelError(string.Empty, "We are unable to access your account with the username or password provided. You may have entered an incorrect password, or your account may be locked due to an extended period of inactivity. Please try entering your username or password again, or email support@techcertain.com.");
                    return View(viewModel);
                }
            }
			catch (UserImportException ex)
			{
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);                
				await _emailService.ContactSupport (_emailService.DefaultSender, "DealEngine 2019 - User Import Error", ex.Message);
				ModelState.AddModelError(string.Empty, "We have encountered an error importing your account. Proposalonline has been notified, and will be in touch shortly to resolve this error.");
				return View(viewModel);
			}
			catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);                
                throw new Exception(ex.Message + " "+ ex.StackTrace);
            }
        }

        private async Task<SignInResult> DealEngineIdentityUserLogin(User user, string password)
        {
            
            try
            {
                var deUser = await _userManager.FindByNameAsync(user.UserName);
                if (deUser == null)
                {
                    deUser = new IdentityUser();
                    deUser.UserName = user.UserName;
                    deUser.Email = user.Email;

                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        var result = await _userManager.CreateAsync(deUser, password);
                        if (result.Succeeded)
                        {
                            var hasRole = await _roleManager.RoleExistsAsync("Client");
                            if (hasRole)
                            {
                                await _userManager.AddToRoleAsync(deUser, "Client");
                                await _userManager.UpdateAsync(deUser);
                            }
                        }
                        await uow.Commit();
                    }
                }

                return await _signInManager.PasswordSignInAsync(deUser, password, true, lockoutOnFailure: false);                
            }
            
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                throw new Exception(ex.Message + " " + ex.StackTrace);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginMarsh(AccountLoginModel viewModel)
		{
            var userName = viewModel.Username.Trim();
            try
            {
                string password = viewModel.Password.Trim();
                var user = await _userService.GetUser(userName);
                int resultCode = -1;
                string resultMessage = "";
                _ldapService.Validate(userName, password, out resultCode, out resultMessage);
                if (resultCode == 0)
                {
                    MarshRsaAuthProvider rsaAuth = new MarshRsaAuthProvider(_logger, _httpClientService, _emailService);
                    MarshRsaUser rsaUser = rsaAuth.GetRsaUser(user.Email);
                    rsaUser.DevicePrint = viewModel.DevicePrint;
                    rsaUser.Email = user.Email;
                    //rsaUser.Username = user.UserName;
                    rsaUser.Username = rsaAuth.GetHashedId(user.UserName.ToLower()+"@mnzconnect.com"); //use hashed username(lower case) + production domain name as requested by Marsh
                    rsaUser.HttpReferer = "~Account/LoginMarsh";
                    rsaUser.OrgName = _appSettingService.MarshRSAOrgName; //staging:Marsh_Model, production: Marsh
                    rsaUser.RsaStatus = RsaStatus.Deny;
                    rsaUser.DeviceTokenCookie = user.DeviceTokenCookie;
                    if (!string.IsNullOrEmpty(user.DeviceTokenCookie)) //As marsh adviced ClientGenCookie is a Mandatory field for none-enrollment request, use DeviceTokenCookie to define the enrollment status
                    {
                        rsaUser.ClientGenCookie = Guid.NewGuid().ToString(); 
                    } else
                    {
                        rsaUser.ClientGenCookie = "";
                    }
                    rsaUser = await rsaAuth.Analyze(rsaUser);

                    if (rsaUser.RsaStatus == RsaStatus.Allow)
                    {
                        _logger.LogInformation("RSA Authentication succeeded for [" + user.UserName + "]");
                        var result = await DealEngineIdentityUserLogin(user, password);
                        if (!result.Succeeded)
                        {

                            var deUser = await _userManager.FindByNameAsync(user.UserName);
                            await _userManager.RemovePasswordAsync(deUser);
                            await _userManager.AddPasswordAsync(deUser, password);
                            await _signInManager.PasswordSignInAsync(deUser, password, true, lockoutOnFailure: false);
                        }
                        else
                        {
                            var deUser = await _userManager.FindByNameAsync(user.UserName);
                            await _signInManager.SignOutAsync();
                            deUser = await _userManager.FindByNameAsync(user.UserName);
                            await _signInManager.PasswordSignInAsync(deUser, password, true, lockoutOnFailure: false);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    if (rsaUser.RsaStatus == RsaStatus.RequiresOtp)
                    {
                        return View("OneTimePasswordMarsh", new RsaOneTimePasswordModel
                        {
                            UserName = user.UserName,
                            DeviceTokenCookie = rsaUser.DeviceTokenCookie,
                            DevicePrint = rsaUser.DevicePrint,
                            SessionId = rsaUser.CurrentSessionId,
                            TransactionId = rsaUser.CurrentTransactionId,
                            Password = password
                        });
                    }

                }
                ModelState.AddModelError(string.Empty, "We are unable to access your account with the username or password provided. You may have entered an incorrect password, or your account may be locked due to an extended period of inactivity. Please try entering your username or password again, or email support@techcertain.com.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
            }

            return await RedirectToLocal();
        }

        [HttpPost]
		[AllowAnonymous]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> OneTimePasswordMarsh (RsaOneTimePasswordModel viewModel)
		{
            if (ModelState.IsValid)
            {                
                MarshRsaAuthProvider rsaAuth = new MarshRsaAuthProvider(_logger, _httpClientService, _emailService);
                string username = viewModel.UserName;
                MarshRsaUser rsaUser = rsaAuth.GetRsaUser(username);                
                rsaUser.DevicePrint = viewModel.DevicePrint;
                rsaUser.DeviceTokenCookie = viewModel.DeviceTokenCookie;
                //rsaUser.Username = username;
                rsaUser.Username = rsaAuth.GetHashedId(username.ToLower() + "@mnzconnect.com"); //use hashed username(lower case) + production domain name as requested by Marsh
                rsaUser.HttpReferer = "";
                rsaUser.OrgName = _appSettingService.MarshRSAOrgName; //staging:Marsh_Model, production: Marsh
                rsaUser.Otp = viewModel.OtpCode;
                rsaUser.CurrentSessionId = viewModel.SessionId;
                rsaUser.CurrentTransactionId = viewModel.TransactionId;
                var user = await _userService.GetUser(viewModel.UserName);
                bool isAuthenticated = await rsaAuth.Authenticate(rsaUser, _userService, username);                
                if (isAuthenticated)
                {
                    var result = await DealEngineIdentityUserLogin(user, viewModel.Password);
                    if (!result.Succeeded)
                    {

                        var deUser = await _userManager.FindByNameAsync(username);
                        await _userManager.RemovePasswordAsync(deUser);
                        await _userManager.AddPasswordAsync(deUser, viewModel.Password);
                        await _signInManager.PasswordSignInAsync(deUser, viewModel.Password, true, lockoutOnFailure: false);
                    }
                    else
                    {
                        var deUser = await _userManager.FindByNameAsync(username);
                        await _signInManager.SignOutAsync();
                        deUser = await _userManager.FindByNameAsync(username);
                        await _signInManager.PasswordSignInAsync(deUser, viewModel.Password, true, lockoutOnFailure: false);                        
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.AccountLocked = "Your Login has failed - Marsh has been notified and will be in contact with you shortly";
                    await Logout();
                }                
            }

            return View ();
		}

		// GET: /account/error
		[HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Error()
        {
            // We do not want to use any existing identity information
            EnsureLoggedOut();

            return View();
        }

		// GET: /account/register
		[HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register()
		{
            if (User.Identity.IsAuthenticated)
                return await RedirectToLocal();

            // We do not want to use any existing identity information
            EnsureLoggedOut();

            return View(new AccountRegistrationModel());
        }

        // POST: /account/register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AccountRegistrationModel model)
        {
            // Ensure we have a valid viewModel to work with
            if (!ModelState.IsValid)
				return View(model);

			return await RedirectToLocal();
        }

        // GET: /account/coastguardreg
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CoastguardReg()
        {
            if (User.Identity.IsAuthenticated)
                return await RedirectToLocal();

            // We do not want to use any existing identity information
            EnsureLoggedOut();

            return View(new AccountRegistrationModel());
        }

        // POST: /account/coastguardreg
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CoastguardReg(AccountRegistrationModel model)
        {
            // Ensure we have a valid viewModel to work with
            if (!ModelState.IsValid)
                return View(model);

            return await RedirectToLocal();


        }

        // GET: /account/coastguardreg
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CoastguardForm()
        {
            if (User.Identity.IsAuthenticated)
                return await RedirectToLocal();

            // We do not want to use any existing identity information
            EnsureLoggedOut();

            return View();
        }

        // POST: /account/coastguardreg
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CoastguardForm(AccountRegistrationModel model)
        {
            // Ensure we have a valid viewModel to work with
            if (!ModelState.IsValid)
                return View(model);

            return await RedirectToLocal();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await  HttpContext.SignOutAsync();
            HttpContext.Response.Cookies.Delete(".AspNet.Consent");

            return await RedirectToLocal();
        }

        void EnsureLoggedOut()
        {
            if (User.Identity.IsAuthenticated)
                Logout();
        }

		[HttpGet]
		public async Task<IActionResult> Profile(string id)
		{
            var currentUser = await CurrentUser();
            var user = string.IsNullOrWhiteSpace (id) ? currentUser : await _userService.GetUser(id);
			if (user == null)
				return PageNotFound ();

            ProfileViewModel model = new ProfileViewModel();

            try
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Email = user.Email;
                model.Phone = user.Phone;
                model.CurrentUser = currentUser;
                model.DefaultOU = user.DefaultOU;
                model.EmployeeNumber = user.EmployeeNumber;
                model.JobTitle = user.JobTitle;
                model.SalesPersonUserName = user.SalesPersonUserName;

                if (user.PrimaryOrganisation != null)
                    model.PrimaryOrganisationName = user.PrimaryOrganisation.Name;
                //if (user.Organisations.Count() > 0 && user.Organisations.ElementAt(0).OrganisationType.Name != "personal")
                //	model.PrimaryOrganisationName = user.Organisations.ElementAt(0).Name;
                model.Description = user.Description;
                if (user.ProfilePicture != null)
                    model.ProfilePicture = user.ProfilePicture.Name;    // TODO - remap this

                model.ViewingSelf = string.IsNullOrEmpty(id) || (currentUser.UserName == id);
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }			

			return View(model);
		}

		[HttpGet]
		public async Task<IActionResult> ProfileEditor()
		{
			var user = await CurrentUser();
			if (user == null)
				return PageNotFound ();
			
			ProfileViewModel model = new ProfileViewModel();
            var organisationalUnits = new List<OrganisationalUnitViewModel>();

            try
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Email = user.Email;
                model.Phone = user.Phone;
                model.JobTitle = user.JobTitle;
                model.CurrentUser = user;

                if (user.DefaultOU != null)
                {
                    model.DefaultOU = user.DefaultOU;
                }

                model.EmployeeNumber = user.EmployeeNumber;
                model.JobTitle = user.JobTitle;
                model.SalesPersonUserName = user.SalesPersonUserName;
                if (user.PrimaryOrganisation != null)
                    model.PrimaryOrganisationName = user.PrimaryOrganisation.Name;
                model.Description = user.Description;
                if (user.ProfilePicture != null)
                    model.ProfilePicture = user.ProfilePicture.Name;    // TODO - remap this

                model.ViewingSelf = true;
                model.OrganisationalUnitsVM = organisationalUnits;
            }
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }
            

            return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ProfileEditor(ProfileViewModel model)
		{
			var user = await CurrentUser();
			if (user == null)
				return PageNotFound ();
            Guid defaultOU = Guid.Empty;

            try 
            {
                //if we've added a new avatar, upload it. Otherwise don't change
                if (Request.Form.Files.Count > 0)
                {
                    //Console.WriteLine ("{0} files uploaded", Request.Files.Count);
                    var file = Request.Form.Files[0];
                    if (file != null && file.Length > 0)
                    {
                        byte[] buffer = new byte[file.Length];
                        file.OpenReadStream().Read(buffer, 0, buffer.Length);
                        if (_fileService.IsImageFile(buffer, file.ContentType, file.FileName))
                        {
                            //_fileService.UploadFile (buffer, file.ContentType, file.FileName);
                            Image img = new Image(user, file.FileName, file.ContentType);
                            img.Contents = buffer;
                            await _fileService.UploadFile(img);
                            model.ProfilePicture = file.FileName;
                            user.ProfilePicture = img;
                        }
                        else
                            ModelState.AddModelError("", "Unable to upload profile picture - invalid image file");
                    }
                }

                Microsoft.Extensions.Primitives.StringValues branchId;
                Request.Form.TryGetValue("branch", out branchId);

                if (branchId.Count == 1 && branchId != "Branch:")
                {
                    defaultOU = Guid.Parse(branchId);
                    OrganisationalUnit DefaultOU = await _organisationalUnitService.GetOrganisationalUnit(defaultOU);
                    user.DefaultOU = DefaultOU;
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Description = model.Description;
                user.EmployeeNumber = model.EmployeeNumber;
                user.JobTitle = model.JobTitle;
                user.SalesPersonUserName = model.SalesPersonUserName;

                await _userService.Update(user);
                
            }
            catch (Exception ex)
            {
                
            }
            return Redirect("~/Account/ProfileEditor");
        }

		[HttpGet]
		public async Task<IActionResult> ChangeOwnPassword()
		{
			return PartialView ("_ChangeOwnPassword");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangeOwnPassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
				return PartialView ("_ChangeOwnPassword", model);

			if (model.NewPassword != model.ConfirmNewPassword) {
				ModelState.AddModelError ("ConfirmNewPassword", "Passwords do not match");
				return PartialView ("_ChangeOwnPassword", model);
			}
			if (model.CurrentPassword == model.NewPassword) {
				ModelState.AddModelError ("NewPassword", "New password needs to be different from the current password");
				return PartialView ("_ChangeOwnPassword", model);
			}

			try
			{
                throw new Exception("this method needs to be re-written in core");
            }
			catch (Exception ex) {
				ModelState.AddModelError ("", ex.Message);
			}
			return PartialView ("_ChangeOwnPassword", model);
		}

		[HttpGet]
		public async Task<IActionResult> ManageUser (Guid Id)
		{
            var user = await _userService.GetUserById(Id);
            var accountModel = new ManageUserViewModel(user);
            //accountModel.UserGroups = new SelectUserGroupsViewModel(user, _permissionsService.GetAllGroups());

            SingleUseToken passwordToken = null;// _authenticationService.GetTokensFor(Id).OrderByDescending(t => t.DateCreated.GetValueOrDefault()).FirstOrDefault();
            if (passwordToken != null)
            {
                accountModel.AccountStatus.LastPasswordResetIssued = passwordToken.DateCreated.GetValueOrDefault().ToString("f");
                accountModel.AccountStatus.PasswordResetExpiryDate = passwordToken.DateCreated.GetValueOrDefault().AddHours(passwordToken.Duration).ToString("f");
                if (passwordToken.Used)
                    accountModel.AccountStatus.PasswordResetStatus = "Used";
                else if (passwordToken.DateCreated.GetValueOrDefault().AddHours(passwordToken.Duration) < DateTime.UtcNow)
                    accountModel.AccountStatus.PasswordResetStatus = "Expired";
                else
                    accountModel.AccountStatus.PasswordResetStatus = "Active";
            }

            //accountModel.UserGroups = new SelectUserGroupsViewModel(user, _permissionsService.GetAllGroups());
            return View(accountModel);
        }
    }
}
