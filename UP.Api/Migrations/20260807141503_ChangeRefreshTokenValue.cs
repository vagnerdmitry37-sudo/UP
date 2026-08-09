using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UP.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRefreshTokenValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshTokens",
                newName: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "RefreshTokens",
                newName: "Token");
        }
    }
}
