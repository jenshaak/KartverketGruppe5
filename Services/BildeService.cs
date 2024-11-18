using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace KartverketGruppe5.Services
{
    public class BildeService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BildeService> _logger;

        public BildeService(IWebHostEnvironment webHostEnvironment, ILogger<BildeService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string?> LagreBilde(IFormFile bilde, int innmeldingId)
        {
            if (bilde == null || bilde.Length == 0)
                return null;

            // Lager mappe for innmeldingsbilder hvis den ikke eksisterer
            var bildemappe = Path.Combine(_webHostEnvironment.WebRootPath, "innmeldingsbilder");
            if (!Directory.Exists(bildemappe))
                Directory.CreateDirectory(bildemappe);

            // Generer unikt filnavn
            var filnavn = $"{innmeldingId}_{DateTime.Now.Ticks}{Path.GetExtension(bilde.FileName)}";
            var filsti = Path.Combine(bildemappe, filnavn);

            try
            {
                using (var stream = new FileStream(filsti, FileMode.Create))
                {
                    await bilde.CopyToAsync(stream);
                }

                // Returner sti for lagring i database
                return Path.Combine("innmeldingsbilder", filnavn);
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Feil ved lagring av bilde for innmelding {InnmeldingId}", innmeldingId);
            return null;
            }
        }
    }
} 