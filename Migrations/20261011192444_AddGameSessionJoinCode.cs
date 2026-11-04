using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USPSimGame.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionJoinCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "GameSessions",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_JoinCode",
                table: "GameSessions",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameSessions_JoinCode",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "GameSessions");
        }
    }
}
