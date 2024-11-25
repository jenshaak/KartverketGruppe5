using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models.RequestModels
{
    /// <summary>
    /// Request-modell for oppdatering av innmeldinger
    /// </summary>
    public class InnmeldingUpdateModel
    {
        public int InnmeldingId { get; set; }
        public string? Status { get; set; }
        public int? SaksbehandlerId { get; set; }

        [StringLength(1000, ErrorMessage = "Beskrivelse kan ikke være lengre enn 1000 tegn")]
        public string? Beskrivelse { get; set; }

        [StringLength(1000, ErrorMessage = "Kommentar kan ikke være lengre enn 1000 tegn")]
        public string? Kommentar { get; set; }
        public string? BildeSti { get; set; }
        public DateTime? OppdatertDato { get; set; }
    }
} 