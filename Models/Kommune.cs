namespace KartverketGruppe5.Models
{
    public class Kommune
    {
        public int KommuneId { get; set; }
        public int FylkeId { get; set; }
        public string Navn { get; set; } = string.Empty;
        public string KommuneNummer { get; set; } = string.Empty;
    }
} 