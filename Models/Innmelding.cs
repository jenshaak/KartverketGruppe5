using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class Innmelding
    {
        [Key]
        public int InnmeldingId { get; set; }

        [Required]
        public int BrukerId { get; set; }

        [Required]
        public int KommuneId { get; set; }

        [Required]
        public int LokasjonId { get; set; }

        public int? SaksbehandlerId { get; set; }

        [Required(ErrorMessage = "Beskrivelse er påkrevd")]
        [StringLength(1000, ErrorMessage = "Beskrivelse kan ikke være lengre enn 1000 tegn")]
        public string Beskrivelse { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Kommentar kan ikke være lengre enn 1000 tegn")]
        public string? Kommentar { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Ny";

        [StringLength(100)]
        public string? BildeSti { get; set; }

        public DateTime OpprettetDato { get; set; }
        public DateTime? OppdatertDato { get; set; }

        public virtual Bruker? Bruker { get; set; }
        public virtual Kommune? Kommune { get; set; }
        public virtual Lokasjon? Lokasjon { get; set; }
        public virtual Saksbehandler? Saksbehandler { get; set; }
    }
} 