public class LokasjonModel
{
    public int LokasjonId { get; set; }
    public string? GeoJson { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string GeometriType { get; set; } = "Point";
} 