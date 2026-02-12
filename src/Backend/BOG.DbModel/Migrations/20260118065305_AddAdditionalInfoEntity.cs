using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalInfoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdditionalInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseRegistrationRequestId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RelatedCaseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LegalBasis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExternalReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PriorityLevel = table.Column<int>(type: "int", nullable: true, defaultValue: 2),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfos_CaseRegistrationRequests_CaseRegistrationRequestId",
                        column: x => x.CaseRegistrationRequestId,
                        principalTable: "CaseRegistrationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfos_CaseRegistrationRequestId",
                table: "AdditionalInfos",
                column: "CaseRegistrationRequestId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalInfos");
        }
    }
}
