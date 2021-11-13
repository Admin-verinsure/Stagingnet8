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
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Principal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using IO.Swagger;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using System.Diagnostics;
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
            IAppSettingService appSettingService) : base(userService)
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
            DealEngine.Domain.Entities.User user = null;
            string errorMessage = @"We have sent you an email to the email address we have recorded in the system, that email address is different from the one you supplied. 
				Please check the other email addresses you may have used. If you cannot locate our email, 
				please go to https://techcertain.com/helpdesk-form and file a Helpdesk ticket with your contact details, we can re-establish your account with your broker.";
            try
            {
                if (!string.IsNullOrWhiteSpace(viewModel.Email))
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
                        catch (Exception ex)
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
            catch (System.Net.Mail.SmtpFailedRecipientsException exception)
            {
                await _applicationLoggingService.LogWarning(_logger, exception, user, HttpContext);
                ModelState.AddModelError("FailureMessage", errorMessage);
                return View(viewModel);
            }
            catch (MailKit.Net.Smtp.SmtpCommandException ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                ModelState.AddModelError("FailureMessage", "Oops, Email services are currently unavailable. The technical support staff have also been notified, and your password reset email will be sent once services have been restored.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                Exception exception = ex;
                while (exception.InnerException != null) exception = exception.InnerException;

                await _emailService.ContactSupport(_emailService.DefaultSender, exception.GetType().Name + ": " + exception.Message, "");

                ModelState.AddModelError("FailureMessage", errorMessage);
                if (exception is MultipleUsersFoundException)
                    ModelState.AddModelError("FailureMessage", "We were unable to generate a password reset email for you.");
                return View(viewModel);
            }

            return View();
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
            SingleUseToken st = _authenticationService.GetToken(id);
            DealEngine.Domain.Entities.User user = await _userService.GetUserById(st.UserID);
            try
            {
                if (id == Guid.Empty)
                    // if we get here - either invalid guid or invalid token - 404
                    return PageNotFound();

                if (viewModel.Password != viewModel.PasswordConfirm)
                {
                    ModelState.AddModelError("passwordConfirm", "Passwords do not match");
                    return View();
                }

                if (user == null)
                    // in theory, we should never get here. Reason being is that a reset request should not be created without a valid user
                    throw new Exception(string.Format("Could not find user with ID {0}", st.UserID));


                if (user != null)
                {
                    string username = user.UserName;

                    //change the users password using admin
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
                ModelState.AddModelError("passwordConfirm", "Your chosen password does not meet the requirements of our password policy. Please refer to the policy above to assist with creating an appropriate password.");
            }
            catch (Exception ex)
            {

                _ldapService.ChangePassword(user.UserName, "", _appSettingService.IntermediatePassword);

                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                ModelState.AddModelError("passwordConfirm", "There was an error while trying to change your password. Please try again with a new password below.");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PasswordChanged()
        {
            return View();
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

            string nameExtension = _appSettingService.AuthenticationService;

            return View("Login" + nameExtension, viewModel);
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
                int resultCode = -1;
                string resultMessage = "";
                IdentityUser deUser;

                // Step 1 validate in  LDap 
                _ldapService.Validate(userName, password, out resultCode, out resultMessage);
                if (resultCode == 0)
                {
                    var user = await _userService.GetUser(userName);
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
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        user.IsLoggedout = false;
                        user.LoggedInTime = DateTime.UtcNow;
                        await uow.Commit();
                    }
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
                    ModelState.AddModelError(string.Empty, "We are unable to access your account with the username or password provided. You may have entered an incorrect password, or your account may be locked due to an extended period of inactivity. Please try entering your username or password again, or go to https://techcertain.com/helpdesk-form and file a Helpdesk ticket.");
                    return View(viewModel);
                }
            }
            catch (UserImportException ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                await _emailService.ContactSupport(_emailService.DefaultSender, "DealEngine - User Import Error", ex.Message);
                ModelState.AddModelError(string.Empty, "We have encountered an error importing your account. DealEngine has been notified, and will be in touch shortly to resolve this error.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                throw new Exception(ex.Message + " " + ex.StackTrace);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginOkta(AccountLoginModel viewModel)
        {
            //Old login method
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
                int resultCode = -1;
                string resultMessage = "";
                IdentityUser deUser;

                // Step 1 validate in  LDap 
                _ldapService.Validate(userName, password, out resultCode, out resultMessage);
                if (resultCode == 0)
                {
                    // Get the User
                    var user = await _userService.GetUser(userName);

                    // Check if they have an OktaID
                    if (user.OktaUID == null)
                    {
                        var result = await AMPSCreateUser(user, password);
                        // AMPS Create User API

                        // Update User table with OktaUID

                        // Redirect User to oktacallbackservice for Login
                    }
                    else
                    {
                        // Redirect User to oktacallbackservice for Login
                        return Redirect("https://localhost:5001/Home/Index");
                    }
                }
            }
            catch
            {

            }

            //Create User AMPS

            return Ok();
        }

        //[Authorize]
        //private async Task<string> AMPSCreateUserSwagger(DealEngine.Domain.Entities.User user, string password)
        //{
        //    // AMPS API Details
        //    // Configure HTTP basic authorization: Basic_Auth
        //    Configuration apiConfiguration = new Configuration();
        //    apiConfiguration.Username = "ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq"; // ClientID
        //    apiConfiguration.Password = "MDjxj51RufK01vmt"; // ClientSecret

        //    ApiClient apiClient = new ApiClient();
        //    apiClient.Configuration = apiConfiguration;

        //    var apiInstance = new ApiAccessTokenApi();
        //    var grantType = "client_credentials";  // String | value should be client_credentials
        //    var appId = "ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq";  // String | Consuming application's Id

        //    try
        //    {
        //        // Generate apigee access token
        //        AccessTokenJson result = apiInstance.getApiAccessToken(grantType, appId);
        //        Debug.WriteLine(result);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Print("Exception when calling ApiAccessTokenApi.getApiAccessToken: " + e.Message );
        //    }


        //    // Client ID: ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq
        //    // Client Secret: MDjxj51RufK01vmt
        //    // Endpoint: marshdev-mmc.oktapreview.com
        //    // https://stackoverflow.com/questions/58014360/how-do-you-use-basic-authentication-with-system-net-http-httpclient
        //    // https://developer.okta.com/docs/guides/implement-grant-type/clientcreds/main/#flow-specifics

        //    // 1 Get Bearer token from https://marshdev-mmc.oktapreview.com/amps/v2/oauth/accesstoken
        //    var ampsUrl = "https://dev.api.m2digitalbroker.com/proxy/";




        //    return "string";
        //}

//        static void RetrieveOktaUsers()
//        {
//            string BASE_URL = “”;
//            //
//            string CLIENT_ID = “clientid”;
//            string YOUR_CLIENT_SECRET = “secret”;

//            string OAUTH_ENDPOINT = "oauth2/default/v1/token";
//            string USERS_ENDPOINT = "api/v1/users";
//            string AUDIENCE = "";

//            OktaToken oktaToken = null;
//            //call 1
//            using (HttpClient httpClient = new HttpClient())
//            {
//                httpClient.BaseAddress = new Uri(BASE_URL);
//                var authToken = Encoding.ASCII.GetBytes($"{CLIENT_ID}:{YOUR_CLIENT_SECRET}");
//                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(“Basic”,
//                Convert.ToBase64String(authToken));
//                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(“application / json”));
//                FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(new
//                {
//                    new KeyValuePair<string, string>(“grant_type”, “client_credentials”),
//                    new KeyValuePair<string, string>(“scope”, “access_token”),
//                    new KeyValuePair<string, string>(“audience”, “api://default”),
//});

//                HttpResponseMessage response = httpClient.PostAsync(OAUTH_ENDPOINT, formUrlEncodedContent).Result;
//                //response.EnsureSuccessStatusCode();
//                var resp = response.Content.ReadAsStringAsync().Result;
//                oktaToken = JsonConvert.DeserializeObject<OktaToken>(resp);

//            }

//            using (HttpClient httpClient = new HttpClient()) //Call 2
//            {
//                httpClient.BaseAddress = new Uri(BASE_URL);
//                httpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {oktaToken.access_token}");
//                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                HttpResponseMessage response = httpClient.GetAsync(USERS_ENDPOINT).Result;
//                //response.EnsureSuccessStatusCode();
//                var resp = response.Content.ReadAsStringAsync().Result;
//            }
//        }

        //[HttpPost]
        [AllowAnonymous]
        //[Authorize]
        private async Task<string> AMPSCreateUser(DealEngine.Domain.Entities.User user, string password)
        {
            // AMPS API Details

            // Client ID: ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq
            // Client Secret: MDjxj51RufK01vmt
            // Endpoint: marshdev-mmc.oktapreview.com
            // https://stackoverflow.com/questions/58014360/how-do-you-use-basic-authentication-with-system-net-http-httpclient
            // https://developer.okta.com/docs/guides/implement-grant-type/clientcreds/main/#flow-specifics
            
            // 1 Get Bearer token from https://marshdev-mmc.oktapreview.com/amps/v2/oauth/accesstoken
            var ampsUrl = "https://dev.api.m2digitalbroker.com/proxy/";



            // Setup client
            HttpClient client = new HttpClient();
            Uri ampsUri = new Uri(ampsUrl);
            client.BaseAddress = ampsUri;
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.ConnectionClose = true;

            // Post body content
            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            values.Add(new KeyValuePair<string, string>("appId", "ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq"));
            //values.Add(new KeyValuePair<string, string>("scope", "access_token"));
            var content = new FormUrlEncodedContent(values);


            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = ampsUri;
                //var authToken = Encoding.ASCII.GetBytes("ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq:MDjxj51RufK01vmt");
                //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                HttpResponseMessage response = httpClient.PostAsync("amps/v2/oauth/accesstoken", content).Result;
                //response.EnsureSuccessStatusCode();
                var resp = response.Content.ReadAsStringAsync().Result;

                //oktaToken = JsonConvert.DeserializeObject<OktaToken>(resp);

            }







            var authenticationString = "ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq:MDjxj51RufK01vmt";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/amps/v2/oauth/accesstoken");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            
            // Add Headers to HTTP Message
            //requestMessage.Headers.Add("appId", "ecq9V461WeyGzzGYPmT1ALlxXAlDbtkq");
            //requestMessage.Headers.Add("grant_type", "client_credentials");
            requestMessage.Content = content;
            
            // Send Request
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            var test = "";

            // Test if you even get a response

                     
            // Get Response
            //var response = task.Result;
            //Console.WriteLine(response);            
            //response.EnsureSuccessStatusCode();          
            //string responseBody = response.Content.ReadAsStringAsync().Result;
            //Console.WriteLine(responseBody);

            client.Dispose();
                      
            // Create User API

            // Setup client
            HttpClient client2 = new HttpClient();
            client2.BaseAddress = ampsUri;
            client2.DefaultRequestHeaders.Clear();
            client2.DefaultRequestHeaders.ConnectionClose = true;
            client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
            client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Post body content

            #region Create JSON objects
            JObject body =
                new JObject(
                    new JProperty("roptions",
                        new JObject(
                            new JProperty("activate", true)
                        )
                    ),
                    new JProperty("profile",
                        new JObject(
                            new JProperty("firstName", user.FirstName),
                            new JProperty("lastName", user.LastName),
                            //new JProperty("mobilePhone", user.MobilePhone),
                            //new JProperty("secondEmail", null),
                            new JProperty("login", user.Email),
                            new JProperty("email", user.Email)
                        )
                    ),
                    new JProperty("credentials",
                        new JObject(
                            new JProperty("password",
                                new JObject(
                                    new JProperty("value", password)
                                )
                            )
                        )
                    ),
                    new JProperty("groupIds",
                        new JObject(
                            new JProperty("00g11paplp3Yz4Qlz0h8")//, //TCDE_External_Users
                                                                 //new JProperty("00g11pb30arN7hD3h0h8"), //TCDE_Internal_Users
                                                                 //new JProperty("00g11pcdujzbgE3ur0h8")  //TCDE_UW_NZI
                        )
                    )
                );
            #endregion
            var data = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            //var content = new FormUrlEncodedContent(values);

            var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/amps/v2/uam/users");

            // Add Headers to HTTP Message
            requestMessage2.Content = data;

            var task2 = client2.SendAsync(requestMessage2);
            var response2 = task2.Result;

            //Console.WriteLine(response);
            //response.EnsureSuccessStatusCode();

            //string responseBody = response.Content.ReadAsStringAsync().Result;
            //Console.WriteLine(responseBody);


            //var response2 = await client.PostAsync(createUserURL, data);

            client2.Dispose();

            //Console.WriteLine(response);

            return "";//response.StatusCode.ToString();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginOktaAsync2(string json)
        {
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            // string username = dict["name"] + dict["okta_uid"].Substring(dict["okta_uid"].Length - 4);
            // string username = dict["firstname"].Substring(0,1) + dict["surname"] + dict["id"].Substring(dict["id"].Length - 4);
            string okta_uid = dict["okta_uid"];
            string email = dict["email"];
            string identityPassword = dict["okta_uid"] + _appSettingService.OktaIntermediatePassword;

            // Will have to run kestrel to see this if possible
            Console.WriteLine(okta_uid, identityPassword, email);

            // See if user is in database already, if so grab them, if not it will end up creating them if they already exist in LDAP (also checks LDAP for them, so if they existed in LDAP they will be created)
            // var user = await _userService.GetUser(username);

            // TODO
            // var user = await _userService.GetMarshUser(okta_uid);

            // UPDATE EMAIL WITH OKTA EMAIL CLAIM? Will only update it if it's a new user though... If they already exist then it won't update it.
            //user.Email = email;

            //// See if identity user is in database, if so grab them, if not create them with password provided, then log them in
            //var identityResult = await DealEngineIdentityUserLogin(user, identityPassword);

            //// If the password worked then we are happy, if it didn't update the password and login
            //if (identityResult.Succeeded)
            //{
            //    // That's what we wanted so can return now
            //}
            //else
            //{
            //    IdentityUser deUser = await _userManager.FindByNameAsync(username);
            //    await _userManager.RemovePasswordAsync(deUser);
            //    await _userManager.AddPasswordAsync(deUser, password);
            //    await _signInManager.PasswordSignInAsync(deUser, password, true, lockoutOnFailure: true);
            //}

            return Ok();
        }

        

        private async Task<SignInResult> DealEngineIdentityUserLogin(DealEngine.Domain.Entities.User user, string password)
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

            catch (Exception ex)
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
                    MarshRsaAuthProvider rsaAuth = new MarshRsaAuthProvider(_logger, _httpClientService, _emailService, _appSettingService);
                    MarshRsaUser rsaUser = rsaAuth.GetRsaUser(user.Email);
                    rsaUser.DevicePrint = viewModel.DevicePrint;
                    rsaUser.Email = user.Email;
                    //rsaUser.Username = user.UserName;
                    rsaUser.Username = rsaAuth.GetHashedId(user.UserName.ToLower() + "@mnzconnect.com"); //use hashed username(lower case) + production domain name as requested by Marsh
                    rsaUser.HttpReferer = "~Account/LoginMarsh";
                    rsaUser.OrgName = _appSettingService.MarshRSAOrgName; //staging:Marsh_Model, production: Marsh
                    rsaUser.RsaStatus = RsaStatus.Deny;
                    rsaUser.DeviceTokenCookie = user.DeviceTokenCookie;
                    if (!string.IsNullOrEmpty(user.DeviceTokenCookie)) //As marsh adviced ClientGenCookie is a Mandatory field for none-enrollment request, use DeviceTokenCookie to define the enrollment status
                    {
                        rsaUser.ClientGenCookie = Guid.NewGuid().ToString();
                    }
                    else
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
                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {
                            user.IsLoggedout = false;
                            user.LoggedInTime = DateTime.UtcNow;
                            await uow.Commit();
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
                    if (rsaUser.RsaStatus == RsaStatus.Deny)
                    {
                        //email the notification
                        _emailService.RsaNotificationEmail(_appSettingService.MarshRSANotificationEmail, user.UserName + "@mnzconnect.com");

                        return Redirect("~/Account/RSAErrorMessage");

                        await Logout();
                    }
                }
                ModelState.AddModelError(string.Empty, "We are unable to access your account with the username or password provided. You may have entered an incorrect password, or your account may be locked due to an extended period of inactivity. Please try entering your username or password again, or go to https://techcertain.com/helpdesk-form and file a Helpdesk ticket.");
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
        public async Task<IActionResult> OneTimePasswordMarsh(RsaOneTimePasswordModel viewModel)
        {
            if (ModelState.IsValid)
            {
                MarshRsaAuthProvider rsaAuth = new MarshRsaAuthProvider(_logger, _httpClientService, _emailService, _appSettingService);
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
                //bool isAuthenticated = await rsaAuth.Authenticate(rsaUser, _userService, username);                
                //if (isAuthenticated)
                string authenticatedStatus = await rsaAuth.AuthenticateStatus(rsaUser, _userService, username);
                if (authenticatedStatus == "SUCCESS")
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
                    using (var uow = _unitOfWork.BeginUnitOfWork())
                    {
                        user.IsLoggedout = false;
                        user.LoggedInTime = DateTime.UtcNow;
                        await uow.Commit();
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (authenticatedStatus == "FAIL")
                {
                    return Redirect("~/Account/OTPFailMessage");
                    await Logout();
                }
                else if (authenticatedStatus == "LOCKOUT")
                {
                    ViewBag.AccountLocked = "Unfortunately the account that you are trying to access has been locked and will require assistance from the Marsh IT Support team to be reset. The support team have been notified and Marsh will be in contact with you to let you know when this has been resolved. This is nothing to do with TechCertain - please do not file a ticket with TechCertain.";

                    //email the notification
                    _emailService.RsaNotificationEmail(_appSettingService.MarshRSANotificationEmail, username + "@mnzconnect.com");

                    await Logout();
                }
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> OTPFailMessage()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RSAErrorMessage()
        {
            return View();
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
            var currentUser = await CurrentUser();

            await _signInManager.SignOutAsync();
            ///required for white hat fix for session .following 2 further calls were added to deal with OWASP cookies vulnerability.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Identity.Application");
            using (var uow = _unitOfWork.BeginUnitOfWork())
            {
                currentUser.IsLoggedout = true;
                currentUser.LoggedOutTime = DateTime.UtcNow;
                await uow.Commit();
            }
           
            var identity = new System.Security.Principal.GenericIdentity(HttpContext.User.Identity.Name);
            var principal = new GenericPrincipal(identity, new string[0]);

            var jhgh = User.Identity.IsAuthenticated;
            HttpContext.Session.Clear();
           
            HttpContext.Response.Cookies.Delete(".AspNet.Consent");
            HttpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
          //  Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddYears(-1);

            return await RedirectToLocal();
        }

        void EnsureLoggedOut()
        {
            if (User.Identity.IsAuthenticated)
                Logout();
        }

        //[HttpGet]
        //[ValidateAntiForgeryToken]

        //public async Task<IActionResult> Profile(string id)
        //{
        //    var currentUser = await CurrentUser();
        //    var user = string.IsNullOrWhiteSpace(id) ? currentUser : await _userService.GetUser(id);
        //    //if (!User.Identity.IsAuthenticated)
        //    //    return PageNotFound();
        //    //Logout();
        //    // We do not want to use any existing identity information
        //    if (user == null)
        //        return PageNotFound();

        //    ProfileViewModel model = new ProfileViewModel();

        //    try
        //    {
        //        model.FirstName = user.FirstName;
        //        model.LastName = user.LastName;
        //        model.Email = user.Email;
        //        model.Phone = user.Phone;
        //        model.CurrentUser = currentUser;
        //        model.DefaultOU = user.DefaultOU;
        //        model.EmployeeNumber = user.EmployeeNumber;
        //        model.JobTitle = user.JobTitle;
        //        model.SalesPersonUserName = user.SalesPersonUserName;

        //        if (user.PrimaryOrganisation != null)
        //            model.PrimaryOrganisationName = user.PrimaryOrganisation.Name;
        //        //if (user.Organisations.Count() > 0 && user.Organisations.ElementAt(0).OrganisationType.Name != "personal")
        //        //	model.PrimaryOrganisationName = user.Organisations.ElementAt(0).Name;
        //        model.Description = user.Description;
        //        if (user.ProfilePicture != null)
        //            model.ProfilePicture = user.ProfilePicture.Name;    // TODO - remap this

        //        model.ViewingSelf = string.IsNullOrEmpty(id) || (currentUser.UserName == id);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
        //    }

        //    return View(model);
        //}

        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var currentUser = await CurrentUser();
            var user = string.IsNullOrWhiteSpace(id) ? currentUser : await _userService.GetUser(id);
            if (currentUser.IsLoggedout)
                return Redirect("~/Home/Index");

            if (user == null)
                return PageNotFound();

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
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
            }

            return View(model);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ProfileEditor()
        {
            var user = await CurrentUser();

            if (user.IsLoggedout)
                return Redirect("~/Home/Index");

            if (user == null)
                return PageNotFound();

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
            catch (Exception ex)
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
                return PageNotFound();
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
            return PartialView("_ChangeOwnPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOwnPassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_ChangeOwnPassword", model);

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Passwords do not match");
                return PartialView("_ChangeOwnPassword", model);
            }
            if (model.CurrentPassword == model.NewPassword)
            {
                ModelState.AddModelError("NewPassword", "New password needs to be different from the current password");
                return PartialView("_ChangeOwnPassword", model);
            }

            try
            {
                throw new Exception("this method needs to be re-written in core");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return PartialView("_ChangeOwnPassword", model);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUser(Guid Id)
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
