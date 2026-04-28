using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_ltree_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_department_ParentId",
                table: "department");

            migrationBuilder.DropIndex(
                name: "ix_departments_path",
                table: "department");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "department",
                newName: "parent_id");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "department",
                type: "ltree",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "ix_departments_path",
                table: "department",
                column: "path")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.AddForeignKey(
                name: "FK_department_department_parent_id",
                table: "department",
                column: "parent_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_department_parent_id",
                table: "department");

            migrationBuilder.DropIndex(
                name: "ix_departments_path",
                table: "department");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "department",
                newName: "ParentId");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "department",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "ltree",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "ix_departments_path",
                table: "department",
                column: "path");

            migrationBuilder.AddForeignKey(
                name: "FK_department_department_ParentId",
                table: "department",
                column: "ParentId",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
