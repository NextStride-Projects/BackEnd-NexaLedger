using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmpresaModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Nombre",
                schema: "public",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "StaffCount",
                schema: "public",
                table: "Empresas");

            migrationBuilder.RenameColumn(
                name: "Telefono",
                schema: "public",
                table: "Empresas",
                newName: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                schema: "public",
                table: "Empresas",
                newName: "Telefono");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                schema: "public",
                table: "Empresas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
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
    }
}
