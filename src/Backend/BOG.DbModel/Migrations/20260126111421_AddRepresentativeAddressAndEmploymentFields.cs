using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddRepresentativeAddressAndEmploymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceAdditionalCode",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceBuildingNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceCityId",
                table: "Representatives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceDistrict",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidencePostalCode",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResidenceRegionId",
                table: "Representatives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceStreet",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResidenceUnitNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkAdditionalCode",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkBuildingNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkCityId",
                table: "Representatives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkDistrict",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPostalCode",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkRegionId",
                table: "Representatives",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkStreet",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkUnitNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Employer",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceAdditionalCode",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceBuildingNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceCityId",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceDistrict",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidencePostalCode",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceRegionId",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceStreet",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "ResidenceUnitNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkAdditionalCode",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkBuildingNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkCityId",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkDistrict",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkPostalCode",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkRegionId",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkStreet",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "WorkUnitNumber",
                table: "Representatives");
        }
    }
}
