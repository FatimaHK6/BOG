using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class AddHierarchicalClassificationLevels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Level1",
                table: "Classifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level2",
                table: "Classifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level3",
                table: "Classifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level4",
                table: "Classifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Classifications",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Classifications_Level1_Level2_Level3_Level4",
                table: "Classifications",
                columns: new[] { "Level1", "Level2", "Level3", "Level4" },
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Classifications_Level1_Level2_Level3_Level4",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "Level1",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "Level2",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "Level3",
                table: "Classifications");

            migrationBuilder.DropColumn(
                name: "Level4",
                table: "Classifications");
        }
    }
}
