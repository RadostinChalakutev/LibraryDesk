using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReaderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhysicalCopyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BorrowedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DueOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ReturnedOn = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_loans_physical_copies_PhysicalCopyId",
                        column: x => x.PhysicalCopyId,
                        principalTable: "physical_copies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loans_readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_loans_PhysicalCopyId",
                table: "loans",
                column: "PhysicalCopyId",
                unique: true,
                filter: "\"ReturnedOn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_loans_ReaderId",
                table: "loans",
                column: "ReaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loans");
        }
    }
}
