using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPropertiesToEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                schema: "public",
                table: "Empresas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                schema: "public",
                table: "Empresas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "public",
                table: "Empresas",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "Empresas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Features",
                schema: "public",
                table: "Empresas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "Empresas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "public",
                table: "Empresas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleEmail",
                schema: "public",
                table: "Empresas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePerson",
                schema: "public",
                table: "Empresas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StaffCount",
                schema: "public",
                table: "Empresas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Alias",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Features",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ResponsibleEmail",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ResponsiblePerson",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "StaffCount",
                schema: "public",
                table: "Empresas");
        }
    }
}
