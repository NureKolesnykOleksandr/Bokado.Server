using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChallengeUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE challenges ALTER COLUMN ""Reward"" TYPE integer USING (""Reward""::integer)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "challenges",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "challenges");

            migrationBuilder.AlterColumn<string>(
                name: "Reward",
                table: "challenges",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
