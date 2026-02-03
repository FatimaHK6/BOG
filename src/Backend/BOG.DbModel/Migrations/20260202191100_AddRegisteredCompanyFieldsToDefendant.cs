using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddRegisteredCompanyFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants");

            migrationBuilder.AlterColumn<string>(
                name: "WaqfUnitNumber",
                table: "Defendants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfStreet",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfPostalCode",
                table: "Defendants",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfName",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfDistrict",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfBuildingNumber",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAgencyName",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAddressDescription",
                table: "Defendants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAdditionalCode",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Defendants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeedSource",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourtDeedNumber",
                table: "Defendants",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyAdditionalCode",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyBuildingNumber",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegCompanyCityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyDistrict",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyPostalCode",
                table: "Defendants",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegCompanyRegionId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyStreet",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegCompanyUnitNumber",
                table: "Defendants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationEndDate",
                table: "Defendants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationStartDate",
                table: "Defendants",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_RegCompanyCityId",
                table: "Defendants",
                column: "RegCompanyCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_RegCompanyRegionId",
                table: "Defendants",
                column: "RegCompanyRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_RegCompanyCityId",
                table: "Defendants",
                column: "RegCompanyCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants",
                column: "WaqfCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_RegCompanyRegionId",
                table: "Defendants",
                column: "RegCompanyRegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants",
                column: "WaqfRegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_RegCompanyCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_RegCompanyRegionId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_RegCompanyCityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_RegCompanyRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyAdditionalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyBuildingNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyCityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyDistrict",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyPostalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyStreet",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegCompanyUnitNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegistrationEndDate",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "RegistrationStartDate",
                table: "Defendants");

            migrationBuilder.AlterColumn<string>(
                name: "WaqfUnitNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfStreet",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfPostalCode",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfDistrict",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfBuildingNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAgencyName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAddressDescription",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WaqfAdditionalCode",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeedSource",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourtDeedNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_WaqfCityId",
                table: "Defendants",
                column: "WaqfCityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_WaqfRegionId",
                table: "Defendants",
                column: "WaqfRegionId",
                principalTable: "Regions",
                principalColumn: "Id");
        }
    }
}
