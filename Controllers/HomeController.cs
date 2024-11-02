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
        private readonly IStedsnavnService _stedsnavnService;

        private static List<PositionModel> positions = new List<PositionModel>();   

        public HomeController(ILogger<HomeController> logger, IKommuneInfoService kommuneInfoService, IStedsnavnService stedsnavnService)
        {
            _logger = logger;
            _kommuneInfoService = kommuneInfoService;
            _stedsnavnService = stedsnavnService;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                UserName = HttpContext.Session.GetString("UserName")
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> KommuneInfo(string kommuneNr)
        {
            if (string.IsNullOrEmpty(kommuneNr))
            {
                ViewData["Error"] = "Please enter a valid Kommune Number.";
                return View("Index");
            }
            var kommuneInfo = await _kommuneInfoService.GetKommuneInfoAsync(kommuneNr);
            if (kommuneInfo != null) {
                var viewModel = new KommuneInfoViewModel {
                    Kommunenavn = kommuneInfo.Kommunenavn,
                    Kommunenummer = kommuneInfo.Kommunenummer,
                    Fylkesnavn = kommuneInfo.Fylkesnavn,
                    SamiskForvaltningsomrade = kommuneInfo.SamiskForvaltningsomrade
                };
                return View("KommuneInfo", viewModel);
            }
            ViewData["Error"] = $"No results found for Kommune Number '{kommuneNr}'.";
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Stedsnavn(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                ViewData["Error"] = "Please enter a valid place name.";
                return View("Index");
            }
            var stedsnavnResponse = await _stedsnavnService.GetStedsnavnAsync(searchTerm);
            if (stedsnavnResponse?.Navn != null && stedsnavnResponse.Navn.Any()) {
                var viewModel = stedsnavnResponse.Navn.Select(navn => new StedsnavnViewModel {
                    Skrivemåte = navn.Skrivemåte,
                    Navneobjekttype = navn.Navneobjekttype,
                    Språk = navn.Språk,
                    Navnestatus = navn.Navnestatus,
                    Stedstatus = navn.Stedstatus
                }).ToList();
                return View("Stedsnavn", viewModel);
            }
            ViewData["Error"] = $"No results found for '{searchTerm}'.";
            return View("Index");
        }

        [HttpGet]
        public ViewResult RegistrationForm()
        {
            return View();
        }

        [HttpPost]
        public ViewResult RegistrationForm(UserData userData)
        {
            return View("Overview", userData);
        }

        [HttpGet]
        public IActionResult CorrectMap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CorrectMap(PositionModel model)
        {
            if (ModelState.IsValid)
            {
                positions.Add(model);

                return View("CorrectionOverview", positions);
            }
            return View();
        }

        [HttpGet]
        public IActionResult CorrectionOverview()
        {
            return View(positions);
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
