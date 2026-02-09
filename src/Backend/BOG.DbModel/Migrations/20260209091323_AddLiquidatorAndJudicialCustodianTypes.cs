using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddLiquidatorAndJudicialCustodianTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RepresentativeTypes",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Liquidator", "مصفي" },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "JudicialCustodian", "حارس قضائي" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RepresentativeTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RepresentativeTypes",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
