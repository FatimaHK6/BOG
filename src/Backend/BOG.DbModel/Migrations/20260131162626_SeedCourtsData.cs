using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class SeedCourtsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Courts",
                columns: new[] { "Id", "Name", "NameAr", "RegionId", "CityId", "IsActive", "IsDeleted", "CreatedDate", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, "Riyadh General Court", "المحكمة العامة بالرياض", 1, 1, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Riyadh Commercial Court", "المحكمة التجارية بالرياض", 1, 1, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Jeddah General Court", "المحكمة العامة بجدة", 2, 2, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Jeddah Commercial Court", "المحكمة التجارية بجدة", 2, 2, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "Dammam General Court", "المحكمة العامة بالدمام", 5, 5, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "Dammam Commercial Court", "المحكمة التجارية بالدمام", 5, 5, true, false, new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
