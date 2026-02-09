using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddRepresentativeDocumentAttachmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AttachmentTypes",
                columns: new[] { "Id", "AllowedExtensions", "CreatedDate", "Description", "IsActive", "IsMandatory", "MaxFileSizeBytes", "ModifiedDate", "Name", "NameAr" },
                values: new object[] { 10, ".pdf", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, 4194304, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RepresentativeDocument", "صورة التمثيل" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AttachmentTypes",
                keyColumn: "Id",
                keyValue: 10);
        }
    }
}
