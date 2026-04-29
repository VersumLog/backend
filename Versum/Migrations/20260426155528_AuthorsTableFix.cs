using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class AuthorsTableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_Users_Id",
                table: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Authors_Id",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Authors");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Authors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_Users_AuthorId",
                table: "Authors",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_Users_AuthorId",
                table: "Authors");

            migrationBuilder.AlterColumn<int>(
                name: "AuthorId",
                table: "Authors",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Authors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Id",
                table: "Authors",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_Users_Id",
                table: "Authors",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
