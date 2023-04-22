using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Update_Customer_Add_IsDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoolOwnerTypeCode",
                table: "Audit_Customers",
                newName: "PoolMaterialCode");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FirstFreeCallAvailed",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotTubTypeCode",
                table: "Audit_Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetDate",
                table: "Audit_Customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilePhotoBlobId",
                table: "Audit_Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditionsAccepted",
                table: "Audit_Customers",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FirstFreeCallAvailed",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "HotTubTypeCode",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "PasswordResetDate",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoBlobId",
                table: "Audit_Customers");

            migrationBuilder.DropColumn(
                name: "TermsAndConditionsAccepted",
                table: "Audit_Customers");

            migrationBuilder.RenameColumn(
                name: "PoolMaterialCode",
                table: "Audit_Customers",
                newName: "PoolOwnerTypeCode");
        }
    }
}
