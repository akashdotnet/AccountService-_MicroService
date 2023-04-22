using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Remove_Dealer_Position : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Audit_Dealers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Dealers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Audit_Dealers",
                type: "text",
                nullable: true);
        }
    }
}
