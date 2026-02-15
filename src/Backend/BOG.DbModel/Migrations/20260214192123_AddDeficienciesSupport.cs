using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddDeficienciesSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeficiencyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeficiencyTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeficiencyDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeficiencyTypeId = table.Column<int>(type: "int", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeficiencyDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeficiencyDescriptions_DeficiencyTypes_DeficiencyTypeId",
                        column: x => x.DeficiencyTypeId,
                        principalTable: "DeficiencyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestDeficiencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseRegistrationRequestId = table.Column<int>(type: "int", nullable: false),
                    DeficiencyDescriptionId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestDeficiencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestDeficiencies_CaseRegistrationRequests_CaseRegistrationRequestId",
                        column: x => x.CaseRegistrationRequestId,
                        principalTable: "CaseRegistrationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestDeficiencies_DeficiencyDescriptions_DeficiencyDescriptionId",
                        column: x => x.DeficiencyDescriptionId,
                        principalTable: "DeficiencyDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DeficiencyTypes",
                columns: new[] { "Id", "CreatedDate", "DisplayOrder", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CaseSubject", "موضوع الدعوى" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CaseClaims", "طلبات الدعوى" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CaseGrounds", "أسانيد الدعوى" },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RelatedCases", "الدعاوى المرتبطة" },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "CaseAttachments", "مرفقات الدعوى" },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AdditionalCaseInfo", "معلومات إضافية للدعوى" }
                });

            migrationBuilder.InsertData(
                table: "DeficiencyDescriptions",
                columns: new[] { "Id", "CreatedDate", "DeficiencyTypeId", "DescriptionAr", "DescriptionEn", "DisplayOrder", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "موضوع الدعوى غير واضح", "Case subject is unclear", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "موضوع الدعوى ناقص ويحتاج إلى تفاصيل إضافية", "Case subject is incomplete and needs additional details", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "موضوع الدعوى لا يتفق مع الطلبات", "Case subject does not match the claims", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "طلبات الدعوى غير محددة", "Claims are not specified", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "طلبات الدعوى غير مكتملة", "Claims are incomplete", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "طلبات الدعوى غير متوافقة مع الموضوع", "Claims are not compatible with the subject", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "أسانيد الدعوى ناقصة", "Grounds are incomplete", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "أسانيد الدعوى غير كافية", "Grounds are insufficient", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "أسانيد الدعوى غير مرتبطة بالطلبات", "Grounds are not related to the claims", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "معلومات الدعوى المرتبطة ناقصة", "Related case information is incomplete", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "رقم الدعوى المرتبطة غير صحيح", "Related case number is incorrect", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "مرفقات الدعوى ناقصة", "Attachments are missing", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "مرفقات الدعوى غير واضحة", "Attachments are unclear", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "مرفقات الدعوى غير مكتملة", "Attachments are incomplete", 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "معلومات إضافية مطلوبة", "Additional information required", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "بيانات إضافية ناقصة", "Additional data is missing", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeficiencyDescriptions_DeficiencyTypeId",
                table: "DeficiencyDescriptions",
                column: "DeficiencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestDeficiencies_CaseRegistrationRequestId",
                table: "RequestDeficiencies",
                column: "CaseRegistrationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestDeficiencies_DeficiencyDescriptionId",
                table: "RequestDeficiencies",
                column: "DeficiencyDescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestDeficiencies");

            migrationBuilder.DropTable(
                name: "DeficiencyDescriptions");

            migrationBuilder.DropTable(
                name: "DeficiencyTypes");
        }
    }
}
