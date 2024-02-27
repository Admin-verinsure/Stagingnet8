using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models.Authorization;
using IdentityRole = NHibernate.AspNetCore.Identity.IdentityRole;
using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
using Claim = System.Security.Claims.Claim;
using NHibernate.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using NHibernate.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using DealEngine.WebUI.Models;
//using Microsoft.AspNet.Identity;
using DealEngine.Services.Impl;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class AuthorizeController : BaseController
    {
        IClaimService _claimService;
        IEmailService _emailService;
        IClaimTemplateService _claimTemplateService;
        RoleManager<IdentityRole> _roleManager;
        UserManager<IdentityUser> _userManager;
        IUserRoleOrganisationService _userRoleOrganisationService;
        IOrganisationService _organisationService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<AuthorizeController> _logger;

        public AuthorizeController(
            IUserService userService,
            IEmailService emailService,
            IClaimService claimService,
            IClaimTemplateService claimTemplateService,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IUserRoleOrganisationService userRoleOrganisationService,
            IOrganisationService organisationService,
            IApplicationLoggingService applicationLoggingService,
            ILogger<AuthorizeController> logger
            )
            : base(userService)
        {
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;
            _organisationService = organisationService;
            _userManager = userManager;
            _userRoleOrganisationService = userRoleOrganisationService;
            _roleManager = roleManager;
            _claimService = claimService;
            _claimTemplateService = claimTemplateService;
            _userService = userService;
            _emailService = emailService;
        }

        // GET: Authorize
        public async Task<IActionResult> Index()
        {
            User user = null;
            AuthorizeViewModel model = new AuthorizeViewModel();
            IList<Organisation> orderedOrganisationList = null;
            IdentityUser identityUser = null;
            try
            {
                user = await CurrentUser();
                if (user != null)
                {
                    identityUser = await _userManager.FindByNameAsync(user.UserName);

                    model.IsTCUser = await _userManager.IsInRoleAsync(identityUser, "TCUser");
                }


                // Insurer logic is wrong now, we want secondaryOrganisations
                if (model.IsTCUser)
                {
                    // All secondary Orgs
                    var organisationList = await _organisationService.GetAllOrganisations();
                    orderedOrganisationList = organisationList
                        //.Where(o => o.OrganisationType?.Name == "Insurer")
                        .Where(o => o.IsSecondaryOrg == true)
                        .GroupBy(o => o.Name)
                        .Select(group => group.First())
                        .OrderBy(o => o.Name)
                        .ToList();
                }
                else
                {
                    // Use Users SecondaryOrg 
                    // Load only own SecondaryOrg & Organisations for selection

                    Organisation secondaryOrg = user.SecondaryOrganisation;

                    // Set isMarshUser
                    if (secondaryOrg != null && secondaryOrg.Name.Contains("Marsh"))
                    {
                        model.isMarshUser = true;
                    }
                    else { model.isMarshUser = false; }

                    // Add main secondaryOrg
                    IList<Organisation> organisationList = new List<Organisation>
                    {
                        secondaryOrg
                    };

                    // Populate Orgs
                    if (await _userManager.IsInRoleAsync(identityUser, "BrokerAdmin"))
                    {
                        // We want Organisations that this user belongs to (has the org role for)                                             

                        // Check other organisations for SecondaryOrgs
                        foreach (Organisation org in user.Organisations)
                        {
                            if (org.IsSecondaryOrg == true)
                            {
                                string orgRole = org.Name.Replace(" ", "");
                                if (await _userManager.IsInRoleAsync(identityUser, orgRole))
                                {
                                    organisationList.Add(org);
                                }
                            }
                        }
                    }

                    orderedOrganisationList = organisationList.OrderBy(o => o.Name).ToList();
                }

                var userList = await _userService.GetAllUsers();
                var orderedUserList = userList.OrderBy(u => u.UserName).ToList();
                var roleList = new List<IdentityRole>();

                var claimList = await _claimService.GetClaimsAllClaimsList();
                if (claimList.Count == 0)
                {
                    await _claimTemplateService.CreateAllClaims();
                    claimList = await _claimService.GetClaimsAllClaimsList();
                }

                model = new AuthorizeViewModel
                {
                    RoleList = await _roleManager.Roles.ToListAsync(),
                    UserList = orderedUserList,
                    ClaimList = claimList,
                    RoleClaims = new Dictionary<string, List<string>>(),
                    OrganisationList = orderedOrganisationList
                };

                foreach (var role in model.RoleList)
                {
                    var claimsInRole = await _roleManager.GetClaimsAsync(role);
                    model.RoleClaims[role.Name] = claimsInRole.Select(c => c.Value).ToList();
                }

                var roleClaimsDictionary = new Dictionary<string, List<string>>();

                foreach (var role in model.RoleList)
                {
                    var claimsInRole = await _roleManager.GetClaimsAsync(role);
                    roleClaimsDictionary[role.Name] = claimsInRole.Select(c => c.Value).ToList();
                }

                model.RoleClaims = roleClaimsDictionary;

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string RoleName, string[] Claims)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                var isRole = await _roleManager.RoleExistsAsync(RoleName);

                if (!isRole)
                {
                    var role = new IdentityRole
                    {
                        Name = RoleName
                    };

                    var identityresult = await _roleManager.CreateAsync(role);
                    if (identityresult.Succeeded)
                    {
                        if (!Claims.IsNullOrEmpty())
                        {
                            foreach (var cl in Claims)
                            {
                                var claim = new Claim(cl, cl);
                                await _roleManager.AddClaimAsync(role, claim);
                            }
                        }

                        return Ok();
                    }
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
        public async Task<IActionResult> DeleteRoleSelect(string RoleName)
        {

            User user = null;

            try
            {
                user = await CurrentUser();
                var identityUser = await _userManager.FindByNameAsync(user.UserName);

                if (identityUser != null)
                {
                    bool isTCUser = await _userManager.IsInRoleAsync(identityUser, "TCUser");

                    if (!isTCUser)
                    {
                        return BadRequest("User not permitted to perform this action.");
                    }
                }
                else
                {
                    return BadRequest("User is not logged in.");
                }

                var isRole = await _roleManager.RoleExistsAsync(RoleName);
                if (isRole)
                {
                    var role = await _roleManager.FindByNameAsync(RoleName);
                    await _roleManager.DeleteAsync(role);
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
        public async Task<IActionResult> UpdateRole(string RoleName, string[] ClaimsToAdd, string[] ClaimsToRemove)
        {
            User user = null;
            try
            {
                user = await CurrentUser();

                // If they want to change TCUser
                if (RoleName == "TCUser")
                {
                    var identityUser = await _userManager.FindByNameAsync(user.UserName);

                    if (identityUser != null)
                    {
                        bool isTCUser = await _userManager.IsInRoleAsync(identityUser, "TCUser");

                        if (!isTCUser)
                        {
                            return BadRequest("User not permitted to perform this action.");
                        }
                    }
                    else
                    {
                        return BadRequest("User is not logged in.");
                    }
                }

                var isRole = await _roleManager.RoleExistsAsync(RoleName);
                if (isRole)
                {
                    var role = await _roleManager.FindByNameAsync(RoleName);
                    var currentClaims = await _roleManager.GetClaimsAsync(role);

                    // Remove the claims that are in the ClaimsToRemove array
                    foreach (var cTR in ClaimsToRemove)
                    {
                        var claim = currentClaims.FirstOrDefault(c => c.Value == cTR);
                        if (claim != null)
                        {
                            await _roleManager.RemoveClaimAsync(role, claim);
                        }
                    }

                    // Add new claims from the ClaimsToAdd array
                    foreach (var cTA in ClaimsToAdd)
                    {
                        var template = await _claimService.GetTemplateByName(cTA);
                        var claim = new Claim(template.Value, template.Value);
                        // Check if the claim already exists to avoid duplicates
                        if (!currentClaims.Any(c => c.Value == template.Value))
                        {
                            await _roleManager.AddClaimAsync(role, claim);
                        }
                    }

                    return Ok();
                }

                return NotFound("Role not found.");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetClaimsForRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            // Transform the claims into a suitable format for the frontend if necessary
            return Json(claims.Select(c => c.Value));
        }


        [HttpPost]
        public async Task<IActionResult> SaveRoleToUser(Guid UserId, string[] RoleIds)
        {
            User user = null;
            try
            {
                user = await _userService.GetUserById(UserId);
                var identityUser = await _userManager.FindByNameAsync(user.UserName);
                if (identityUser == null)
                {
                    identityUser = new IdentityUser();
                    identityUser.UserName = user.UserName;
                    identityUser.Email = user.Email;
                    await _userManager.CreateAsync(identityUser);
                }

                foreach (var id in RoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(id);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(identityUser, role.Name);
                    }
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
        public async Task<IActionResult> RemoveRoleFromUser(Guid UserId, string[] roleIds)
        {
            User user = null;
            try
            {
                user = await _userService.GetUserById(UserId);
                var identityUser = await _userManager.FindByNameAsync(user.UserName);
                if (identityUser == null)
                {
                    return BadRequest("User not found.");
                }

                foreach (var roleId in roleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role != null)
                    {
                        if (await _userManager.IsInRoleAsync(identityUser, role.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(identityUser, role.Name);
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return BadRequest("Error removing roles from user.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserToClaim(string UserId, string[] Claims)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                var appUser = await _userService.GetUserById(Guid.Parse(UserId));
                var identityUser = await _userManager.FindByNameAsync(appUser.UserName);
                if (identityUser != null)
                {
                    foreach (var cl in Claims)
                    {
                        var claim = new Claim(cl, cl);
                        await _userManager.AddClaimAsync(identityUser, claim);
                    }
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
        public async Task<IActionResult> CreateClaim(IFormCollection form)
        {
            User user = null;
            try
            {
                var claimvalue = form["value"];
                var claimtype = form["type"];
                Domain.Entities.Claim claim = new Domain.Entities.Claim(claimtype, claimvalue);
                await _claimService.AddClaim(claim);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddClaim(string claimType, string claimValue)
        {
            try
            {
                // Check if the claim already exists
                var existingClaim = await _claimService.GetTemplateByName(claimValue);
                if (existingClaim != null)
                {
                    return Json(new { success = false, message = "A claim with this value already exists." });
                }

                // Create a new claim instance
                var claim = new Domain.Entities.Claim(claimType, claimValue);

                // Add the claim using the ClaimService
                await _claimService.AddClaim(claim);

                // Return a success response
                return Json(new { success = true, message = "Claim created successfully!" });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                return Json(new { success = false, message = "Error creating the claim." });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

                var userDtos = usersInRole
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                }).ToList();

                return Json(userDtos);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                return BadRequest("Error retrieving users for the role.");
            }

        }

        [HttpPost]
        public async Task<IActionResult> DeleteClaim(string claimType, string claimValue)
        {
            try
            {
                // Only let TC_USER
                User currentUser = await CurrentUser();
                var identityUser = await _userManager.FindByNameAsync(currentUser.UserName);

                if (identityUser != null) {
                    bool isTCUser = await _userManager.IsInRoleAsync(identityUser, "TCUser");

                    if (!isTCUser)
                    {
                        return BadRequest("User not permitted to perform this action.");
                    }
                }
                else
                {
                    return BadRequest("User is not logged in.");
                }


                // Get the claim by its value
                var claim = await _claimService.GetTemplateByName(claimValue);
                if (claim == null)
                {
                    return BadRequest("Claim not found.");
                }

                // Create a System.Security.Claims.Claim instance for removal
                var systemClaim = new System.Security.Claims.Claim(claimType, claimValue);

                // Remove the claim from all roles that have it
                var roles = _roleManager.Roles.ToList();
                foreach (var role in roles)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    if (roleClaims.Any(c => c.Type == claimType && c.Value == claimValue))
                    {
                        await _roleManager.RemoveClaimAsync(role, systemClaim);
                    }
                }

                // Remove the claim from all users that have it
                var users = _userManager.Users.ToList();
                foreach (var user in users)
                {
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    if (userClaims.Any(c => c.Type == claimType && c.Value == claimValue))
                    {
                        await _userManager.RemoveClaimAsync(user, systemClaim);
                    }
                }

                // Remove the claim from the system using the ClaimService
                await _claimService.RemoveClaim(claimValue);

                // Return a success response
                return Ok(new { Message = "Claim deleted successfully and removed from all roles and users!" });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                return BadRequest("Error deleting the claim.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetailsByLastName(string lastName)
        {
            try
            {
                var appUsers = await _userService.GetUsersByLastName(lastName);
                if (appUsers == null || !appUsers.Any())
                {
                    return NotFound("Users not found");
                }

                var userDetails = new List<object>();

                foreach (var appUser in appUsers)
                {
                    var idenUser = await _userManager.FindByNameAsync(appUser.UserName);
                    if (idenUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(idenUser);
                        userDetails.Add(new
                        {
                            appUser.Id,
                            appUser.UserName,
                            appUser.Email,
                            appUser.FirstName,
                            appUser.LastName,
                            Roles = roles
                        });
                    }
                    else
                    {
                        userDetails.Add(new
                        {
                            appUser.Id,
                            appUser.UserName,
                            appUser.Email,
                            appUser.FirstName,
                            appUser.LastName,
                            Roles = new List<string>()
                        });
                    }
                }

                return Json(userDetails);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                return BadRequest("Error retrieving user details.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetOrganisationDetails([FromBody] IList<Guid> organisationIds)
        {
            try
            {
                IList<string> rolesToAllocate = new List<string>();

                var responseList = new List<OrganisationRolesResponse>();

                foreach (Guid id in organisationIds)
                {
                    var secondaryOrg = await _organisationService.GetOrganisation(id);

                    rolesToAllocate.Clear();
                    if (secondaryOrg.IsBroker)
                    {
                        rolesToAllocate.Add("Broker");
                        rolesToAllocate.Add("SeniorBroker");
                        rolesToAllocate.Add("BrokerAdmin");
                    }
                    if (secondaryOrg.IsInsurer)
                    {
                        rolesToAllocate.Add("Underwriter");
                        rolesToAllocate.Add("SeniorUnderwriter");
                        rolesToAllocate.Add("InsurerAdmin");
                    }
                    if (secondaryOrg.IsAssociation)
                    {
                        rolesToAllocate.Add("AssociationAdmin");
                    }
                    if (secondaryOrg.IsClient)
                    {
                        rolesToAllocate.Add("Client");
                    }
                    if (secondaryOrg.IsInsuredNamedParty)
                    {
                        rolesToAllocate.Add("NamedParty");
                    }
                    if (secondaryOrg.IsCoInsurer)
                    {
                        rolesToAllocate.Add("CoInsuredUnderwriter");
                    }
                    if (secondaryOrg.IsExcessLayerInsurer)
                    {
                        rolesToAllocate.Add("ExcessLayerUnderwriter");
                    }
                    if (secondaryOrg.IsReInsurer)
                    {
                        rolesToAllocate.Add("ReinsurerUnderwriter");
                    }
                    if (secondaryOrg.IsCaptive)
                    {
                        rolesToAllocate.Add("CaptiveUnderwriter");
                    }

                    responseList.Add(new OrganisationRolesResponse
                    {
                        OrganisationId = id,
                        Name = secondaryOrg.Name,
                        Roles = rolesToAllocate.Distinct().ToList()
                    });
                }

                return Ok(responseList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting organisation details: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            // Declare key variables
            string username, firstName, lastName, email, homePhone, mobilePhone;
            string oktaUID, proposalOnlineUsername, salespersonUsername, employeeNumber, password;
            IList<string> rolesForUser = new List<string>();
            IList<OrganisationRoleMapping> organisationRoleMappings = model.OrganisationRoleMappings;
            Organisation secondaryOrganisation = null;
            IList<Organisation> organisations = new List<Organisation>();
            IdentityRole primaryOrganisationRole = null;
            string userType = null;
            IList<IdentityRole> roles = new List<IdentityRole>();

            //userType, organisationId,
            if (model == null)
            {
                _logger.LogWarning("Attempted to create user with null model.");
                return BadRequest("Invalid data. The user data cannot be null.");
            }

            // Assign values from the model
            firstName = model.FirstName;
            lastName = model.LastName;
            email = model.Email;
            homePhone = model.HomePhone;
            mobilePhone = model.MobilePhone;

            try
            {
                _logger.LogInformation("Initiating user creation for: {Email}", model.Email);

                if (model.MainOrganisationId != Guid.Empty)
                {
                    secondaryOrganisation = await _organisationService.GetOrganisation(model.MainOrganisationId);
                }

                oktaUID = model.OktaUID;
                proposalOnlineUsername = model.ProposalOnlineUsername;
                salespersonUsername = model.SalespersonUsername;
                employeeNumber = model.EmployeeNumber;
                password = model.Password;

                if (proposalOnlineUsername == null)
                {
                    // Generate a username
                    Random random = new Random();
                    username = firstName.Replace(" ", string.Empty) + "_" + lastName.Replace(" ", string.Empty) + random.Next(1000);
                }
                else {
                    username = proposalOnlineUsername;
                }


                // Determine UserType for Primary Organization & Add Organisations so can add to User
                foreach (OrganisationRoleMapping organisationRoleMapping in organisationRoleMappings)
                {

                    Organisation org = await _organisationService.GetOrganisation(organisationRoleMapping.OrganisationId);
                    organisations.Add(org);

                    // For the main employing Organisation Set the userType for creating PrimaryOrg
                    if (organisationRoleMapping.OrganisationId == secondaryOrganisation.Id)
                    {
                        var role = await _roleManager.FindByNameAsync(organisationRoleMapping.RoleName);
                        primaryOrganisationRole = role;

                        if (role.Name.Contains("Broker"))
                        {
                            userType = "Broker";
                        }
                        else if (role.Name.Contains("Underwriter") || role.Name.Contains("InsurerAdmin"))
                        { 
                            userType = "Insurer";
                        }
                        else if (role.Name.Contains("Association"))
                        {
                            userType = "Association";
                        }
                        else
                        {
                            userType = "Client";
                        }
                    }
                }

                // Create a new User instance
                User user = new User(null, Guid.NewGuid(), username)
                {
                    FirstName = firstName,
                    LastName = lastName,
                    FullName = firstName + " " + lastName,
                    Email = email,
                    Phone = homePhone,
                    MobilePhone = mobilePhone,
                    SecondaryOrganisation = secondaryOrganisation,
                    Organisations = organisations,
                    OktaUID = oktaUID,
                    SalesPersonUserName = salespersonUsername,
                    EmployeeNumber = employeeNumber,
                    Password = password
                };

                bool createdUser = await _userService.CreateUserPrimaryOrgOrgTypeAndLDAP(user, userType);

                if (!createdUser)
                {
                    _logger.LogWarning("Failed to create user in application and LDAP for: {Email}", model.Email);
                    return BadRequest("Failed to create user.");
                }


                if (createdUser)
                {
                    _logger.LogInformation("User created successfully in application and LDAP for: {Email}", model.Email);
                    var identityUser = new IdentityUser
                    {
                        UserName = username,
                        Email = model.Email
                    };

                    // Create Identity User
                    var identityResult = await _userManager.CreateAsync(identityUser, user.Password);

                    if (!identityResult.Succeeded)
                    {
                        // Handle errors (e.g., return an error response)
                        _logger.LogError("Identity user creation failed for: {Email}. Errors: {Errors}", model.Email, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
                        return BadRequest(identityResult.Errors);
                    }

                    List<string> uniqueRoleNames = organisationRoleMappings
                        .Select(orm => orm.RoleName)
                        .Distinct()
                        .ToList();

                    foreach (string roleName in uniqueRoleNames)
                    {
                        IdentityRole roleToAdd = await _roleManager.FindByNameAsync(roleName);
                        if (roleToAdd != null)
                        {
                            roles.Add(roleToAdd);
                        }
                    }

                    foreach (IdentityRole role in roles)
                    {
                        await _userManager.AddToRoleAsync(identityUser, role.Name);
                    }

                    foreach (OrganisationRoleMapping orm in organisationRoleMappings)
                    {
                        UserRoleOrganisation userRoleOrg = new UserRoleOrganisation(user);
                        //Organisation org = await _organisationService.GetOrganisation(orm.OrganisationId);
                        userRoleOrg.OrganisationId = orm.OrganisationId;
                        IdentityRole getRoleId = await _roleManager.FindByNameAsync(orm.RoleName);
                        userRoleOrg.RoleId = getRoleId.Id;
                        userRoleOrg.User = user;

                        await _userRoleOrganisationService.AddUserRoleOrganisationAsync(userRoleOrg);
                    }

                    _logger.LogInformation("Identity user and roles created successfully for: {Email}", model.Email);
                }

                //await _emailService.CreateUserEmail(user);

                var creatingUser = await CurrentUser();
                //await _emailService.CreateUserCreatedByEmail(user, creatingUser);

                _logger.LogInformation("Completed user creation process for: {Email}", model.Email);
                return Ok(new { message = "User created successfully", username = user.UserName });

            }

            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}