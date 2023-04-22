using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_Change_SkillLanguageBrand_Schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Skill",
                table: "ExpertSkills",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "ExpertLanguages",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "JobCategoryCode",
                table: "BusinessJobCategories",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "BrandCode",
                table: "BusinessBrands",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Skill",
                table: "Audit_ExpertSkills",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Audit_ExpertLanguages",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "JobCategoryCode",
                table: "Audit_BusinessJobCategories",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "BrandCode",
                table: "Audit_BusinessBrands",
                newName: "Code");

            migrationBuilder.AddColumn<string>(
                name: "Others",
                table: "ExpertSkills",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Others",
                table: "ExpertLanguages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Others",
                table: "BusinessBrands",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Others",
                table: "ExpertSkills");

            migrationBuilder.DropColumn(
                name: "Others",
                table: "ExpertLanguages");

            migrationBuilder.DropColumn(
                name: "Others",
                table: "BusinessBrands");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "ExpertSkills",
                newName: "Skill");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "ExpertLanguages",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "BusinessJobCategories",
                newName: "JobCategoryCode");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "BusinessBrands",
                newName: "BrandCode");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Audit_ExpertSkills",
                newName: "Skill");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Audit_ExpertLanguages",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Audit_BusinessJobCategories",
                newName: "JobCategoryCode");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Audit_BusinessBrands",
                newName: "BrandCode");
        }
    }
}
