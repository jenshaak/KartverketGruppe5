using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class AddRootAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Saksbehandler",
                columns: new[] { "SaksbehandlerId", "Admin", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 1, true, "admin@kartverket.no", "Bruker", "Admin", new DateTime(2024, 11, 15, 13, 16, 10, 970, DateTimeKind.Utc).AddTicks(4858), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1);
        }
    }
}
