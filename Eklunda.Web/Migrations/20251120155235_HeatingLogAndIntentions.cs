using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class HeatingLogAndIntentions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeatingLogEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    TempTankUpper = table.Column<int>(type: "INTEGER", nullable: true),
                    TempTankMid = table.Column<int>(type: "INTEGER", nullable: true),
                    TempTankLower = table.Column<int>(type: "INTEGER", nullable: true),
                    TempTankBoiler = table.Column<int>(type: "INTEGER", nullable: true),
                    LitFire = table.Column<bool>(type: "INTEGER", nullable: false),
                    StokedFire = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeatingLogEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Intentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Done = table.Column<bool>(type: "INTEGER", nullable: false),
                    DoneWhenEvent = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intentions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeatingLogEntries");

            migrationBuilder.DropTable(
                name: "Intentions");
        }
    }
}
