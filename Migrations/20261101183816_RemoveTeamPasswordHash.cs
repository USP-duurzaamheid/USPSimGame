using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USPSimGame.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTeamPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Teams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Teams",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
