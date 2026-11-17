using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class IntentionRemindersAndHeatingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextReminderAt",
                table: "Intentions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RemindAt",
                table: "Intentions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemindersSent",
                table: "Intentions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HeatingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    NextCheckReminderHours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeatingSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "HeatingSettings",
                columns: new[] { "Id", "NextCheckReminderHours" },
                values: new object[] { 1, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeatingSettings");

            migrationBuilder.DropColumn(
                name: "NextReminderAt",
                table: "Intentions");

            migrationBuilder.DropColumn(
                name: "RemindAt",
                table: "Intentions");

            migrationBuilder.DropColumn(
                name: "RemindersSent",
                table: "Intentions");
        }
    }
}
