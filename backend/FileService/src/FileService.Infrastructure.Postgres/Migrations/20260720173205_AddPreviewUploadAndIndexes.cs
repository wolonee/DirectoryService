using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewUploadAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_media_asset_final_key",
                schema: "files",
                table: "media_asset",
                column: "final_key");

            migrationBuilder.CreateIndex(
                name: "IX_media_asset_hls_root_key",
                schema: "files",
                table: "media_asset",
                column: "hls_root_key");

            migrationBuilder.CreateIndex(
                name: "IX_media_asset_owner_context_owner_entity_id",
                schema: "files",
                table: "media_asset",
                columns: new[] { "owner_context", "owner_entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_media_asset_raw_key",
                schema: "files",
                table: "media_asset",
                column: "raw_key");

            migrationBuilder.CreateIndex(
                name: "IX_media_asset_usage_status",
                schema: "files",
                table: "media_asset",
                columns: new[] { "usage", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_media_asset_final_key",
                schema: "files",
                table: "media_asset");

            migrationBuilder.DropIndex(
                name: "IX_media_asset_hls_root_key",
                schema: "files",
                table: "media_asset");

            migrationBuilder.DropIndex(
                name: "IX_media_asset_owner_context_owner_entity_id",
                schema: "files",
                table: "media_asset");

            migrationBuilder.DropIndex(
                name: "IX_media_asset_raw_key",
                schema: "files",
                table: "media_asset");

            migrationBuilder.DropIndex(
                name: "IX_media_asset_usage_status",
                schema: "files",
                table: "media_asset");
        }
    }
}
