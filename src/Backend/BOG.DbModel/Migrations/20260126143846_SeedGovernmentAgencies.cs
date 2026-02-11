using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class SeedGovernmentAgencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GovernmentAgencies",
                columns: new[] { "Id", "Code", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, "MOJ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Justice", "وزارة العدل" },
                    { 2, "MOI", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Interior", "وزارة الداخلية" },
                    { 3, "MOF", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Finance", "وزارة المالية" },
                    { 4, "MOH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Health", "وزارة الصحة" },
                    { 5, "MOE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Education", "وزارة التعليم" },
                    { 6, "MOC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Commerce", "وزارة التجارة" },
                    { 7, "HRSD", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Human Resources", "وزارة الموارد البشرية والتنمية الاجتماعية" },
                    { 8, "MOT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Transport", "وزارة النقل والخدمات اللوجستية" },
                    { 9, "MOMRA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Municipal Affairs", "وزارة الشؤون البلدية والقروية والإسكان" },
                    { 10, "MEWA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Environment", "وزارة البيئة والمياه والزراعة" },
                    { 11, "ZATCA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General Authority of Zakat and Tax", "هيئة الزكاة والضريبة والجمارك" },
                    { 12, "GOSI", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General Organization for Social Insurance", "المؤسسة العامة للتأمينات الاجتماعية" },
                    { 13, "SAMA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Saudi Central Bank", "البنك المركزي السعودي" },
                    { 14, "CMA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Capital Market Authority", "هيئة السوق المالية" },
                    { 15, "RC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Royal Court", "الديوان الملكي" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "GovernmentAgencies",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
