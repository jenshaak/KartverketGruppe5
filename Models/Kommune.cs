namespace KartverketGruppe5.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Kommune
    {
        public int KommuneId { get; set; }

        [Required(ErrorMessage = "FylkeId er påkrevd")]
        public int FylkeId { get; set; }

        [Required(ErrorMessage = "Kommunenavn er påkrevd")]
        [StringLength(100, ErrorMessage = "Kommunenavn kan ikke være lengre enn 100 tegn")]
        public string Navn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kommunenummer er påkrevd")]
        [StringLength(4, ErrorMessage = "Kommunenummer kan ikke være lengre enn 4 tegn")]
        public string KommuneNummer { get; set; } = string.Empty;

        public virtual Fylke? Fylke { get; set; }
    }
} 