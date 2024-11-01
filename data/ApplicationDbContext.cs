using Microsoft.EntityFrameworkCore;
using KartverketGruppe5.Data;

namespace KartverketGruppe5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<GeoChange> GeoChanges { get; set; }
    }
}