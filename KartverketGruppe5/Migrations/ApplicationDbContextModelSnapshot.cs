﻿// <auto-generated />
using System;
using KartverketGruppe5.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("KartverketGruppe5.Models.Bruker", b =>
                {
                    b.Property<int>("BrukerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("BrukerId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("varchar(80)");

                    b.Property<string>("Etternavn")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Fornavn")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("OpprettetDato")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Passord")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("Slettet")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.HasKey("BrukerId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Bruker", (string)null);

                    b.HasData(
                        new
                        {
                            BrukerId = 1,
                            Email = "ole@gmail.com",
                            Etternavn = "Olsen",
                            Fornavn = "Ole",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(1160),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=",
                            Slettet = false
                        });
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Fylke", b =>
                {
                    b.Property<int>("FylkeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("FylkeId"));

                    b.Property<string>("FylkeNummer")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("varchar(2)");

                    b.Property<string>("Navn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("FylkeId");

                    b.HasIndex("FylkeNummer")
                        .IsUnique();

                    b.HasIndex("Navn")
                        .IsUnique();

                    b.ToTable("Fylke", (string)null);
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Innmelding", b =>
                {
                    b.Property<int>("InnmeldingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("InnmeldingId"));

                    b.Property<string>("Beskrivelse")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<string>("BildeSti")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("BrukerId")
                        .HasColumnType("int");

                    b.Property<string>("Kommentar")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<int>("KommuneId")
                        .HasColumnType("int");

                    b.Property<int>("LokasjonId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("OppdatertDato")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("OpprettetDato")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int?>("SaksbehandlerId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
                        .HasDefaultValue("Ny");

                    b.HasKey("InnmeldingId");

                    b.HasIndex("BrukerId");

                    b.HasIndex("KommuneId");

                    b.HasIndex("LokasjonId");

                    b.HasIndex("SaksbehandlerId");

                    b.ToTable("Innmelding", (string)null);
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Kommune", b =>
                {
                    b.Property<int>("KommuneId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("KommuneId"));

                    b.Property<int>("FylkeId")
                        .HasColumnType("int");

                    b.Property<string>("KommuneNummer")
                        .IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("varchar(4)");

                    b.Property<string>("Navn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("KommuneId");

                    b.HasIndex("FylkeId");

                    b.HasIndex("Navn")
                        .IsUnique();

                    b.ToTable("Kommune", (string)null);
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Lokasjon", b =>
                {
                    b.Property<int>("LokasjonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("LokasjonId"));

                    b.Property<string>("GeoJson")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GeometriType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<decimal>("Latitude")
                        .HasColumnType("DECIMAL(10,8)");

                    b.Property<decimal>("Longitude")
                        .HasColumnType("DECIMAL(11,8)");

                    b.HasKey("LokasjonId");

                    b.ToTable("Lokasjon", (string)null);
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Saksbehandler", b =>
                {
                    b.Property<int>("SaksbehandlerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("SaksbehandlerId"));

                    b.Property<bool>("Admin")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("varchar(80)");

                    b.Property<string>("Etternavn")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Fornavn")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("OpprettetDato")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Passord")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("SaksbehandlerId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Saksbehandler", (string)null);

                    b.HasData(
                        new
                        {
                            SaksbehandlerId = 1,
                            Admin = true,
                            Email = "rune@kartverket.no",
                            Etternavn = "Bengtson",
                            Fornavn = "Rune",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3750),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 2,
                            Admin = false,
                            Email = "lars@kartverket.no",
                            Etternavn = "Larsen",
                            Fornavn = "Lars",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3750),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 3,
                            Admin = true,
                            Email = "andreas@kartverket.no",
                            Etternavn = "Hansen",
                            Fornavn = "Andreas",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3750),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 4,
                            Admin = false,
                            Email = "arne@kartverket.no",
                            Etternavn = "Olsen",
                            Fornavn = "Arne",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3760),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 5,
                            Admin = false,
                            Email = "ronny@kartverket.no",
                            Etternavn = "Larsen",
                            Fornavn = "Ronny",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3760),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 6,
                            Admin = true,
                            Email = "knut@kartverket.no",
                            Etternavn = "Knudsen",
                            Fornavn = "Knut",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3760),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 7,
                            Admin = false,
                            Email = "ivar@kartverket.no",
                            Etternavn = "Imsdal",
                            Fornavn = "Ivar",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3760),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 8,
                            Admin = false,
                            Email = "ida@kartverket.no",
                            Etternavn = "Carlsen",
                            Fornavn = "Ida",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3760),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 9,
                            Admin = false,
                            Email = "caroline@kartverket.no",
                            Etternavn = "Ryerson",
                            Fornavn = "Caroline",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3770),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 10,
                            Admin = false,
                            Email = "jesper@kartverket.no",
                            Etternavn = "Kristiansen",
                            Fornavn = "Jesper",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3770),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 11,
                            Admin = false,
                            Email = "sandra@kartverket.no",
                            Etternavn = "Bakken",
                            Fornavn = "Sandra",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3770),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 12,
                            Admin = false,
                            Email = "alex@kartverket.no",
                            Etternavn = "Dale",
                            Fornavn = "Alex",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3770),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        },
                        new
                        {
                            SaksbehandlerId = 13,
                            Admin = false,
                            Email = "preben@kartverket.no",
                            Etternavn = "Jensen",
                            Fornavn = "Preben",
                            OpprettetDato = new DateTime(2024, 11, 25, 9, 12, 43, 734, DateTimeKind.Utc).AddTicks(3770),
                            Passord = "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM="
                        });
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Innmelding", b =>
                {
                    b.HasOne("KartverketGruppe5.Models.Bruker", "Bruker")
                        .WithMany()
                        .HasForeignKey("BrukerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("KartverketGruppe5.Models.Kommune", "Kommune")
                        .WithMany()
                        .HasForeignKey("KommuneId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("KartverketGruppe5.Models.Lokasjon", "Lokasjon")
                        .WithMany()
                        .HasForeignKey("LokasjonId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("KartverketGruppe5.Models.Saksbehandler", "Saksbehandler")
                        .WithMany()
                        .HasForeignKey("SaksbehandlerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Bruker");

                    b.Navigation("Kommune");

                    b.Navigation("Lokasjon");

                    b.Navigation("Saksbehandler");
                });

            modelBuilder.Entity("KartverketGruppe5.Models.Kommune", b =>
                {
                    b.HasOne("KartverketGruppe5.Models.Fylke", "Fylke")
                        .WithMany()
                        .HasForeignKey("FylkeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Fylke");
                });
#pragma warning restore 612, 618
        }
    }
}
