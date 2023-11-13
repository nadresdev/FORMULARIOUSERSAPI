using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngularAuthYtAPI.Migrations
{
    public partial class entidadesPersonaUserUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Personas",
                newName: "FechaCreacion");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Personas",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Personas");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Personas",
                newName: "Date");
        }
    }
}
