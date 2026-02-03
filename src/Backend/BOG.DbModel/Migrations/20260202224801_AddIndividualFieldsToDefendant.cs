using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddIndividualFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Defendants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Defendants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentStatusId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyName",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrandfatherName",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndAdditionalCode",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndBuildingNumber",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndCityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndDistrict",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndPostalCode",
                table: "Defendants",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndRegionId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndStreet",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndUnitNumber",
                table: "Defendants",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Defendants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Defendants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TribeName",
                table: "Defendants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Nationalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_IndCityId",
                table: "Defendants",
                column: "IndCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_IndRegionId",
                table: "Defendants",
                column: "IndRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_NationalityId",
                table: "Defendants",
                column: "NationalityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_IndCityId",
                table: "Defendants",
                column: "IndCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Nationalities_NationalityId",
                table: "Defendants",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_IndRegionId",
                table: "Defendants",
                column: "IndRegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_IndCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Nationalities_NationalityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_IndRegionId",
                table: "Defendants");

            migrationBuilder.DropTable(
                name: "Nationalities");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_IndCityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_IndRegionId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_NationalityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "Employer",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "EmploymentStatusId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "FamilyName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "GenderId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "GrandfatherName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndAdditionalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndBuildingNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndCityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndDistrict",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndPostalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndStreet",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "IndUnitNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "TribeName",
                table: "Defendants");
        }
    }
}
