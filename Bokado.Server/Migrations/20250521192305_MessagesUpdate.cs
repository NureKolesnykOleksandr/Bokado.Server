using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class MessagesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGif",
                table: "messages");

            migrationBuilder.RenameColumn(
                name: "AttachmentUrl",
                table: "messages",
                newName: "Attachment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Attachment",
                table: "messages",
                newName: "AttachmentUrl");

            migrationBuilder.AddColumn<bool>(
                name: "IsGif",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
