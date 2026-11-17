using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eklunda.Web.Migrations
{
    /// <inheritdoc />
    public partial class SuggestionCommentsAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Suggestions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                table: "Suggestions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SuggestionComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SuggestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuggestionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuggestionComments_Suggestions_SuggestionId",
                        column: x => x.SuggestionId,
                        principalTable: "Suggestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suggestions_DeletedAt",
                table: "Suggestions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SuggestionComments_SuggestionId",
                table: "SuggestionComments",
                column: "SuggestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuggestionComments");

            migrationBuilder.DropIndex(
                name: "IX_Suggestions_DeletedAt",
                table: "Suggestions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Suggestions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Suggestions");
        }
    }
}
