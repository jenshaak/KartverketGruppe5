namespace KartverketGruppe5.Models
{
    public class InnmeldingModel
    {
        public int InnmeldingId { get; set; }
        public int BrukerId { get; set; }
        public int KommuneId { get; set; }
        public int LokasjonId { get; set; }
        public string Beskrivelse { get; set; }
        public string Status { get; set; }
        public DateTime OpprettetDato { get; set; }
        public DateTime? OppdatertDato { get; set; }
        public string KommuneNavn { get; set; }
        public string? FylkeNavn { get; set; }
        public string StatusClass { get; set; } = "bg-gray-100 text-gray-800";
        public int? SaksbehandlerId { get; set; }
        public string? SaksbehandlerNavn { get; set; }
        public IFormFile? Bilde { get; set; }
        public string? BildeSti { get; set; }
    }
} 