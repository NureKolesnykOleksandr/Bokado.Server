using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddedChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "challenges",
                columns: new[] { "ChallengeId", "CreatedAt", "Description", "IsActive", "Reward", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Почни спілкуватися з 3 різними людьми", false, 2, "Соціальний старт" },
                    { 2, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Відправ 10 повідомлень у чатах", false, 2, "Активний учасник" },
                    { 3, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Прийми участь у будь-якій події", false, 1, "Знайомство з подією" },
                    { 4, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Додай 5 нових людей у друзі", false, 2, "Дружелюбний крок" },
                    { 5, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Підтримуй бесіду в 3 різних чатах", false, 1, "Чат-ентузіаст" },
                    { 6, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Створи власну подію", false, 3, "Організатор" },
                    { 7, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Знайди 7 людей зі спільними інтересами", false, 2, "Дослідник" },
                    { 8, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Відправ хоча б одне голосове повідомлення", false, 1, "Віртуальний зустріч" },
                    { 9, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Відвідай 2 різні події за тиждень", false, 3, "Соціальний активіст" },
                    { 10, new DateTime(2025, 5, 21, 20, 30, 0, 0, DateTimeKind.Utc), "Заповни всі поля свого профілю на 100%", false, 1, "Профільний експерт" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "challenges",
                keyColumn: "ChallengeId",
                keyValue: 10);
        }
    }
}
