using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyRepresentativeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RepresentationDocNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentationDocSource",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentationDocType",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentativeCapacity",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepresentationDocNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "RepresentationDocSource",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "RepresentationDocType",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "RepresentativeCapacity",
                table: "Representatives");
        }
    }
}
