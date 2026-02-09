using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefendantTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "RegisteredCompany", "شركة مسجلة في المملكة" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "UnregisteredCompany", "شركة غير مسجلة في المملكة" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "BusinessOwner", "صاحب مؤسسة" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "NGO", "جمعية/مؤسسة أهلية" });

            migrationBuilder.InsertData(
                table: "DefendantTypes",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[] { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Waqf", "وقف" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Company", "شركة" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Society", "جمعية" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Waqf", "وقف" });

            migrationBuilder.UpdateData(
                table: "DefendantTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Unknown", "مجهول" });
        }
    }
}
