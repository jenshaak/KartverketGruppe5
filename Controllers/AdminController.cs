using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly BrukerService _brukerService;

        public AdminController(BrukerService brukerService)
        {
            _brukerService = brukerService;
        }

        public async Task<IActionResult> Index()
        {
            var brukere = await _brukerService.GetAlleBrukere();
            return View(brukere);
        }
    }
} 