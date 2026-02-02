using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nhom6_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "ServiceCategories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ServiceCategories",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
}
