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

        public required DbSet<Saksbehandler> Saksbehandlere { get; set; }
        public required DbSet<Bruker> Brukere { get; set; }
        public required DbSet<Fylke> Fylker { get; set; }
        public required DbSet<Kommune> Kommuner { get; set; }
        public required DbSet<Lokasjon> Lokasjoner { get; set; }
        public required DbSet<Innmelding> Innmeldinger { get; set; }

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
                entity.Property(e => e.Slettet).HasDefaultValue(false);
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
                entity.HasOne(e => e.Fylke)
                    .WithMany()
                    .HasForeignKey(e => e.FylkeId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
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
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 3,
                        Fornavn = "Andreas",
                        Etternavn = "Hansen",
                        Email = "andreas@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = true,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 4,
                        Fornavn = "Arne",
                        Etternavn = "Olsen",
                        Email = "arne@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 5,
                        Fornavn = "Ronny",
                        Etternavn = "Larsen",
                        Email = "ronny@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 6,
                        Fornavn = "Knut",
                        Etternavn = "Knudsen",
                        Email = "knut@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = true,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 7,
                        Fornavn = "Ivar",
                        Etternavn = "Imsdal",
                        Email = "ivar@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 8,
                        Fornavn = "Ida",
                        Etternavn = "Carlsen",
                        Email = "ida@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 9,
                        Fornavn = "Caroline",
                        Etternavn = "Ryerson",
                        Email = "caroline@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 10,
                        Fornavn = "Jesper",
                        Etternavn = "Kristiansen",
                        Email = "jesper@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 11,
                        Fornavn = "Sandra",
                        Etternavn = "Bakken",
                        Email = "sandra@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 12,
                        Fornavn = "Alex",
                        Etternavn = "Dale",
                        Email = "alex@kartverket.no",
                        Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                        Admin = false,
                        OpprettetDato = DateTime.UtcNow
                    },
                    new Saksbehandler
                    {
                        SaksbehandlerId = 13,
                        Fornavn = "Preben",
                        Etternavn = "Jensen",
                        Email = "preben@kartverket.no",
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

                // Enkel one-way relasjon
                entity.HasOne(e => e.Bruker)
                    .WithMany()
                    .HasForeignKey(e => e.BrukerId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Kommune)
                    .WithMany()
                    .HasForeignKey(e => e.KommuneId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Lokasjon)
                    .WithMany()
                    .HasForeignKey(e => e.LokasjonId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Saksbehandler)
                    .WithMany()
                    .HasForeignKey(e => e.SaksbehandlerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}