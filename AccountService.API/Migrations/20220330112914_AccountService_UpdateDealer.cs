using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_UpdateDealer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Businesses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PoolCount",
                table: "Businesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Businesses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Audit_Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PoolCount",
                table: "Audit_Businesses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartYear",
                table: "Audit_Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "Audit_Businesses",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PoolCount",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Audit_Businesses");

            migrationBuilder.DropColumn(
                name: "PoolCount",
                table: "Audit_Businesses");

            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Audit_Businesses");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "Audit_Businesses");
        }
    }
}
