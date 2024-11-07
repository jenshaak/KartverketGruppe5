using Microsoft.EntityFrameworkCore;
using KartverketGruppe5.Data;
using KartverketGruppe5.Models;

namespace KartverketGruppe5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<GeoChange> GeoChanges { get; set; }
        public DbSet<Bruker> Brukere { get; set; }
    }
}