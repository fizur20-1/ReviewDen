using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewDen.Migrations
{
    /// <inheritdoc />
    public partial class AddMood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Mood",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mood",
                table: "Posts");
        }
    }
}
