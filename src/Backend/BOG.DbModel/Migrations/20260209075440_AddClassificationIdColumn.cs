using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddClassificationIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ClassificationId column (non-nullable with default for existing data)
            migrationBuilder.AddColumn<int>(
                name: "ClassificationId",
                table: "RequestClassifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Make ClassificationText nullable (it's deprecated)
            migrationBuilder.AlterColumn<string>(
                name: "ClassificationText",
                table: "RequestClassifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            // Drop existing index on CaseRegistrationRequestId
            migrationBuilder.DropIndex(
                name: "IX_RequestClassifications_CaseRegistrationRequestId",
                table: "RequestClassifications");

            // Create index on ClassificationId
            migrationBuilder.CreateIndex(
                name: "IX_RequestClassifications_ClassificationId",
                table: "RequestClassifications",
                column: "ClassificationId");

            // Create unique index on (CaseRegistrationRequestId, ClassificationId) for non-deleted rows
            migrationBuilder.CreateIndex(
                name: "IX_RequestClassifications_CaseRegistrationRequestId_ClassificationId",
                table: "RequestClassifications",
                columns: new[] { "CaseRegistrationRequestId", "ClassificationId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            // Add foreign key to Classifications
            migrationBuilder.AddForeignKey(
                name: "FK_RequestClassifications_Classifications_ClassificationId",
                table: "RequestClassifications",
                column: "ClassificationId",
                principalTable: "Classifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_RequestClassifications_Classifications_ClassificationId",
                table: "RequestClassifications");

            // Drop indices
            migrationBuilder.DropIndex(
                name: "IX_RequestClassifications_ClassificationId",
                table: "RequestClassifications");

            migrationBuilder.DropIndex(
                name: "IX_RequestClassifications_CaseRegistrationRequestId_ClassificationId",
                table: "RequestClassifications");

            // Drop ClassificationId column
            migrationBuilder.DropColumn(
                name: "ClassificationId",
                table: "RequestClassifications");

            // Restore ClassificationText as not nullable
            migrationBuilder.AlterColumn<string>(
                name: "ClassificationText",
                table: "RequestClassifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            // Recreate original index
            migrationBuilder.CreateIndex(
                name: "IX_RequestClassifications_CaseRegistrationRequestId",
                table: "RequestClassifications",
                column: "CaseRegistrationRequestId");
        }
    }
}
