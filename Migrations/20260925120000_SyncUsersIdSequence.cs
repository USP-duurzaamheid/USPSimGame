using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using USPSimGame.Data;

#nullable disable

namespace USPSimGame.Migrations
{
    /// <summary>
    /// The seeded Admin user is inserted with an explicit Id, which does not advance the
    /// identity sequence. Without this, the first registered user would get Id 1 and fail.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260925120000_SyncUsersIdSequence")]
    public partial class SyncUsersIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), GREATEST((SELECT MAX("Id") FROM "Users"), 1));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
