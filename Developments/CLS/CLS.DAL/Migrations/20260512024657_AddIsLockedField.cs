using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLS.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLockedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_locked",
                table: "users");
        }
    }
}
