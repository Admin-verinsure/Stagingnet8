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

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class AuthorizeController : BaseController
    {
        IClaimService _claimService;
        IClaimTemplateService _claimTemplateService;
        RoleManager<IdentityRole> _roleManager;
        UserManager<IdentityUser> _userManager;
        
        IOrganisationService _organisationService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<AuthorizeController> _logger;

        public AuthorizeController(
            IUserService userService, 
            IClaimService claimService, 
            IClaimTemplateService claimTemplateService, 
            RoleManager<IdentityRole> roleManager, 
            UserManager<IdentityUser> userManager, 
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
            _roleManager = roleManager;
            _claimService = claimService;
            _claimTemplateService = claimTemplateService;
            _userService = userService;
        }

        // GET: Authorize
        public async Task<IActionResult> Index()
        {
            User user = null;
            try
            {
                user = await CurrentUser();

                var userList = await _userService.GetAllUsers();
                var orderedUserList = userList.OrderBy(u => u.UserName).ToList();
                var roleList = new List<IdentityRole>();

                var claimList = await _claimService.GetClaimsAllClaimsList();
                if (claimList.Count == 0)
                {
                    await _claimTemplateService.CreateAllClaims();
                    claimList = await _claimService.GetClaimsAllClaimsList();
                }

                AuthorizeViewModel model = new AuthorizeViewModel
                {
                    RoleList = await _roleManager.Roles.ToListAsync(),
                    UserList = orderedUserList,
                    ClaimList = claimList,
                    RoleClaims = new Dictionary<string, List<string>>()
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

                if (user != null)
                {
                    var identityUser = await _userManager.FindByNameAsync(user.UserName);

                    model.IsTCUser = await _userManager.IsInRoleAsync(identityUser, "TCUser");
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
            catch(Exception ex)
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
                if(identityUser != null)
                {
                    foreach (var cl in Claims)
                    {
                        var claim = new Claim(cl, cl);
                        await _userManager.AddClaimAsync(identityUser, claim);
                    }
                }

                return Ok();
            }
            catch(Exception ex)
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
            catch(Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
                return NoContent();
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
        public async Task<IActionResult> CreateClaim(string claimType, string claimValue)
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


    }
}