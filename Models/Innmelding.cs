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

        // Navigasjonsproperties fra JOIN
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? GeoJson { get; set; }
    }
} 