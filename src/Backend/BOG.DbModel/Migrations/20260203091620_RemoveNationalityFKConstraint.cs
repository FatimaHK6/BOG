using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNationalityFKConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Defendants_Nationalities_NationalityId",
                table: "Defendants");

            migrationBuilder.DropIndex(
                name: "IX_Defendants_NationalityId",
                table: "Defendants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Defendants_NationalityId",
                table: "Defendants",
                column: "NationalityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Defendants_Nationalities_NationalityId",
                table: "Defendants",
                column: "NationalityId",
                principalTable: "Nationalities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
