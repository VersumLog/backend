using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class Dictpatch2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dictionary_UserId_AnchorId",
                table: "Dictionary");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionary_UserId_PostId_AnchorId",
                table: "Dictionary",
                columns: new[] { "UserId", "PostId", "AnchorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dictionary_UserId_PostId_AnchorId",
                table: "Dictionary");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionary_UserId_AnchorId",
                table: "Dictionary",
                columns: new[] { "UserId", "AnchorId" },
                unique: true);
        }
    }
}
