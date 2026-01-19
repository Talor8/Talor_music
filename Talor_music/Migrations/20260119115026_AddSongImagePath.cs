using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talor_music.Migrations
{
    /// <inheritdoc />
    public partial class AddSongImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Song",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Song");
        }
    }
}
