using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DSB10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_department_ParentId",
                table: "department");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_department_DepartmentId",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_LocationId",
                table: "department_locations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "department_locations");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "department_locations",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department_locations",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_LocationId",
                table: "department_locations",
                newName: "IX_department_locations_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_DepartmentId",
                table: "department_locations",
                newName: "IX_department_locations_department_id");

            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "department",
                newName: "depth");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "department",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "department",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "department",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "DepartmentPath",
                table: "department",
                newName: "path");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "children_count",
                table: "department",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Identifier_Unique",
                table: "department",
                column: "identifier",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_department_department_ParentId",
                table: "department",
                column: "ParentId",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_department_department_id",
                table: "department_locations",
                column: "department_id",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_department_ParentId",
                table: "department");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_department_department_id",
                table: "department_locations");

            migrationBuilder.DropForeignKey(
                name: "FK_department_locations_locations_location_id",
                table: "department_locations");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Identifier_Unique",
                table: "department");

            migrationBuilder.DropColumn(
                name: "children_count",
                table: "department");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "department_locations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "department_locations",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations",
                newName: "IX_department_locations_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_department_locations_department_id",
                table: "department_locations",
                newName: "IX_department_locations_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "depth",
                table: "department",
                newName: "Depth");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "department",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "department",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "department",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "path",
                table: "department",
                newName: "DepartmentPath");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "department_locations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_department_department_ParentId",
                table: "department",
                column: "ParentId",
                principalTable: "department",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_department_DepartmentId",
                table: "department_locations",
                column: "DepartmentId",
                principalTable: "department",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_department_locations_locations_LocationId",
                table: "department_locations",
                column: "LocationId",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
