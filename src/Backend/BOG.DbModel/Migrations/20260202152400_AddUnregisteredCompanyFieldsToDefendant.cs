using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddUnregisteredCompanyFieldsToDefendant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Defendants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Defendants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Defendants_CountryId",
                table: "Defendants",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Countries_CountryId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_CountryId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Defendants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Defendants");
        }
    }
}
