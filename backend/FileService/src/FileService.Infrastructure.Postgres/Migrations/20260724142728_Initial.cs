using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.CreateTable(
                name: "media_asset",
                schema: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_type = table.Column<string>(type: "text", nullable: false),
                    usage = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_key = table.Column<string>(type: "jsonb", nullable: false),
                    final_key = table.Column<string>(type: "jsonb", nullable: false),
                    owner_context = table.Column<string>(type: "text", nullable: false),
                    owner_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_uploader_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    storage_reference_key = table.Column<string>(type: "jsonb", nullable: true),
                    storage_reference_size = table.Column<long>(type: "bigint", nullable: true),
                    storage_reference_content_type = table.Column<string>(type: "text", nullable: true),
                    storage_reference_etag = table.Column<string>(type: "text", nullable: true),
                    storage_reference_checksum = table.Column<string>(type: "text", nullable: true),
                    storage_reference_last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hls_root_key = table.Column<string>(type: "jsonb", nullable: true),
                    media_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_asset", x => x.id);
                });

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
                name: "IX_media_asset_status_created_at",
                schema: "files",
                table: "media_asset",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_media_asset_usage_status",
                schema: "files",
                table: "media_asset",
                columns: new[] { "usage", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_asset",
                schema: "files");
        }
    }
}
