using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Add_Dealer_Terms_and_Conditions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceivePromotionalContent",
                table: "Dealers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditionsAccepted",
                table: "Dealers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReceivePromotionalContent",
                table: "Audit_Dealers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditionsAccepted",
                table: "Audit_Dealers",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivePromotionalContent",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "TermsAndConditionsAccepted",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "ReceivePromotionalContent",
                table: "Audit_Dealers");

            migrationBuilder.DropColumn(
                name: "TermsAndConditionsAccepted",
                table: "Audit_Dealers");
        }
    }
}
