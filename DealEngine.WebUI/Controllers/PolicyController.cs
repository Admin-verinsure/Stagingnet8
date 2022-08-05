using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DealEngine.WebUI.Models.Policy;
using System;

namespace DealEngine.WebUI.Controllers
{
    public class PolicyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Consumes("application/xml")]
        public async Task<IActionResult> GetAccountResponse(string xml)
        {
            // Do stuff with it
            Console.WriteLine("Hello World");
            Console.WriteLine(xml);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Consumes("application/xml")]
        public async Task<IActionResult> PostAccountResponse(string xml)
        {
            // Do stuff with it
            Console.WriteLine("Hello World");
            Console.WriteLine(xml);

            return Ok();
        }

        //[HttpGet]
        //[AllowAnonymous]
        //[Consumes("application/xml")]
        //public async Task<IActionResult> GetAccountResponse(GetAccountNotificationResponse response)
        //{
        //    // Do stuff with it
        //    Console.WriteLine("Hello World 2");
        //    Console.WriteLine(response);

        //    return Ok();
        //}
    }
}
