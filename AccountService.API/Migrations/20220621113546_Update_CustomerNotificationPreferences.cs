using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class Update_CustomerNotificationPreferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailNotificationEnabled",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPushNotificationEnabled",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSmsNotificationEnabled",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailNotificationEnabled",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPushNotificationEnabled",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSmsNotificationEnabled",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCommunicationsEmail",
                table: "Audit_Businesses",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailNotificationEnabled",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsPushNotificationEnabled",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsSmsNotificationEnabled",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsEmailNotificationEnabled",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "IsPushNotificationEnabled",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "IsSmsNotificationEnabled",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "PreferredCommunicationsEmail",
                table: "Audit_Businesses");
        }
    }
}
