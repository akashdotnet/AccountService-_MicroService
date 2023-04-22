using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Create_Suggested_Dealer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audit_CustomerSuggestedDealers",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DealerName = table.Column<string>(type: "text", nullable: false),
                    DealerAddress = table.Column<string>(type: "text", nullable: false),
                    DealerEmail = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    AuditAction = table.Column<string>(type: "text", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_CustomerSuggestedDealers", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSuggestedDealers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    DealerName = table.Column<string>(type: "text", nullable: false),
                    DealerAddress = table.Column<string>(type: "text", nullable: false),
                    DealerEmail = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSuggestedDealers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSuggestedDealers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSuggestedDealers_CustomerId",
                table: "CustomerSuggestedDealers",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit_CustomerSuggestedDealers");

            migrationBuilder.DropTable(
                name: "CustomerSuggestedDealers");
        }
    }
}
