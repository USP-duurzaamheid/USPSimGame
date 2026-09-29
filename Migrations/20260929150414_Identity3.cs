using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USPSimGame.Migrations
{
    /// <inheritdoc />
    public partial class Identity3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LegacyUser",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "Username" },
                values: new object[] { "AQAAAAIAAYagAAAAEITB3XoBD9OUNfCEFQjdACjOmvavSu6DAGcOCZui+ELzkJlD0TPulJwrfEH7Bdajfg==", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LegacyUser",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "Username" },
                values: new object[] { "AQAAAAIAAYagAAAAEKstQTVmO/0bmR5/P2B+mTIYP9Ju76yHdGFRYt7uq9Im2XkmV3pwZpvDAMTmlgzY3w==", "Admin" });
        }
    }
}
