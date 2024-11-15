using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class AddRune : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                columns: new[] { "Email", "Etternavn", "Fornavn", "OpprettetDato" },
                values: new object[] { "rune@kartverket.no", "Bengtson", "Rune", new DateTime(2024, 11, 15, 13, 18, 7, 378, DateTimeKind.Utc).AddTicks(5299) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                columns: new[] { "Email", "Etternavn", "Fornavn", "OpprettetDato" },
                values: new object[] { "admin@kartverket.no", "Bruker", "Admin", new DateTime(2024, 11, 15, 13, 16, 10, 970, DateTimeKind.Utc).AddTicks(4858) });
        }
    }
}
