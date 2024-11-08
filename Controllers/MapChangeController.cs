using Microsoft.AspNetCore.Mvc;
using KartverketGruppe5.Models;
using KartverketGruppe5.Services;

namespace KartverketGruppe5.Controllers
{
    public class MapChangeController : Controller
    {
        private readonly LokasjonService _lokasjonService;
        private readonly InnmeldingService _innmeldingService;

        public MapChangeController(LokasjonService lokasjonService, InnmeldingService innmeldingService)
        {
            _lokasjonService = lokasjonService;
            _innmeldingService = innmeldingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CorrectionOverview()
        {
            var lokasjoner = _lokasjonService.GetAllLokasjoner();
            var innmeldinger = lokasjoner.Select(l => new
            {
                Lokasjon = l,
                Innmelding = _innmeldingService.GetInnmeldingForLokasjon(l.LokasjonId)
            }).ToList();

            return View(innmeldinger);
        }

        [HttpPost]
        public async Task<IActionResult> CorrectionOverview(LokasjonModel model, string beskrivelse)
        {
            if (ModelState.IsValid)
            {
                var brukerId = HttpContext.Session.GetInt32("BrukerId");
                if (brukerId == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                try
                {
                    if (string.IsNullOrEmpty(model.GeoJson))
                    {
                        ModelState.AddModelError("", "GeoJson data mangler");
                        return View("Index", model);
                    }

                    int lokasjonId = _lokasjonService.AddLokasjon(
                        model.GeoJson,
                        model.Latitude,
                        model.Longitude,
                        model.GeometriType
                    );

                    int kommuneId = await _lokasjonService.GetKommuneIdFromCoordinates(model.Latitude, model.Longitude);

                    _innmeldingService.AddInnmelding(
                        brukerId: brukerId.Value,
                        kommuneId: kommuneId,
                        lokasjonId: lokasjonId,
                        beskrivelse: beskrivelse
                    );

                    return RedirectToAction("Index", "MineInnmeldinger");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kunne ikke lagre innmeldingen");
                    return View("Index", model);
                }
            }
            return View("Index", model);
        }

        public IActionResult ViewInnmelding(int id)
        {
            var innmelding = _innmeldingService.GetInnmeldingById(id);
            if (innmelding == null)
            {
                return NotFound();
            }

            var lokasjon = _lokasjonService.GetLokasjonById(innmelding.LokasjonId);
            if (lokasjon == null)
            {
                return NotFound();
            }

            ViewBag.Innmelding = innmelding;
            return View(lokasjon);
        }
    }
} 