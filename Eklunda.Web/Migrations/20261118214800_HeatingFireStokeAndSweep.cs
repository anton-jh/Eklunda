using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class HeatingFireStokeAndSweep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FireState",
                table: "HeatingLogEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StokeAmount",
                table: "HeatingLogEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SweptBoiler",
                table: "HeatingLogEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FireState",
                table: "HeatingLogEntries");

            migrationBuilder.DropColumn(
                name: "StokeAmount",
                table: "HeatingLogEntries");

            migrationBuilder.DropColumn(
                name: "SweptBoiler",
                table: "HeatingLogEntries");
        }
    }
}
