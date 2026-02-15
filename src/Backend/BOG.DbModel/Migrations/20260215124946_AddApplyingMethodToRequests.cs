using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddApplyingMethodToRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplyingMethodId",
                table: "CaseRegistrationRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplyingMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyingMethods", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ApplyingMethods",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Request submitted in person at the court", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Through Court", "من خلال المحكمة" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Request submitted through the online portal", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Through Portal", "من خلال البوابة" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegistrationRequests_ApplyingMethodId",
                table: "CaseRegistrationRequests",
                column: "ApplyingMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRegistrationRequests_ApplyingMethods_ApplyingMethodId",
                table: "CaseRegistrationRequests",
                column: "ApplyingMethodId",
                principalTable: "ApplyingMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRegistrationRequests_ApplyingMethods_ApplyingMethodId",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropTable(
                name: "ApplyingMethods");

            migrationBuilder.DropIndex(
                name: "IX_CaseRegistrationRequests_ApplyingMethodId",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "ApplyingMethodId",
                table: "CaseRegistrationRequests");
        }
    }
}
