using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecorderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessedFieldsToLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns
            migrationBuilder.AddColumn<int>(
                name: "AccessedEmpresaId",
                table: "Logs",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccessedUsuarioId",
                table: "Logs",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "Logs",
                maxLength: 500,
                nullable: true);

            // Modify EmpresaId column type with USING clause
            migrationBuilder.Sql(
                "ALTER TABLE \"Logs\" ALTER COLUMN \"EmpresaId\" TYPE integer USING \"EmpresaId\"::integer;"
            );

            // If you have other columns with similar issues, handle them here
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the column type change
            migrationBuilder.Sql(
                "ALTER TABLE \"Logs\" ALTER COLUMN \"EmpresaId\" TYPE text USING \"EmpresaId\"::text;"
            );

            // Drop the columns added
            migrationBuilder.DropColumn(
                name: "AccessedEmpresaId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "AccessedUsuarioId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "Logs");
        }
    }

}
