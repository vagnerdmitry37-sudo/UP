using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UP.Api.Migrations
{
    /// <inheritdoc />
    public partial class init_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ReplacedByTokenId",
                table: "RefreshTokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ReplacedByTokenId",
                table: "RefreshTokens",
                column: "ReplacedByTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_RefreshTokens_ReplacedByTokenId",
                table: "RefreshTokens",
                column: "ReplacedByTokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_RefreshTokens_ReplacedByTokenId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_ReplacedByTokenId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByTokenId",
                table: "RefreshTokens");
        }
    }
}
