using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class Merge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Savings_Posts_PostId",
                table: "Savings");

            migrationBuilder.AddForeignKey(
                name: "FK_Savings_Posts_PostId",
                table: "Savings",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Savings_Posts_PostId",
                table: "Savings");

            migrationBuilder.DropForeignKey(
                name: "FK_Savings_Users_UserId",
                table: "Savings");

            migrationBuilder.AddForeignKey(
                name: "FK_Savings_Posts_PostId",
                table: "Savings",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }
    }
}
