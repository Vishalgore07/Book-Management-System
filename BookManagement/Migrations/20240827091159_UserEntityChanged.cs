using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManagement.Migrations
{
    /// <inheritdoc />
    public partial class UserEntityChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Users",
                newName: "Password");
        }
    }
}
