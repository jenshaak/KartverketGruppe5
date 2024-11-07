namespace KartverketGruppe5.Models
{
    public class InnmeldingModel
    {
        public int InnmeldingId { get; set; }
        public string KommuneNavn { get; set; } = string.Empty;
        public string Beskrivelse { get; set; } = string.Empty;
        public string Status { get; set; } = "Ny";
        public DateTime OpprettetDato { get; set; }
        public string? Kommentar { get; set; }
        
        public string StatusClass => Status switch
        {
            "Godkjent" => "bg-green-200 text-green-800",
            "Avvist" => "bg-red-200 text-red-800",
            "Under Behandling" => "bg-yellow-200 text-yellow-800",
            _ => "bg-gray-200 text-gray-800"
        };
    }
} 