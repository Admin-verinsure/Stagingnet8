
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using System;
using Microsoft.AspNetCore.Mvc;
using DealEngine.WebUI.Models.Milestone;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class MilestoneController : BaseController
    {
        IMilestoneService _milestoneService;
        IProgrammeService _programmeService;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<MilestoneController> _logger;

        public MilestoneController(
            ILogger<MilestoneController> logger,
            IApplicationLoggingService applicationLoggingService,
            IUserService userRepository,
            IProgrammeService programmeService,            
            IMilestoneService milestoneService)
            : base(userRepository)
        {
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;            
            _programmeService = programmeService;                        
            _milestoneService = milestoneService;
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                if (user.IsLoggedout)
                    return PageNotFound();

                if (user == null)
                    return PageNotFound();

                var programmeList = await _programmeService.GetAllProgrammes();
                MilestoneViewModel model = new MilestoneViewModel(programmeList);               

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> GetMilestone(IFormCollection collection)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                string JsonString = await _milestoneService.GetMilestone(collection);
                return Json(JsonString);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostMilestone(IFormCollection collection)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                await _milestoneService.CreateMilestone(user, collection);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTask(IFormCollection collection)
        {
            User user = null;

            try
            {
                user = await CurrentUser();
                await _milestoneService.RemoveTask(user, collection);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }
    }
}
