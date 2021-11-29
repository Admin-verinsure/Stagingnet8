using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces;

namespace DealEngine.WebUI
{
    public class ErrorController : Controller
    {
        
        // GET: Error
        [ActionName ("Error403")]
		[HttpGet]
		public async Task<IActionResult> Error403 ()
		{
            Response.StatusCode = 403;
			return View ();
		}

		// GET: Error
		[ActionName("Error404")]
		[HttpGet]
        public async Task<IActionResult> Error404 ()
        {
            Response.StatusCode = 404;
            return View();
        }

        // GET: Error
		[ActionName("Error500")]
		[HttpGet]
        public async Task<IActionResult> Error500 ()
        {
            Response.StatusCode = 500;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> UserWithNoOrganisation()
        {
            return View();
        }
        [HttpGet]
		public ViewResult Antiforgery ()
		{
            return View();
		}

		[HttpGet]
		public ViewResult InvalidToken ()
		{
            return View ();
		}

		[HttpGet]
		public ViewResult InvalidPasswordReset ()
		{
            return View ();
		}

		[HttpGet]
		public async Task<IActionResult> DangerousRequest ()
		{
            Response.StatusCode = 400;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentRequestError()
        {
            //"bc3c9972 - 1733 - 41a1 - 8786 - fa22229c66f8"
            //Console.WriteLine(Id);
            //ClientInformationSheet sheet = _clientInformationService.GetInformation(Id);
            //InformationViewModel model = new InformationViewModel();
            //Response.StatusCode = 400;

            return View();
        }

    }
}