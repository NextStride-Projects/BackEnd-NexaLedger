using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDirectResourceReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movements_Resources_ResourceId",
                table: "Movements");

            migrationBuilder.DropIndex(
                name: "IX_Movements_ResourceId",
                table: "Movements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Movements_ResourceId",
                table: "Movements",
                column: "ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movements_Resources_ResourceId",
                table: "Movements",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
