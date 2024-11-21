using KartverketGruppe5.Models;
using KartverketGruppe5.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KartverketGruppe5.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IKommuneInfoService _kommuneInfoService;

        private static List<LokasjonModel> positions = new List<LokasjonModel>();   

        public HomeController(ILogger<HomeController> logger, IKommuneInfoService kommuneInfoService)
        {
            _logger = logger;
            _kommuneInfoService = kommuneInfoService;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                UserName = HttpContext.Session.GetString("UserName")
            };
            
            return View(viewModel);
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
