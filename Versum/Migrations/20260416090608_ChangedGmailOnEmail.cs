using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class ChangedGmailOnEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gmail",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Gmail",
                table: "Users",
                newName: "IX_Users_Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Gmail");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
                newName: "IX_Users_Gmail");
        }
    }
}
