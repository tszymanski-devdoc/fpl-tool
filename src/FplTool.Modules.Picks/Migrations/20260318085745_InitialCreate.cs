using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FplTool.Modules.Picks.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "picks_captain_picks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GameweekId = table.Column<int>(type: "int", nullable: false),
                    FplPlayerId = table.Column<int>(type: "int", nullable: false),
                    PlayerWebName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointsScored = table.Column<int>(type: "int", nullable: true),
                    PickedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picks_captain_picks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "picks_gw_points_sync",
                columns: table => new
                {
                    GameweekId = table.Column<int>(type: "int", nullable: false),
                    LastSyncedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsComplete = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picks_gw_points_sync", x => x.GameweekId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_picks_captain_picks_GameweekId",
                table: "picks_captain_picks",
                column: "GameweekId");

            migrationBuilder.CreateIndex(
                name: "IX_picks_captain_picks_UserId_GameweekId",
                table: "picks_captain_picks",
                columns: new[] { "UserId", "GameweekId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "picks_captain_picks");

            migrationBuilder.DropTable(
                name: "picks_gw_points_sync");
        }
    }
}
