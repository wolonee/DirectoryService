using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.CreateTable(
                name: "department",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    identifier = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    path = table.Column<string>(type: "ltree", maxLength: 150, nullable: false),
                    depth = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    children_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department", x => x.id);
                    table.ForeignKey(
                        name: "FK_department_department_parent_id",
                        column: x => x.parent_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    timezone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "department_locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_department_locations_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_department_locations_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department_positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_department_positions_department_department_id",
                        column: x => x.department_id,
                        principalTable: "department",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_department_positions_position_position_id",
                        column: x => x.position_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_departments_name",
                table: "department",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_departments_path",
                table: "department",
                column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_parent_id",
                table: "department",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ux_departments_identifier",
                table: "department",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_locations_department_id",
                table: "department_locations",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_locations_location_id",
                table: "department_locations",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ux_department_locations",
                table: "department_locations",
                columns: new[] { "department_id", "location_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_positions_department_id",
                table: "department_positions",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_department_positions_department_id_position_id",
                table: "department_positions",
                columns: new[] { "department_id", "position_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_department_positions_position_id",
                table: "department_positions",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_city",
                table: "locations",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ix_locations_country",
                table: "locations",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "ix_locations_street",
                table: "locations",
                column: "street");

            migrationBuilder.CreateIndex(
                name: "ix_locations_timezone",
                table: "locations",
                column: "timezone");

            migrationBuilder.CreateIndex(
                name: "ux_locations_full_address",
                table: "locations",
                columns: new[] { "country", "street", "city" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_locations_name",
                table: "locations",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "department_locations");

            migrationBuilder.DropTable(
                name: "department_positions");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "department");

            migrationBuilder.DropTable(
                name: "position");
        }
    }
}
