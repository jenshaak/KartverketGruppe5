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
        public DbSet<Saksbehandler> Saksbehandlere { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Saksbehandler>(entity =>
            {
                entity.ToTable("Saksbehandler");
                entity.HasKey(e => e.SaksbehandlerId);
                entity.Property(e => e.Fornavn).IsRequired();
                entity.Property(e => e.Etternavn).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Passord).IsRequired();
            });
        }
    }
}