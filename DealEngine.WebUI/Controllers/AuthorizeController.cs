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
                var roleList = new List<IdentityRole>();

                var claimList = await _claimService.GetClaimsAllClaimsList();
                if (claimList.Count == 0)
                {
                    await _claimTemplateService.CreateAllClaims();
                    claimList = await _claimService.GetClaimsAllClaimsList();
                }

                AuthorizeViewModel model = new AuthorizeViewModel();

                model.RoleList = new List<IdentityRole>();
                model.UserList = new List<User>();
                model.ClaimList = claimList;
                model.RoleList = await _roleManager.Roles.ToListAsync();

                if (userList.Count != 0)
                {
                    model.UserList = userList;
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
        public async Task<IActionResult> AddRole(string RoleName, string[] Claims, string OrganisationId)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                var isRole = await _roleManager.RoleExistsAsync(RoleName);
                var organisation = user.PrimaryOrganisation;

                if (OrganisationId != null)
                {
                    organisation = await _organisationService.GetOrganisation(Guid.Parse(OrganisationId));
                }

                if (!isRole)
                {
                    var role = new IdentityRole
                    {
                        Name = RoleName
                    };

                    var identityresult = await _roleManager.CreateAsync(role);
                    if (identityresult.Succeeded)
                    {
                        foreach (var cl in Claims)
                        {
                            var claim = new Claim(cl, cl);
                            await _roleManager.AddClaimAsync(role, claim);
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
        public async Task<IActionResult> UpdateRole(string RoleName, string[] Claims)
        {
            User user = null;
            try
            {
                user = await CurrentUser();
                var isRole = await _roleManager.RoleExistsAsync(RoleName);
                if (isRole)
                {
                    var role = await _roleManager.FindByNameAsync(RoleName);
                    var claimList = await _roleManager.GetClaimsAsync(role);

                    foreach (var claim in claimList)
                    {
                        await _roleManager.RemoveClaimAsync(role, claim);
                    }

                    foreach (var cl in Claims)
                    {
                        var template = await _claimService.GetTemplateByName(cl);
                        var claim = new Claim(template.Value, template.Value);
                        await _roleManager.AddClaimAsync(role, claim);
                    }

                    return Ok();
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

                var userDtos = usersInRole.Select(u => new
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
                    return BadRequest("A claim with this value already exists.");
                }

                // Create a new claim instance
                var claim = new Domain.Entities.Claim(claimType, claimValue);

                // Add the claim using the ClaimService
                await _claimService.AddClaim(claim);

                // Return a success response
                return Ok(new { Message = "Claim created successfully!" });
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, null, HttpContext);
                return BadRequest("Error creating the claim.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteClaim(string claimType, string claimValue)
        {
            try
            {
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


    }
}