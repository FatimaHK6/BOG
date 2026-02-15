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
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Courts] WHERE [Id] = 1)
                BEGIN
                    SET IDENTITY_INSERT [Courts] ON;
                    INSERT INTO [Courts] ([Id], [Name], [NameAr], [RegionId], [CityId], [IsActive], [IsDeleted], [CreatedDate], [ModifiedDate])
                    VALUES
                        (1, N'Riyadh General Court', N'المحكمة العامة بالرياض', 1, 1, 1, 0, '2026-02-08', '2026-02-08'),
                        (2, N'Riyadh Commercial Court', N'المحكمة التجارية بالرياض', 1, 1, 1, 0, '2026-02-08', '2026-02-08'),
                        (3, N'Jeddah General Court', N'المحكمة العامة بجدة', 2, 2, 1, 0, '2026-02-08', '2026-02-08'),
                        (4, N'Jeddah Commercial Court', N'المحكمة التجارية بجدة', 2, 2, 1, 0, '2026-02-08', '2026-02-08'),
                        (5, N'Dammam General Court', N'المحكمة العامة بالدمام', 5, 5, 1, 0, '2026-02-08', '2026-02-08'),
                        (6, N'Dammam Commercial Court', N'المحكمة التجارية بالدمام', 5, 5, 1, 0, '2026-02-08', '2026-02-08');
                    SET IDENTITY_INSERT [Courts] OFF;
                END
            ");
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
