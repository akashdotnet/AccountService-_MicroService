using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Rename_organization_To_Business_Main_Tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Organizations_BusinessId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationBrands_Organizations_BusinessId",
                table: "OrganizationBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLocations_Address_AddressId",
                table: "OrganizationLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLocations_Organizations_BusinessId",
                table: "OrganizationLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLocationServiceableCounties_OrganizationLocatio~",
                table: "OrganizationLocationServiceableCounties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationLocationServiceableCounties",
                table: "OrganizationLocationServiceableCounties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationLocations",
                table: "OrganizationLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganizationBrands",
                table: "OrganizationBrands");

            migrationBuilder.RenameTable(
                name: "Organizations",
                newName: "Businesses");

            migrationBuilder.RenameTable(
                name: "OrganizationLocationServiceableCounties",
                newName: "BusinessLocationServiceableCounties");

            migrationBuilder.RenameTable(
                name: "OrganizationLocations",
                newName: "BusinessLocations");

            migrationBuilder.RenameTable(
                name: "OrganizationBrands",
                newName: "BusinessBrands");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocationServiceableCounties_BusinessLocationId",
                table: "BusinessLocationServiceableCounties",
                newName: "IX_BusinessLocationServiceableCounties_BusinessLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocations_BusinessId",
                table: "BusinessLocations",
                newName: "IX_BusinessLocations_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocations_AddressId",
                table: "BusinessLocations",
                newName: "IX_BusinessLocations_AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationBrands_BusinessId",
                table: "BusinessBrands",
                newName: "IX_BusinessBrands_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessLocationServiceableCounties",
                table: "BusinessLocationServiceableCounties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessLocations",
                table: "BusinessLocations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessBrands",
                table: "BusinessBrands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessBrands_Businesses_BusinessId",
                table: "BusinessBrands",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessLocations_Address_AddressId",
                table: "BusinessLocations",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessLocations_Businesses_BusinessId",
                table: "BusinessLocations",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessLocationServiceableCounties_BusinessLocations_Busin~",
                table: "BusinessLocationServiceableCounties",
                column: "BusinessLocationId",
                principalTable: "BusinessLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Businesses_BusinessId",
                table: "Dealers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessBrands_Businesses_BusinessId",
                table: "BusinessBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessLocations_Address_AddressId",
                table: "BusinessLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessLocations_Businesses_BusinessId",
                table: "BusinessLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessLocationServiceableCounties_BusinessLocations_Busin~",
                table: "BusinessLocationServiceableCounties");

            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Businesses_BusinessId",
                table: "Dealers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessLocationServiceableCounties",
                table: "BusinessLocationServiceableCounties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessLocations",
                table: "BusinessLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Businesses",
                table: "Businesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BusinessBrands",
                table: "BusinessBrands");

            migrationBuilder.RenameTable(
                name: "BusinessLocationServiceableCounties",
                newName: "OrganizationLocationServiceableCounties");

            migrationBuilder.RenameTable(
                name: "BusinessLocations",
                newName: "OrganizationLocations");

            migrationBuilder.RenameTable(
                name: "Businesses",
                newName: "Organizations");

            migrationBuilder.RenameTable(
                name: "BusinessBrands",
                newName: "OrganizationBrands");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessLocationServiceableCounties_BusinessLocationId",
                table: "OrganizationLocationServiceableCounties",
                newName: "IX_OrganizationLocationServiceableCounties_BusinessLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessLocations_BusinessId",
                table: "OrganizationLocations",
                newName: "IX_OrganizationLocations_BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessLocations_AddressId",
                table: "OrganizationLocations",
                newName: "IX_OrganizationLocations_AddressId");

            migrationBuilder.RenameIndex(
                name: "IX_BusinessBrands_BusinessId",
                table: "OrganizationBrands",
                newName: "IX_OrganizationBrands_BusinessId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationLocationServiceableCounties",
                table: "OrganizationLocationServiceableCounties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationLocations",
                table: "OrganizationLocations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganizationBrands",
                table: "OrganizationBrands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Organizations_BusinessId",
                table: "Dealers",
                column: "BusinessId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationBrands_Organizations_BusinessId",
                table: "OrganizationBrands",
                column: "BusinessId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLocations_Address_AddressId",
                table: "OrganizationLocations",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLocations_Organizations_BusinessId",
                table: "OrganizationLocations",
                column: "BusinessId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLocationServiceableCounties_OrganizationLocatio~",
                table: "OrganizationLocationServiceableCounties",
                column: "BusinessLocationId",
                principalTable: "OrganizationLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
