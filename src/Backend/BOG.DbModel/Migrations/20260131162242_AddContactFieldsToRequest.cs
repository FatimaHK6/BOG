using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddContactFieldsToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CaseRegistrationRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryMobile",
                table: "CaseRegistrationRequests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMobile",
                table: "CaseRegistrationRequests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "PrimaryMobile",
                table: "CaseRegistrationRequests");

            migrationBuilder.DropColumn(
                name: "SecondaryMobile",
                table: "CaseRegistrationRequests");
        }
    }
}
