namespace KartverketGruppe5.Models.RequestModels
{
    public class InnmeldingUpdateModel
    {
        public int InnmeldingId { get; set; }
        public string? Status { get; set; }
        public int? SaksbehandlerId { get; set; }
        public string? Beskrivelse { get; set; }
        public string? Kommentar { get; set; }
        public string? BildeSti { get; set; }
        public DateTime? OppdatertDato { get; set; }
    }
} 