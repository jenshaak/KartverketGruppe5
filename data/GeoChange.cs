namespace KartverketGruppe5.Data
{
    public class GeoChange
    {
        // Use int for auto-incremented primary key
        public int Id { get; set; }  
        public string? GeoJson { get; set; }
        public string? Description { get; set; }
    }
}