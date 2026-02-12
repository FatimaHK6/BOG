using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseTypeId",
                table: "CaseRegistrationRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "CaseRequestWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseRegistrationRequestId = table.Column<int>(type: "int", nullable: false),
                    PreviousStatusId = table.Column<int>(type: "int", nullable: false),
                    NewStatusId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseRequestWorkflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseRequestWorkflows_CaseRegistrationRequests_CaseRegistrationRequestId",
                        column: x => x.CaseRegistrationRequestId,
                        principalTable: "CaseRegistrationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseTypes",
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
                    table.PrimaryKey("PK_CaseTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CaseTypes",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrative case type", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrative", "إداري" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Disciplinary case type", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Disciplinary", "تأديبي" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegistrationRequests_CaseTypeId",
                table: "CaseRegistrationRequests",
                column: "CaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRequestWorkflows_ActionDate",
                table: "CaseRequestWorkflows",
                column: "ActionDate",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRequestWorkflows_CaseRegistrationRequestId",
                table: "CaseRequestWorkflows",
                column: "CaseRegistrationRequestId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseRegistrationRequests_CaseTypes_CaseTypeId",
                table: "CaseRegistrationRequests",
                column: "CaseTypeId",
                principalTable: "CaseTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseRegistrationRequests_CaseTypes_CaseTypeId",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropTable(
                name: "CaseRequestWorkflows");

            migrationBuilder.DropTable(
                name: "CaseTypes");

            migrationBuilder.DropIndex(
                name: "IX_CaseRegistrationRequests_CaseTypeId",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "CaseTypeId",
                table: "CaseRegistrationRequests");
        }
    }
}
