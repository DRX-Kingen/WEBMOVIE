using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieStreaming.Migrations
{
    /// <inheritdoc />
    public partial class RenameTittleToTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tittle",
                table: "Movies",
                newName: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Movies",
                newName: "Tittle");
        }
    }
}
