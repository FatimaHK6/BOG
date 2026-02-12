using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelatedCaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "RelatedCases");

            migrationBuilder.AlterColumn<int>(
                name: "CaseNumber",
                table: "RelatedCases",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "CaseYear",
                table: "RelatedCases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourtId",
                table: "RelatedCases",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NotificationMethodId",
                table: "AdditionalInfoManagementDecisions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IssuingAuthorityId",
                table: "AdditionalInfoManagementDecisions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_RelatedCases_CourtId",
                table: "RelatedCases",
                column: "CourtId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelatedCases_Courts_CourtId",
                table: "RelatedCases",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelatedCases_Courts_CourtId",
                table: "RelatedCases");

            migrationBuilder.DropIndex(
                name: "IX_RelatedCases_CourtId",
                table: "RelatedCases");

            migrationBuilder.DropColumn(
                name: "CaseYear",
                table: "RelatedCases");

            migrationBuilder.DropColumn(
                name: "CourtId",
                table: "RelatedCases");

            migrationBuilder.AlterColumn<string>(
                name: "CaseNumber",
                table: "RelatedCases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "RelatedCases",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NotificationMethodId",
                table: "AdditionalInfoManagementDecisions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IssuingAuthorityId",
                table: "AdditionalInfoManagementDecisions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
