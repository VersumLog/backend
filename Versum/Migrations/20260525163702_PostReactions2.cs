using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class PostReactions2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostReactions_UserId_PostId_Type",
                table: "PostReactions");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "PostReactions",
                newName: "ViewCount");

            migrationBuilder.RenameColumn(
                name: "ReactedAt",
                table: "PostReactions",
                newName: "LastInteractedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsLiked",
                table: "PostReactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "PriorityScore",
                table: "PostReactions",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_UserId_PostId",
                table: "PostReactions",
                columns: new[] { "UserId", "PostId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostReactions_UserId_PostId",
                table: "PostReactions");

            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "PostReactions");

            migrationBuilder.DropColumn(
                name: "PriorityScore",
                table: "PostReactions");

            migrationBuilder.RenameColumn(
                name: "ViewCount",
                table: "PostReactions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "LastInteractedAt",
                table: "PostReactions",
                newName: "ReactedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostReactions_UserId_PostId_Type",
                table: "PostReactions",
                columns: new[] { "UserId", "PostId", "Type" },
                unique: true);
        }
    }
}
