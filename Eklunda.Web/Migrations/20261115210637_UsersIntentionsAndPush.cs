using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class UsersIntentionsAndPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Intentions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DoneAt",
                table: "Intentions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DoneBy",
                table: "Intentions",
                type: "uuid",
                nullable: true);

            // Done (bool) -> DoneAt: the creation time is the best guess we have for old rows.
            migrationBuilder.Sql("""UPDATE "Intentions" SET "DoneAt" = "CreatedDate" WHERE "Done";""");

            migrationBuilder.DropColumn(
                name: "Done",
                table: "Intentions");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "HeatingLogEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "text", nullable: false),
                    P256dh = table.Column<string>(type: "text", nullable: false),
                    Auth = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VapidKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    PrivateKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VapidKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Intentions_DoneWhenEvent_DoneAt",
                table: "Intentions",
                columns: new[] { "DoneWhenEvent", "DoneAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions",
                column: "Endpoint",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushSubscriptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VapidKeys");

            migrationBuilder.DropIndex(
                name: "IX_Intentions_DoneWhenEvent_DoneAt",
                table: "Intentions");

            migrationBuilder.AddColumn<bool>(
                name: "Done",
                table: "Intentions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""UPDATE "Intentions" SET "Done" = "DoneAt" IS NOT NULL;""");

            migrationBuilder.DropColumn(
                name: "DoneAt",
                table: "Intentions");

            migrationBuilder.DropColumn(
                name: "DoneBy",
                table: "Intentions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HeatingLogEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Intentions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

        }
    }
}
