using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class Dictpatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dictionary_UserId_Phrase",
                table: "Dictionary");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionary_UserId_AnchorId",
                table: "Dictionary",
                columns: new[] { "UserId", "AnchorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dictionary_UserId_AnchorId",
                table: "Dictionary");

            migrationBuilder.CreateIndex(
                name: "IX_Dictionary_UserId_Phrase",
                table: "Dictionary",
                columns: new[] { "UserId", "Phrase" },
                unique: true);
        }
    }
}
