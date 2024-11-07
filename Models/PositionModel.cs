namespace KartverketGruppe5.Models
{
    public class PositionModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Description { get; set; }
        public string? GeoJson { get; set; }
        public string GeometriType { get; set; } = "Point";
    }
}
