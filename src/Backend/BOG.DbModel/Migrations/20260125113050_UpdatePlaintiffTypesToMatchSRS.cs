using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlaintiffTypesToMatchSRS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "IndividualWithoutId", "فرد بدون هوية" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "BusinessOwner", "صاحب مؤسسة" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "RegisteredCompany", "شركة مسجلة" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "UnregisteredCompany", "شركة غير مسجلة" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "GovernmentAgency", "جهة حكومية" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Society", "جمعية/مؤسسة أهلية" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Waqf", "وقف" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityId",
                table: "Addresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_RegionId",
                table: "Addresses",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Regions_RegionId",
                table: "Addresses",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Regions_RegionId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_CityId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_RegionId",
                table: "Addresses");

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Company", "شركة" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "GovernmentAgency", "جهة حكومية" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Society", "جمعية" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Waqf", "وقف" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "MinorOrIncapacitated", "قاصر أو محجور عليه" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "Heir", "وريث" });

            migrationBuilder.UpdateData(
                table: "PlaintiffTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "NameAr" },
                values: new object[] { "BankruptEstate", "تفليسة" });
        }
    }
}
