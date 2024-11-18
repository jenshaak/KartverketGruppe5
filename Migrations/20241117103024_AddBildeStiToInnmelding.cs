using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class AddBildeStiToInnmelding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BildeSti",
                table: "Innmelding",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Bruker",
                keyColumn: "BrukerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 17, 10, 30, 22, 821, DateTimeKind.Utc).AddTicks(5746));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 17, 10, 30, 22, 824, DateTimeKind.Utc).AddTicks(9634));

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 2,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 17, 10, 30, 22, 824, DateTimeKind.Utc).AddTicks(9641));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BildeSti",
                table: "Innmelding");

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
