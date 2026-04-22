using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAndParentTables : Migration
    {
        /// <inheritdoc />
        /// BASELINE: Bảng parents và students đã được tạo sẵn bởi 01_CLS_Initial_Schema.sql
        /// trên Supabase. Migration này chỉ đăng ký vào __EFMigrationsHistory mà không tạo lại bảng.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // intentionally empty — baseline
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // intentionally empty — baseline
        }
    }
}
