using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddRepresentativeSRSComplianceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionSource",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeedDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeedNumber",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeedSource",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Representatives",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityExpiryDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IdentityIssueDate",
                table: "Representatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NationalityId",
                table: "Representatives",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecisionDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "DecisionNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "DecisionSource",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "DeedDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "DeedNumber",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "DeedSource",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "IdentityExpiryDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "IdentityIssueDate",
                table: "Representatives");

            migrationBuilder.DropColumn(
                name: "NationalityId",
                table: "Representatives");
        }
    }
}
