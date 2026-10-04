using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "readers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MembershipNumber = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_readers_MembershipNumber",
                table: "readers",
                column: "MembershipNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "readers");
        }
    }
}
