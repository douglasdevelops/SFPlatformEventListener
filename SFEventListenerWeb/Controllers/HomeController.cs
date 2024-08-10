using Microsoft.AspNetCore.Mvc;
using SFEventListenerWeb.Models;
using System.Diagnostics;

namespace SFEventListenerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            string accessToken = HttpContext.Request.Cookies["OAuthAccessToken"];
            string instanceUrl = HttpContext.Request.Cookies["InstanceUrl"];            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}