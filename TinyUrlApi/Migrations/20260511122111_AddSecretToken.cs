using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TinyUrlApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretToken",
                table: "ShortUrls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretToken",
                table: "ShortUrls");
        }
    }
}
