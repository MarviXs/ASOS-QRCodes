using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class QRCodeStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "QRCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CornerDotStyle",
                table: "QRCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CornerSquareStyle",
                table: "QRCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DotStyle",
                table: "QRCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "CornerDotStyle",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "CornerSquareStyle",
                table: "QRCodes");

            migrationBuilder.DropColumn(
                name: "DotStyle",
                table: "QRCodes");
        }
    }
}
