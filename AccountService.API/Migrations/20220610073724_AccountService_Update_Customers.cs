using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Update_Customers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoolOwnerTypeCode",
                table: "Customers",
                newName: "PoolMaterialCode");

            migrationBuilder.AddColumn<string>(
                name: "HotTubTypeCode",
                table: "Customers",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HotTubTypeCode",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "PoolMaterialCode",
                table: "Customers",
                newName: "PoolOwnerTypeCode");
        }
    }
}
