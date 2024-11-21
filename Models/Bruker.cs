using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class Bruker
    {
        public int BrukerId { get; set; }
        
        [Required(ErrorMessage = "Fornavn er påkrevd")]
        [StringLength(50, ErrorMessage = "Fornavn kan ikke være lengre enn 50 tegn")]
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ\s-]*$", ErrorMessage = "Fornavn kan bare inneholde bokstaver, mellomrom og bindestrek")]
        public string Fornavn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Etternavn er påkrevd")]
        [StringLength(50, ErrorMessage = "Etternavn kan ikke være lengre enn 50 tegn")]
        [RegularExpression(@"^[a-zA-ZæøåÆØÅ\s-]*$", ErrorMessage = "Etternavn kan bare inneholde bokstaver, mellomrom og bindestrek")]
        public string Etternavn { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-post er påkrevd")]
        [StringLength(80, ErrorMessage = "E-post kan ikke være lengre enn 80 tegn")]
        [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Passord er påkrevd")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Passord må være mellom 8 og 256 tegn")]
        public string Passord { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Rolle { get; set; } = "Bruker";  // Default verdi som matcher databasen
        
        public DateTime OpprettetDato { get; set; }

        // Navigasjonsegenskap for innmeldinger
        public virtual ICollection<Innmelding>? Innmeldinger { get; set; }
    }
} 