using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class Saksbehandler
    {
        [Key]
        public int SaksbehandlerId { get; set; }
        
        [Required(ErrorMessage = "Fornavn er påkrevd")]
        [StringLength(50, ErrorMessage = "Fornavn kan ikke være lengre enn 50 tegn")]
        public string Fornavn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Etternavn er påkrevd")]
        [StringLength(50, ErrorMessage = "Etternavn kan ikke være lengre enn 50 tegn")]
        public string Etternavn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-post er påkrevd")]
        [StringLength(80, ErrorMessage = "E-post kan ikke være lengre enn 80 tegn")]
        [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Passord er påkrevd")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Passord må være mellom 8 og 256 tegn")]
        public string Passord { get; set; } = string.Empty;
        
        public bool Admin { get; set; } = false;
        
        public DateTime OpprettetDato { get; set; }
    }
} 