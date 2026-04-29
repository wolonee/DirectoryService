using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_custom_indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE INDEX ix_locations_active_name 
                ON locations (is_active, name);");
            
            migrationBuilder.Sql(
                @"CREATE INDEX ix_position_active_speciality 
                ON position (is_active, (name ->> 'speciality'));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS ix_locations_active_name;");
            
            migrationBuilder.Sql(
                @"DROP INDEX IF EXISTS ix_position_active_speciality;");
        }
    }
}
