using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Bruker",
                keyColumn: "BrukerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 15, 31, 16, 165, DateTimeKind.Utc).AddTicks(2783));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 15, 31, 16, 166, DateTimeKind.Utc).AddTicks(7919));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 2,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 15, 31, 16, 166, DateTimeKind.Utc).AddTicks(7922));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Bruker",
                keyColumn: "BrukerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 13, 46, 3, 504, DateTimeKind.Utc).AddTicks(653));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 13, 46, 3, 505, DateTimeKind.Utc).AddTicks(7429));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 2,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 13, 46, 3, 505, DateTimeKind.Utc).AddTicks(7433));
        }
    }
}
