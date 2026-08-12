using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class RenameVideoProcessingRetryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RetryCount",
                schema: "files",
                table: "video_processing",
                newName: "retry_count");

            migrationBuilder.RenameColumn(
                name: "NextRetryAt",
                schema: "files",
                table: "video_processing",
                newName: "next_retry_at");

            migrationBuilder.RenameColumn(
                name: "MaxRetries",
                schema: "files",
                table: "video_processing",
                newName: "max_retries");

            migrationBuilder.RenameColumn(
                name: "IsCriticalError",
                schema: "files",
                table: "video_processing",
                newName: "is_critical_error");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "retry_count",
                schema: "files",
                table: "video_processing",
                newName: "RetryCount");

            migrationBuilder.RenameColumn(
                name: "next_retry_at",
                schema: "files",
                table: "video_processing",
                newName: "NextRetryAt");

            migrationBuilder.RenameColumn(
                name: "max_retries",
                schema: "files",
                table: "video_processing",
                newName: "MaxRetries");

            migrationBuilder.RenameColumn(
                name: "is_critical_error",
                schema: "files",
                table: "video_processing",
                newName: "IsCriticalError");
        }
    }
}
