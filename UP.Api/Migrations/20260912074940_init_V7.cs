using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UP.Api.Migrations
{
    /// <inheritdoc />
    public partial class init_V7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Collections",
                newName: "Label");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Collections",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Icon", "SortedBy" },
                values: new object[] { "", "Label" });

            migrationBuilder.UpdateData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Icon", "SortedBy" },
                values: new object[] { "", "Label" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "Collections",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NewJson = table.Column<string>(type: "text", nullable: true),
                    OldJson = table.Column<string>(type: "text", nullable: true),
                    What = table.Column<int>(type: "integer", nullable: false),
                    When = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Where = table.Column<int>(type: "integer", nullable: false),
                    Who = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 1,
                column: "SortedBy",
                value: "Name");

            migrationBuilder.UpdateData(
                table: "Collections",
                keyColumn: "Id",
                keyValue: 2,
                column: "SortedBy",
                value: "Name");
        }
    }
}
