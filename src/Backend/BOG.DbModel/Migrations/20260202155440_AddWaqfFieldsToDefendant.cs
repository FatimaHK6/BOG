using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddWaqfFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CourtDeedDate",
                table: "Defendants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourtDeedNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeedSource",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfAgencyName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfBuildingNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaqfCityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfDistrict",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfPostalCode",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaqfRegionId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfStreet",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaqfSupervisoryTypeId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfUnitNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_WaqfCityId",
                table: "Defendants",
                column: "WaqfCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_WaqfRegionId",
                table: "Defendants",
                column: "WaqfRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants",
                column: "WaqfCityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants",
                column: "WaqfRegionId",
                principalTable: "Regions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_WaqfCityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_WaqfRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "CourtDeedDate",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "CourtDeedNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "DeedSource",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfAgencyName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfBuildingNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfCityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfDistrict",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfPostalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfStreet",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfSupervisoryTypeId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WaqfUnitNumber",
                table: "Defendants");
        }
    }
}
