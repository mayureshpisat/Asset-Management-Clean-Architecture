using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Temperature = table.Column<int>(type: "int", nullable: false),
                    Power = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetInfos_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetInfos_AssetId",
                table: "AssetInfos",
                column: "AssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetInfos");
        }
    }
}
