using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fei.Is.Api.Data.Migrations
{
    public partial class QRScanDeviceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove old text column and create a fresh int enum column
            migrationBuilder.DropColumn(name: "DeviceType", table: "ScanRecords");

            migrationBuilder.AddColumn<int>(name: "DeviceType", table: "ScanRecords", type: "integer", nullable: false, defaultValue: 3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to text
            migrationBuilder.DropColumn(name: "DeviceType", table: "ScanRecords");

            migrationBuilder.AddColumn<string>(name: "DeviceType", table: "ScanRecords", type: "text", nullable: false, defaultValue: "Other");
        }
    }
}
