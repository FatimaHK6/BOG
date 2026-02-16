using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOG.DbModel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArabicRequestStatusNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update Arabic names for request statuses
            // Most are already correct from seed data, but AutoRejected needs to be updated
            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'طلب جديد' WHERE Id = 2");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'مقيد حديثًا' WHERE Id = 4");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'تم حفظ الطلب' WHERE Id = 5");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'استكمال النواقص' WHERE Id = 6");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'عرض على رئيس المحكمة' WHERE Id = 7");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'لم يتم استكمال النواقص' WHERE Id = 9");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Arabic names for request statuses to original seed values
            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'طلب جديد' WHERE Id = 2");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'مقيد حديثًا' WHERE Id = 4");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'تم حفظ الطلب' WHERE Id = 5");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'استكمال النواقص' WHERE Id = 6");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'عرض على رئيس المحكمة' WHERE Id = 7");

            migrationBuilder.Sql(
                "UPDATE RequestStatuses SET NameAr = N'مرفوض تلقائياً' WHERE Id = 9");
        }
    }
}
