using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USPSimGame.Migrations
{
    /// <inheritdoc />
    public partial class Identity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "LegacyUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LegacyUser",
                table: "LegacyUser",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LegacyUser",
                table: "LegacyUser");

            migrationBuilder.RenameTable(
                name: "LegacyUser",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }
    }
}
