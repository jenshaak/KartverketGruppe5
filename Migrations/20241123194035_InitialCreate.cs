using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bruker",
                columns: table => new
                {
                    BrukerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fornavn = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Etternavn = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Passord = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpprettetDato = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bruker", x => x.BrukerId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fylke",
                columns: table => new
                {
                    FylkeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Navn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FylkeNummer = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fylke", x => x.FylkeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lokasjon",
                columns: table => new
                {
                    LokasjonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GeoJson = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<decimal>(type: "DECIMAL(10,8)", nullable: false),
                    Longitude = table.Column<decimal>(type: "DECIMAL(11,8)", nullable: false),
                    GeometriType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokasjon", x => x.LokasjonId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Saksbehandler",
                columns: table => new
                {
                    SaksbehandlerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fornavn = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Etternavn = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Passord = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Admin = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    OpprettetDato = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saksbehandler", x => x.SaksbehandlerId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Kommune",
                columns: table => new
                {
                    KommuneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FylkeId = table.Column<int>(type: "int", nullable: false),
                    Navn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KommuneNummer = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kommune", x => x.KommuneId);
                    table.ForeignKey(
                        name: "FK_Kommune_Fylke_FylkeId",
                        column: x => x.FylkeId,
                        principalTable: "Fylke",
                        principalColumn: "FylkeId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Innmelding",
                columns: table => new
                {
                    InnmeldingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BrukerId = table.Column<int>(type: "int", nullable: false),
                    KommuneId = table.Column<int>(type: "int", nullable: false),
                    LokasjonId = table.Column<int>(type: "int", nullable: false),
                    SaksbehandlerId = table.Column<int>(type: "int", nullable: true),
                    Beskrivelse = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Kommentar = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Ny")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BildeSti = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpprettetDato = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OppdatertDato = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innmelding", x => x.InnmeldingId);
                    table.ForeignKey(
                        name: "FK_Innmelding_Bruker_BrukerId",
                        column: x => x.BrukerId,
                        principalTable: "Bruker",
                        principalColumn: "BrukerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innmelding_Kommune_KommuneId",
                        column: x => x.KommuneId,
                        principalTable: "Kommune",
                        principalColumn: "KommuneId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innmelding_Lokasjon_LokasjonId",
                        column: x => x.LokasjonId,
                        principalTable: "Lokasjon",
                        principalColumn: "LokasjonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innmelding_Saksbehandler_SaksbehandlerId",
                        column: x => x.SaksbehandlerId,
                        principalTable: "Saksbehandler",
                        principalColumn: "SaksbehandlerId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Bruker",
                columns: new[] { "BrukerId", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 1, "ole@gmail.com", "Olsen", "Ole", new DateTime(2024, 11, 23, 19, 40, 35, 539, DateTimeKind.Utc).AddTicks(8830), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });

            migrationBuilder.InsertData(
                table: "Saksbehandler",
                columns: new[] { "SaksbehandlerId", "Admin", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 1, true, "rune@kartverket.no", "Bengtson", "Rune", new DateTime(2024, 11, 23, 19, 40, 35, 540, DateTimeKind.Utc).AddTicks(1160), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });

            migrationBuilder.InsertData(
                table: "Saksbehandler",
                columns: new[] { "SaksbehandlerId", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 2, "lars@kartverket.no", "Larsen", "Lars", new DateTime(2024, 11, 23, 19, 40, 35, 540, DateTimeKind.Utc).AddTicks(1160), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });

            migrationBuilder.CreateIndex(
                name: "IX_Bruker_Email",
                table: "Bruker",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fylke_FylkeNummer",
                table: "Fylke",
                column: "FylkeNummer",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fylke_Navn",
                table: "Fylke",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Innmelding_BrukerId",
                table: "Innmelding",
                column: "BrukerId");

            migrationBuilder.CreateIndex(
                name: "IX_Innmelding_KommuneId",
                table: "Innmelding",
                column: "KommuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Innmelding_LokasjonId",
                table: "Innmelding",
                column: "LokasjonId");

            migrationBuilder.CreateIndex(
                name: "IX_Innmelding_SaksbehandlerId",
                table: "Innmelding",
                column: "SaksbehandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_Kommune_FylkeId",
                table: "Kommune",
                column: "FylkeId");

            migrationBuilder.CreateIndex(
                name: "IX_Kommune_Navn",
                table: "Kommune",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Saksbehandler_Email",
                table: "Saksbehandler",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Innmelding");

            migrationBuilder.DropTable(
                name: "Bruker");

            migrationBuilder.DropTable(
                name: "Kommune");

            migrationBuilder.DropTable(
                name: "Lokasjon");

            migrationBuilder.DropTable(
                name: "Saksbehandler");

            migrationBuilder.DropTable(
                name: "Fylke");
        }
    }
}
