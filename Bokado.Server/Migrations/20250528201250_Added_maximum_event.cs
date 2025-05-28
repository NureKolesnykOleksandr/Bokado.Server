using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class Added_maximum_event : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Maximum",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Maximum",
                table: "events");
        }
    }
}
