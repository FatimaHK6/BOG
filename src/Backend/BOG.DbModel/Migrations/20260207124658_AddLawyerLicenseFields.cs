using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddLawyerLicenseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LawyerLicenseDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LawyerLicenseExpiryDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LawyerLicenseNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LawyerLicenseDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "LawyerLicenseExpiryDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "LawyerLicenseNumber",
                table: "Representatives");
        }
    }
}
