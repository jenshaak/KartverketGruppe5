using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class MinProfilController : Controller
    {
        private readonly BrukerService _brukerService;

        public MinProfilController(BrukerService brukerService)
        {
            _brukerService = brukerService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var brukerEmail = HttpContext.Session.GetString("BrukerEmail");
            if (string.IsNullOrEmpty(brukerEmail))
            {
                return RedirectToAction("Login", "Auth");
            }

            var bruker = await _brukerService.GetBrukerByEmail(brukerEmail);

            if (bruker == null)
            {
                return NotFound();
            }

            return View(bruker);

        }
    }
}
