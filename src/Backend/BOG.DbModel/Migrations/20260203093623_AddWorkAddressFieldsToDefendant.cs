using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkAddressFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkAdditionalCode",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkBuildingNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkCityId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkDistrict",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPostalCode",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkRegionId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkStreet",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkUnitNumber",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_WorkCityId",
                table: "Defendants",
                column: "WorkCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_WorkRegionId",
                table: "Defendants",
                column: "WorkRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Cities_WorkCityId",
                table: "Defendants",
                column: "WorkCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Regions_WorkRegionId",
                table: "Defendants",
                column: "WorkRegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Cities_WorkCityId",
                table: "Defendants");

            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Regions_WorkRegionId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_WorkCityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_WorkRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkAdditionalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkBuildingNumber",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkCityId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkDistrict",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkPostalCode",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkRegionId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkStreet",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "WorkUnitNumber",
                table: "Defendants");
        }
    }
}
