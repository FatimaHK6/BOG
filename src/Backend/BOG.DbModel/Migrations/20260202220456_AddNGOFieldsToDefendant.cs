using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddNGOFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseDate",
                table: "Defendants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Defendants",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LicenseSourceId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOAdditionalCode",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOBuildingNumber",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NGOCityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGODistrict",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOName",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOPostalCode",
                table: "Defendants",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NGORegionId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOStreet",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOUnitNumber",
                table: "Defendants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_LicenseSourceId",
                table: "Defendants",
                column: "LicenseSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_NGOCityId",
                table: "Defendants",
                column: "NGOCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_NGORegionId",
                table: "Defendants",
                column: "NGORegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_NGOCityId",
                table: "Defendants",
                column: "NGOCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_LicenseSources_LicenseSourceId",
                table: "Defendants",
                column: "LicenseSourceId",
                principalTable: "LicenseSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_NGORegionId",
                table: "Defendants",
                column: "NGORegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_NGOCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_LicenseSources_LicenseSourceId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_NGORegionId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_LicenseSourceId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_NGOCityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_NGORegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "LicenseDate",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "LicenseSourceId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOAdditionalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOBuildingNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOCityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGODistrict",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOPostalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGORegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOStreet",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NGOUnitNumber",
                table: "Defendants");
        }
    }
}
