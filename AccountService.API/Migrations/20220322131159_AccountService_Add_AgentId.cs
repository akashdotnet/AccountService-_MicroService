using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Add_AgentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentId",
                table: "Experts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentId",
                table: "Audit_Experts",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "Experts");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "Audit_Experts");
        }
    }
}
