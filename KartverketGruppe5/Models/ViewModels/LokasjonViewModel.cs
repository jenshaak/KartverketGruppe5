using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models.ViewModels
{
    /// <summary>
    /// ViewModel for lokasjoner
    /// </summary>
    public class LokasjonViewModel
    {
        public int LokasjonId { get; set; }
        public string? GeoJson { get; set; }

        [Required(ErrorMessage = "Latitude er påkrevd")]
        [Range(-90, 90, ErrorMessage = "Latitude må være mellom -90 og 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude er påkrevd")]
        [Range(-180, 180, ErrorMessage = "Longitude må være mellom -180 og 180")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Geometri type er påkrevd")]
        public string GeometriType { get; set; } = "Point";

        [Required(ErrorMessage = "Beskrivelse er påkrevd")]
        [StringLength(1000, ErrorMessage = "Beskrivelse kan ikke være lengre enn 1000 tegn")]
        public string Beskrivelse { get; set; } = string.Empty;
    }
} 