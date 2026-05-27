using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "users",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "users");
        }
    }
}
