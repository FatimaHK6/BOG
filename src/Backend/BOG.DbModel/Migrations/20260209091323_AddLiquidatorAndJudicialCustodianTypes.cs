using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddLiquidatorAndJudicialCustodianTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [RepresentativeTypes] WHERE [Id] = 10)
                BEGIN
                    SET IDENTITY_INSERT [RepresentativeTypes] ON;
                    INSERT INTO [RepresentativeTypes] ([Id], [CreatedDate], [Description], [IsActive], [ModifiedDate], [Name], [NameAr])
                    VALUES
                        (10, '2024-01-01', NULL, 1, '2024-01-01', N'Liquidator', N'مصفي'),
                        (11, '2024-01-01', NULL, 1, '2024-01-01', N'JudicialCustodian', N'حارس قضائي');
                    SET IDENTITY_INSERT [RepresentativeTypes] OFF;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RepresentativeTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "RepresentativeTypes",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
