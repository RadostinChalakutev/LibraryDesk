using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryDesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBooksAndCopies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 240, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Isbn = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PublishedYear = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "physical_copies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BookId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_physical_copies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_physical_copies_books_BookId",
                        column: x => x.BookId,
                        principalTable: "books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_books_Isbn",
                table: "books",
                column: "Isbn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_physical_copies_Barcode",
                table: "physical_copies",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_physical_copies_BookId",
                table: "physical_copies",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "physical_copies");

            migrationBuilder.DropTable(
                name: "books");
        }
    }
}
