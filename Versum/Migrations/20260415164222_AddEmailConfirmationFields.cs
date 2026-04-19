using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailConfirmationToken",
                table: "Users",
                newName: "EmailConfirmationTokenHash");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailTokenExpiryDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTokenExpiryDate",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmationTokenHash",
                table: "Users",
                newName: "EmailConfirmationToken");
        }
    }
}
