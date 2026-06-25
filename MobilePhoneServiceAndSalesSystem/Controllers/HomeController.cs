using Microsoft.AspNetCore.Mvc;
using MobilePhoneServiceAndSalesSystem.Models;
using System.Diagnostics;

namespace MobilePhoneServiceAndSalesSystem.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("")]
        [Route("/")]
        public IActionResult Index()
        {
            _logger.LogDebug("Index page accessed");
            return View();
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        public IActionResult Error()
        {
            _logger.LogWarning("Error page displayed with RequestId: {RequestId}", 
                Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
