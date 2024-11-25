using KartverketGruppe5.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace KartverketGruppe5.Controllers
{
    /// <summary>

    public class HomeController : Controller
    {
        /// <summary>
        /// Konstruktør for HomeController
        /// </summary>
        public HomeController()
        {
        }

        /// <summary>
        /// Viser hjemmesiden
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {   
            return View();
        }

        /// <summary>
        /// Viser feilside
        /// </summary>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Viser tilgangsfeilside
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
