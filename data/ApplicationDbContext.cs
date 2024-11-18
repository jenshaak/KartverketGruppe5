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
        public DbSet<Bruker> Brukere { get; set; }
        public DbSet<Fylke> Fylker { get; set; }
        public DbSet<Kommune> Kommuner { get; set; }
        public DbSet<Lokasjon> Lokasjoner { get; set; }
        public DbSet<Innmelding> Innmeldinger { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bruker>(entity =>
            {
                entity.ToTable("Bruker");
                entity.HasKey(e => e.BrukerId);
                entity.Property(e => e.Fornavn).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Etternavn).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Passord).IsRequired().HasMaxLength(256);
                entity.Property(e => e.OpprettetDato)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Email).IsUnique();
                
                entity.HasData(new Bruker
                {
                    BrukerId = 1,
                    Fornavn = "Ole",
                    Etternavn = "Olsen",
                    Email = "ole@gmail.com",
                    Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                    OpprettetDato = DateTime.UtcNow
                });
            });

            modelBuilder.Entity<Fylke>(entity =>
            {
                entity.ToTable("Fylke");
                entity.HasKey(e => e.FylkeId);
                entity.Property(e => e.Navn).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FylkeNummer).IsRequired().HasMaxLength(2);
                entity.HasIndex(e => e.Navn).IsUnique();
                entity.HasIndex(e => e.FylkeNummer).IsUnique();
            });

            modelBuilder.Entity<Kommune>(entity =>
            {
                entity.ToTable("Kommune");
                entity.HasKey(e => e.KommuneId);
                entity.Property(e => e.Navn).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Navn).IsUnique();
                entity.HasOne<Fylke>()
                    .WithMany()
                    .HasForeignKey(e => e.FylkeId)
                    .IsRequired();
            });

            modelBuilder.Entity<Lokasjon>(entity =>
            {
                entity.ToTable("Lokasjon");
                entity.HasKey(e => e.LokasjonId);
                entity.Property(e => e.GeoJson).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Latitude).HasColumnType("DECIMAL(10,8)").IsRequired();
                entity.Property(e => e.Longitude).HasColumnType("DECIMAL(11,8)").IsRequired();
                entity.Property(e => e.GeometriType).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Saksbehandler>(entity =>
            {
                entity.ToTable("Saksbehandler");
                entity.HasKey(e => e.SaksbehandlerId);
                entity.Property(e => e.Fornavn).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Etternavn).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Passord).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Admin).HasDefaultValue(false);
                entity.Property(e => e.OpprettetDato)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasData(
                    new Saksbehandler
                    {
                        SaksbehandlerId = 1,
                        Fornavn = "Rune",
                        Etternavn = "Bengtson",
                        Email = "rune@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = true,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 2,
                        Fornavn = "Lars",
                        Etternavn = "Larsen",
                        Email = "lars@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    }
                );
            });

            modelBuilder.Entity<Innmelding>(entity =>
            {
                entity.ToTable("Innmelding");
                entity.HasKey(e => e.InnmeldingId);
                entity.Property(e => e.Beskrivelse).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Kommentar).HasColumnType("TEXT");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20)
                    .HasDefaultValue("Ny");
                entity.Property(e => e.OpprettetDato)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.OppdatertDato)
                    .ValueGeneratedOnUpdate();
                entity.Property(e => e.BildeSti)
                    .HasMaxLength(100);

                // Relasjoner
                entity.HasOne(e => e.Bruker)
                    .WithMany()
                    .HasForeignKey(e => e.BrukerId)
                    .IsRequired();

                entity.HasOne(e => e.Kommune)
                    .WithMany()
                    .HasForeignKey(e => e.KommuneId)
                    .IsRequired();

                entity.HasOne(e => e.Lokasjon)
                    .WithMany()
                    .HasForeignKey(e => e.LokasjonId)
                    .IsRequired();

                entity.HasOne(e => e.Saksbehandler)
                    .WithMany()
                    .HasForeignKey(e => e.SaksbehandlerId);
            });
        }
    }
}