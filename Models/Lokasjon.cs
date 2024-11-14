namespace KartverketGruppe5.Models
{
    public class Lokasjon
    {
        public int LokasjonId { get; set; }
        public string GeoJson { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? GeometriType { get; set; } = string.Empty;
    }
} 