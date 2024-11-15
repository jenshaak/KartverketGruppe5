using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KartverketGruppe5.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Bruker",
                columns: new[] { "BrukerId", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 1, "ole@gmail.com", "Olsen", "Ole", new DateTime(2024, 11, 15, 13, 46, 3, 504, DateTimeKind.Utc).AddTicks(653), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 13, 46, 3, 505, DateTimeKind.Utc).AddTicks(7429));

            migrationBuilder.InsertData(
                table: "Saksbehandler",
                columns: new[] { "SaksbehandlerId", "Email", "Etternavn", "Fornavn", "OpprettetDato", "Passord" },
                values: new object[] { 2, "lars@kartverket.no", "Larsen", "Lars", new DateTime(2024, 11, 15, 13, 46, 3, 505, DateTimeKind.Utc).AddTicks(7433), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Bruker",
                keyColumn: "BrukerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Saksbehandler",
                keyColumn: "SaksbehandlerId",
                keyValue: 1,
                column: "OpprettetDato",
                value: new DateTime(2024, 11, 15, 13, 18, 7, 378, DateTimeKind.Utc).AddTicks(5299));
        }
    }
}
