using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaintiffSRSComplianceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LicenseSource",
                table: "Plaintiffs",
                newName: "WaqfName");

            migrationBuilder.AlterColumn<string>(
                name: "WaqfOversightType",
                table: "Plaintiffs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Profession",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Plaintiffs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeedSource",
                table: "Plaintiffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourtDeedNumber",
                table: "Plaintiffs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessAddressId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyAddressId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Plaintiffs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Headquarters",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LicenseSourceId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NGOAddressId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOName",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnregisteredCompanyAddress",
                table: "Plaintiffs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnregisteredCompanyCity",
                table: "Plaintiffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WaqfAddressId",
                table: "Plaintiffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfAgencyName",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaqfDescription",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PhoneCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LicenseSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseSources", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "CreatedDate", "IsActive", "IsoCode", "ModifiedDate", "Name", "NameAr", "PhoneCode" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Saudi Arabia", "المملكة العربية السعودية", "966" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "AE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "United Arab Emirates", "الإمارات العربية المتحدة", "971" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "KW", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kuwait", "الكويت", "965" },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "BH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bahrain", "البحرين", "973" },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "QA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Qatar", "قطر", "974" },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "OM", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Oman", "عُمان", "968" },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "EG", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Egypt", "مصر", "20" },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "JO", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jordan", "الأردن", "962" },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "LB", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lebanon", "لبنان", "961" },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SY", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Syria", "سوريا", "963" },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IQ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Iraq", "العراق", "964" },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "YE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yemen", "اليمن", "967" },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "US", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "United States", "الولايات المتحدة", "1" },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "GB", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "United Kingdom", "المملكة المتحدة", "44" },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "FR", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "France", "فرنسا", "33" },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "DE", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Germany", "ألمانيا", "49" },
                    { 17, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "India", "الهند", "91" },
                    { 18, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "PK", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pakistan", "باكستان", "92" },
                    { 19, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CN", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "China", "الصين", "86" },
                    { 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "JP", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Japan", "اليابان", "81" }
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "CityId", "CreatedDate", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Olaya", "العليا" },
                    { 2, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Malaz", "الملز" },
                    { 3, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Naseem", "النسيم" },
                    { 4, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Wurud", "الورود" },
                    { 5, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Sulimaniyah", "السليمانية" },
                    { 6, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Rawdah", "الروضة" },
                    { 7, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Hamra", "الحمراء" },
                    { 8, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Shati", "الشاطئ" },
                    { 9, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Faisaliyah", "الفيصلية" },
                    { 10, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Al Anoud", "العنود" }
                });

            migrationBuilder.InsertData(
                table: "LicenseSources",
                columns: new[] { "Id", "Code", "CreatedDate", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, "MOL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MinistryOfLabor", "وزارة العمل والتنمية الاجتماعية" },
                    { 2, "MOC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MinistryOfCommerce", "وزارة التجارة" },
                    { 3, "GAA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "GeneralAuthorityForAwqaf", "الهيئة العامة للأوقاف" },
                    { 4, "SAMA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SaudiCentralBank", "البنك المركزي السعودي" },
                    { 5, "CMA", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CapitalMarketAuthority", "هيئة السوق المالية" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_BusinessAddressId",
                table: "Plaintiffs",
                column: "BusinessAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_CompanyAddressId",
                table: "Plaintiffs",
                column: "CompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_CountryId",
                table: "Plaintiffs",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_LicenseSourceId",
                table: "Plaintiffs",
                column: "LicenseSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_NGOAddressId",
                table: "Plaintiffs",
                column: "NGOAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Plaintiffs_WaqfAddressId",
                table: "Plaintiffs",
                column: "WaqfAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_CityId",
                table: "Districts",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_Addresses_BusinessAddressId",
                table: "Plaintiffs",
                column: "BusinessAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_Addresses_CompanyAddressId",
                table: "Plaintiffs",
                column: "CompanyAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_Addresses_NGOAddressId",
                table: "Plaintiffs",
                column: "NGOAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_Addresses_WaqfAddressId",
                table: "Plaintiffs",
                column: "WaqfAddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_Countries_CountryId",
                table: "Plaintiffs",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plaintiffs_LicenseSources_LicenseSourceId",
                table: "Plaintiffs",
                column: "LicenseSourceId",
                principalTable: "LicenseSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_Addresses_BusinessAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_Addresses_CompanyAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_Addresses_NGOAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_Addresses_WaqfAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_Countries_CountryId",
                table: "Plaintiffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plaintiffs_LicenseSources_LicenseSourceId",
                table: "Plaintiffs");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "LicenseSources");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_BusinessAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_CompanyAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_CountryId",
                table: "Plaintiffs");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_LicenseSourceId",
                table: "Plaintiffs");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_NGOAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropIndex(
                name: "IX_Plaintiffs_WaqfAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "BusinessAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "CompanyAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "Headquarters",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "LicenseSourceId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "NGOAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "NGOName",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "UnregisteredCompanyAddress",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "UnregisteredCompanyCity",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "WaqfAddressId",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "WaqfAgencyName",
                table: "Plaintiffs");

            migrationBuilder.DropColumn(
                name: "WaqfDescription",
                table: "Plaintiffs");

            migrationBuilder.RenameColumn(
                name: "WaqfName",
                table: "Plaintiffs",
                newName: "LicenseSource");

            migrationBuilder.AlterColumn<string>(
                name: "WaqfOversightType",
                table: "Plaintiffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Profession",
                table: "Plaintiffs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Plaintiffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeedSource",
                table: "Plaintiffs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourtDeedNumber",
                table: "Plaintiffs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
