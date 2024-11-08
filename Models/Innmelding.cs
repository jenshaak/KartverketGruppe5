namespace KartverketGruppe5.Models
{
    public class Innmelding
    {
        public int InnmeldingId { get; set; }
        public int BrukerId { get; set; }
        public int KommuneId { get; set; }
        public int LokasjonId { get; set; }
        public string Beskrivelse { get; set; } = string.Empty;
        public string? Kommentar { get; set; }
        public string Status { get; set; } = "Ny";
        public DateTime OpprettetDato { get; set; }
        public DateTime? OppdatertDato { get; set; }

        // Navigasjonsegenskaper hvis du bruker Entity Framework
        // public virtual Bruker? Bruker { get; set; }
        // public virtual Kommune? Kommune { get; set; }
        // public virtual Lokasjon? Lokasjon { get; set; }
    }
} 