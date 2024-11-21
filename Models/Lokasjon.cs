namespace KartverketGruppe5.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Lokasjon
    {
        public int LokasjonId { get; set; }

        [Required(ErrorMessage = "GeoJSON er påkrevd")]
        public string GeoJson { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude må være mellom -90 og 90")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude må være mellom -180 og 180")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "GeometriType er påkrevd")]
        [StringLength(20)]
        public string GeometriType { get; set; } = string.Empty;
    }
} 