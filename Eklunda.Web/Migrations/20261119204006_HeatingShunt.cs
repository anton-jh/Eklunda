using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class HeatingShunt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Shunt",
                table: "HeatingLogEntries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shunt",
                table: "HeatingLogEntries");
        }
    }
}
