using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Replace_ZipCodes_With_Counties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit_OrganizationLocationServiceableZipCodes");

            migrationBuilder.DropTable(
                name: "OrganizationLocationServiceableZipCodes");

            migrationBuilder.CreateTable(
                name: "Audit_OrganizationLocationServiceableCounties",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrganizationLocationId = table.Column<int>(type: "integer", nullable: true),
                    ServiceableCounty = table.Column<string>(type: "text", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_OrganizationLocationServiceableCounties", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationLocationServiceableCounties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationLocationId = table.Column<int>(type: "integer", nullable: false),
                    ServiceableCounty = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLocationServiceableCounties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationLocationServiceableCounties_OrganizationLocatio~",
                        column: x => x.OrganizationLocationId,
                        principalTable: "OrganizationLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLocationServiceableCounties_OrganizationLocatio~",
                table: "OrganizationLocationServiceableCounties",
                column: "OrganizationLocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit_OrganizationLocationServiceableCounties");

            migrationBuilder.DropTable(
                name: "OrganizationLocationServiceableCounties");

            migrationBuilder.CreateTable(
                name: "Audit_OrganizationLocationServiceableZipCodes",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrganizationLocationId = table.Column<int>(type: "integer", nullable: true),
                    ServiceableZipCode = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_OrganizationLocationServiceableZipCodes", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationLocationServiceableZipCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationLocationId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceableZipCode = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationLocationServiceableZipCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationLocationServiceableZipCodes_OrganizationLocatio~",
                        column: x => x.OrganizationLocationId,
                        principalTable: "OrganizationLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationLocationServiceableZipCodes_OrganizationLocatio~",
                table: "OrganizationLocationServiceableZipCodes",
                column: "OrganizationLocationId");
        }
    }
}
