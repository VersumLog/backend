using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Versum.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
  
          

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);


        }

        /// <inheritdoc />
       
    }
}
