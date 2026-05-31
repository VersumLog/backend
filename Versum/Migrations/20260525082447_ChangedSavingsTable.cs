using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class ChangedSavingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Savings");

            migrationBuilder.AddColumn<DateTime>(
                name: "SavedAt",
                table: "Savings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedAt",
                table: "Savings");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Savings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
