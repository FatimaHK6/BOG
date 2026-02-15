using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 1)
                BEGIN
                    SET IDENTITY_INSERT [Users] ON;
                    INSERT INTO [Users] ([Id], [CreatedDate], [Email], [FirstName], [IsActive], [LastName], [ModifiedDate], [PhoneNumber])
                    VALUES (1, '2026-02-08T18:57:03.6496918Z', 'system@bog.sa', 'System', 1, 'User', '2026-02-08T18:57:03.6496918Z', NULL);
                    SET IDENTITY_INSERT [Users] OFF;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
