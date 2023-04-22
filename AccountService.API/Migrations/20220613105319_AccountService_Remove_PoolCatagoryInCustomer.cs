using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Remove_PoolCatagoryInCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PoolCategoryCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PoolCategoryCode",
                table: "Audit_Customers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PoolCategoryCode",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoolCategoryCode",
                table: "Audit_Customers",
                type: "text",
                nullable: true);
        }
    }
}
