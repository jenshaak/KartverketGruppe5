namespace KartverketGruppe5.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Fylke
    {
        public int FylkeId { get; set; }

        [Required(ErrorMessage = "Fylkesnavn er påkrevd")]
        [StringLength(100, ErrorMessage = "Fylkesnavn kan ikke være lengre enn 100 tegn")]
        public string Navn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fylkesnummer er påkrevd")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Fylkesnummer må være 2 tegn")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Fylkesnummer må være 2 siffer")]
        public string FylkeNummer { get; set; } = string.Empty;
    }
} 