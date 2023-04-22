using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Update_BusinessStartYear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Businesses");
            
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Audit_Businesses");

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Businesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Audit_Businesses",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Businesses");
            
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Audit_Businesses");

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Audit_Businesses",
                type: "text",
                nullable: true);
        }
    }
}
