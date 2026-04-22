using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CLS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty Migration Baseline:
            // Bảng users đã được tạo trên Supabase qua script SQL.
            // Đoạn code này được dọn trống để EF Core chỉ tạo bảng __EFMigrationsHistory
            // và không báo lỗi đụng độ bảng.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty Migration Baseline
        }
    }
}
