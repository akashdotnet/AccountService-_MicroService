using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Update_Customer_ProfilePictureId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfilePhotoBlobId",
                table: "Customers",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePhotoBlobId",
                table: "Customers");
        }
    }
}
