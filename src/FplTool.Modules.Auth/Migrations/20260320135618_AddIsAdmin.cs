using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FplTool.Modules.Auth.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "auth_users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "auth_users");
        }
    }
}
