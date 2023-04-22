using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Update_AuditCustomerAddresses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Audit_CustomerAddresses");
            
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Audit_CustomerAddresses",
                type: "integer",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Audit_CustomerAddresses");
            
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "Audit_CustomerAddresses",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "Audit_CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AddressId",
                table: "Audit_CustomerAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
