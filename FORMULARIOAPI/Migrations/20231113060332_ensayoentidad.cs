using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngularAuthYtAPI.Migrations
{
    public partial class ensayoentidad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "Apellidos");

            migrationBuilder.AddColumn<long>(
                name: "NroIdentificacion",
                table: "users",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NroIdentificacion",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Apellidos",
                table: "users",
                newName: "FirstName");
        }
    }
}
