using System.ComponentModel.DataAnnotations;

namespace KartverketGruppe5.Models
{
    public class Innmelding
    {
        [Key]
        public int InnmeldingId { get; set; }
        public int BrukerId { get; set; }
        public int KommuneId { get; set; }
        public int LokasjonId { get; set; }
        public int? SaksbehandlerId { get; set; }
        [Required]
        public string Beskrivelse { get; set; } = string.Empty;
        public string? Kommentar { get; set; }
        public string Status { get; set; } = "Ny";
        public DateTime OpprettetDato { get; set; }
        public DateTime? OppdatertDato { get; set; }
        public string? BildeSti { get; set; }

        public virtual Bruker? Bruker { get; set; }
        public virtual Kommune? Kommune { get; set; }
        public virtual Lokasjon? Lokasjon { get; set; }
        public virtual Saksbehandler? Saksbehandler { get; set; }
    }
} 