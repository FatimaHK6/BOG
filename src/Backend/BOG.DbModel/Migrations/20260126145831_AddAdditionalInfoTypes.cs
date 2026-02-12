using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalInfoTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalRemarks",
                table: "AdditionalInfos");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "AdditionalInfos");

            migrationBuilder.DropColumn(
                name: "LegalBasis",
                table: "AdditionalInfos");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AdditionalInfos");

            migrationBuilder.DropColumn(
                name: "PriorityLevel",
                table: "AdditionalInfos");

            migrationBuilder.DropColumn(
                name: "RelatedCaseNumber",
                table: "AdditionalInfos");

            migrationBuilder.CreateTable(
                name: "AdditionalInfoTrademarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalInfoId = table.Column<int>(type: "int", nullable: false),
                    RequestNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoTrademarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoTrademarks_AdditionalInfos_AdditionalInfoId",
                        column: x => x.AdditionalInfoId,
                        principalTable: "AdditionalInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GovernmentEntities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernmentEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalInfoServiceRightses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalInfoId = table.Column<int>(type: "int", nullable: false),
                    HasComplaint = table.Column<bool>(type: "bit", nullable: true),
                    ComplaintNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ComplaintDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComplaintAuthorityId = table.Column<int>(type: "int", nullable: true),
                    ComplaintDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SystemResult = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoServiceRightses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoServiceRightses_AdditionalInfos_AdditionalInfoId",
                        column: x => x.AdditionalInfoId,
                        principalTable: "AdditionalInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoServiceRightses_GovernmentEntities_ComplaintAuthorityId",
                        column: x => x.ComplaintAuthorityId,
                        principalTable: "GovernmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalInfoManagementDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalInfoId = table.Column<int>(type: "int", nullable: false),
                    DecisionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotificationMethodId = table.Column<int>(type: "int", nullable: false),
                    IssuingAuthorityId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalInfoManagementDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoManagementDecisions_AdditionalInfos_AdditionalInfoId",
                        column: x => x.AdditionalInfoId,
                        principalTable: "AdditionalInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoManagementDecisions_GovernmentEntities_IssuingAuthorityId",
                        column: x => x.IssuingAuthorityId,
                        principalTable: "GovernmentEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdditionalInfoManagementDecisions_NotificationMethods_NotificationMethodId",
                        column: x => x.NotificationMethodId,
                        principalTable: "NotificationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GovernmentEntities",
                columns: new[] { "Id", "Code", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, "MOJ", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Justice", "وزارة العدل" },
                    { 2, "MOI", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Interior", "وزارة الداخلية" },
                    { 3, "MOF", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Finance", "وزارة المالية" },
                    { 4, "MOL", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Labor", "وزارة العمل والتنمية الاجتماعية" },
                    { 5, "MOC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ministry of Commerce", "وزارة التجارة والاستثمار" },
                    { 6, "CSB", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Civil Service Bureau", "ديوان الخدمة المدنية" },
                    { 7, "GC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General Court", "المحكمة العامة" },
                    { 8, "AC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Appellate Court", "محكمة الاستئناف" }
                });

            migrationBuilder.InsertData(
                table: "NotificationMethods",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "ModifiedDate", "Name", "NameAr" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Notification", "الإبلاغ بالقرار" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Knowledge", "العلم به" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "OfficialGazette", "الجريدة الرسمية" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoManagementDecisions_AdditionalInfoId",
                table: "AdditionalInfoManagementDecisions",
                column: "AdditionalInfoId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoManagementDecisions_IssuingAuthorityId",
                table: "AdditionalInfoManagementDecisions",
                column: "IssuingAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoManagementDecisions_NotificationMethodId",
                table: "AdditionalInfoManagementDecisions",
                column: "NotificationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoServiceRightses_AdditionalInfoId",
                table: "AdditionalInfoServiceRightses",
                column: "AdditionalInfoId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoServiceRightses_ComplaintAuthorityId",
                table: "AdditionalInfoServiceRightses",
                column: "ComplaintAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalInfoTrademarks_AdditionalInfoId",
                table: "AdditionalInfoTrademarks",
                column: "AdditionalInfoId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalInfoManagementDecisions");

            migrationBuilder.DropTable(
                name: "AdditionalInfoServiceRightses");

            migrationBuilder.DropTable(
                name: "AdditionalInfoTrademarks");

            migrationBuilder.DropTable(
                name: "NotificationMethods");

            migrationBuilder.DropTable(
                name: "GovernmentEntities");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalRemarks",
                table: "AdditionalInfos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "AdditionalInfos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalBasis",
                table: "AdditionalInfos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AdditionalInfos",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriorityLevel",
                table: "AdditionalInfos",
                type: "int",
                nullable: true,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "RelatedCaseNumber",
                table: "AdditionalInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
