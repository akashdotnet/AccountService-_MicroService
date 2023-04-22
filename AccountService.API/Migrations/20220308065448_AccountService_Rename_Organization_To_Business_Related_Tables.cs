using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Rename_Organizatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Organizations_OrganizationId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationBrands_Organizations_OrganizationId",
                table: "OrganizationBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLocations_Organizations_OrganizationId",
                table: "OrganizationLocations");

            migrationBuilder.DropTable(
                name: "Audit_OrganizationBrands");

            migrationBuilder.DropTable(
                name: "Audit_OrganizationLocations");

            migrationBuilder.DropTable(
                name: "Audit_OrganizationLocationServiceableCounties");

            migrationBuilder.DropTable(
                name: "Audit_Organizations");

            migrationBuilder.RenameColumn(
                name: "OrganizationLocationId",
                table: "OrganizationLocationServiceableCounties",
                newName: "BusinessLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocationServiceableCounties_OrganizationLocatio~",
                table: "OrganizationLocationServiceableCounties",
                newName: "IX_OrganizationLocationServiceableCounties_BusinessLocationId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "OrganizationLocations",
                newName: "BusinessId");

            migrationBuilder.RenameColumn(
                name: "LocationName",
                table: "OrganizationLocations",
                newName: "OfficeName");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocations_OrganizationId",
                table: "OrganizationLocations",
                newName: "IX_OrganizationLocations_BusinessId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "OrganizationBrands",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationBrands_OrganizationId",
                table: "OrganizationBrands",
                newName: "IX_OrganizationBrands_BusinessId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Dealers",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_Dealers_OrganizationId",
                table: "Dealers",
                newName: "IX_Dealers_BusinessId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Audit_Dealers",
                newName: "BusinessId");

            migrationBuilder.CreateTable(
                name: "Audit_BusinessBrands",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BusinessId = table.Column<int>(type: "integer", nullable: true),
                    BrandCode = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_BusinessBrands", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_Businesses",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    LogoBlobId = table.Column<int>(type: "integer", nullable: true),
                    About = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_Businesses", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_BusinessLocations",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BusinessId = table.Column<int>(type: "integer", nullable: true),
                    AddressId = table.Column<int>(type: "integer", nullable: true),
                    OfficeName = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_BusinessLocations", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_BusinessLocationServiceableCounties",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    BusinessLocationId = table.Column<int>(type: "integer", nullable: true),
                    ServiceableCounty = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_BusinessLocationServiceableCounties", x => x.AuditId);
                });

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
                name: "FK_OrganizationLocations_Organizations_BusinessId",
                table: "OrganizationLocations",
                column: "BusinessId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Organizations_BusinessId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationBrands_Organizations_BusinessId",
                table: "OrganizationBrands");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLocations_Organizations_BusinessId",
                table: "OrganizationLocations");

            migrationBuilder.DropTable(
                name: "Audit_BusinessBrands");

            migrationBuilder.DropTable(
                name: "Audit_Businesses");

            migrationBuilder.DropTable(
                name: "Audit_BusinessLocations");

            migrationBuilder.DropTable(
                name: "Audit_BusinessLocationServiceableCounties");

            migrationBuilder.RenameColumn(
                name: "BusinessLocationId",
                table: "OrganizationLocationServiceableCounties",
                newName: "OrganizationLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocationServiceableCounties_BusinessLocationId",
                table: "OrganizationLocationServiceableCounties",
                newName: "IX_OrganizationLocationServiceableCounties_OrganizationLocatio~");

            migrationBuilder.RenameColumn(
                name: "OfficeName",
                table: "OrganizationLocations",
                newName: "LocationName");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "OrganizationLocations",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationLocations_BusinessId",
                table: "OrganizationLocations",
                newName: "IX_OrganizationLocations_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "OrganizationBrands",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_OrganizationBrands_BusinessId",
                table: "OrganizationBrands",
                newName: "IX_OrganizationBrands_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "Dealers",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Dealers_BusinessId",
                table: "Dealers",
                newName: "IX_Dealers_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "Audit_Dealers",
                newName: "OrganizationId");

            migrationBuilder.CreateTable(
                name: "Audit_OrganizationBrands",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BrandCode = table.Column<string>(type: "text", nullable: true),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_OrganizationBrands", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_OrganizationLocations",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<int>(type: "integer", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LocationName = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_OrganizationLocations", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_OrganizationLocationServiceableCounties",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrganizationLocationId = table.Column<int>(type: "integer", nullable: true),
                    ServiceableCounty = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_OrganizationLocationServiceableCounties", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "Audit_Organizations",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    About = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LogoBlobId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_Organizations", x => x.AuditId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Organizations_OrganizationId",
                table: "Dealers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationBrands_Organizations_OrganizationId",
                table: "OrganizationBrands",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLocations_Organizations_OrganizationId",
                table: "OrganizationLocations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
