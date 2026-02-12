using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedDate", "Email", "FirstName", "IsActive", "LastName", "ModifiedDate", "PhoneNumber" },
                values: new object[] { 1, new DateTime(2026, 2, 8, 18, 57, 3, 649, DateTimeKind.Utc).AddTicks(6918), "system@bog.sa", "System", true, "User", new DateTime(2026, 2, 8, 18, 57, 3, 649, DateTimeKind.Utc).AddTicks(6918), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
