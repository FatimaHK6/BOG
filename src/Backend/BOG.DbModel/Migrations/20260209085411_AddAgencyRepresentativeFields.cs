using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyRepresentativeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RepresentationLetterDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentationLetterNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepresentationLetterSource",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepresentationLetterDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "RepresentationLetterNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "RepresentationLetterSource",
                table: "Representatives");
        }
    }
}
