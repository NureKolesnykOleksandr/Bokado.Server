using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class GroupUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "groups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "group_members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "challenges",
                columns: new[] { "ChallengeId", "CreatedAt", "Description", "IsActive", "Reward", "Title" },
                values: new object[,]
                {
                    { 11, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Опублікуй свій перший пост", false, 1, "Перший пост" },
                    { 12, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Отримай 5 лайків на один пост", false, 2, "Популярний контент" },
                    { 13, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Вступи в свою першу групу", false, 1, "Груповий учасник" },
                    { 14, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Створи власну групу", false, 3, "Лідер спільноти" },
                    { 15, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Надішли фото у повідомленні", false, 1, "Фотограф" },
                    { 16, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Запусти відеодзвінок у групі", false, 2, "Відеоорганізатор" },
                    { 17, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Отримай 3 запити на дружбу", false, 2, "Магніт" },
                    { 18, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Набери 10 друзів", false, 3, "Відданий друг" },
                    { 19, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Постав 10 лайків на пости інших", false, 1, "Активний лайкер" },
                    { 20, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Вступи в 3 різні групи", false, 3, "Командний гравець" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "City",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "group_members");
        }
    }
}
