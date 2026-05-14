using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bokado.Server.Migrations
{
    /// <inheritdoc />
    public partial class Databaseupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "swipes");

            migrationBuilder.CreateTable(
                name: "friend_requests",
                columns: table => new
                {
                    FriendRequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ToUserId = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_friend_requests", x => x.FriendRequestId);
                    table.ForeignKey(
                        name: "FK_friend_requests_users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_friend_requests_users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_posts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_likes",
                columns: table => new
                {
                    PostLikeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_likes", x => x.PostLikeId);
                    table.ForeignKey(
                        name: "FK_post_likes_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "interests",
                columns: new[] { "InterestId", "Name" },
                values: new object[,]
                {
                    { 1, "Ігри" },
                    { 2, "Музика" },
                    { 3, "Кіно" },
                    { 4, "Спорт" },
                    { 5, "Подорожі" },
                    { 6, "Кулінарія" },
                    { 7, "Мистецтво" },
                    { 8, "Технології" },
                    { 9, "Книги" },
                    { 10, "Фітнес" },
                    { 11, "Фотографія" },
                    { 12, "Природа" }
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "BirthDate",
                value: new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.CreateIndex(
                name: "IX_friend_requests_FromUserId_ToUserId",
                table: "friend_requests",
                columns: new[] { "FromUserId", "ToUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_friend_requests_ToUserId",
                table: "friend_requests",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_post_likes_PostId_UserId",
                table: "post_likes",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_likes_UserId",
                table: "post_likes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_UserId",
                table: "posts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "friend_requests");

            migrationBuilder.DropTable(
                name: "post_likes");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "interests",
                keyColumn: "InterestId",
                keyValue: 12);

            migrationBuilder.CreateTable(
                name: "swipes",
                columns: table => new
                {
                    SwipeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    SwiperId = table.Column<int>(type: "integer", nullable: false),
                    TargetUserId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    IsMatch = table.Column<bool>(type: "boolean", nullable: false),
                    SwipedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_swipes", x => x.SwipeId);
                    table.ForeignKey(
                        name: "FK_swipes_users_SwiperId",
                        column: x => x.SwiperId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_swipes_users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "BirthDate",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_swipes_SwiperId",
                table: "swipes",
                column: "SwiperId");

            migrationBuilder.CreateIndex(
                name: "IX_swipes_TargetUserId",
                table: "swipes",
                column: "TargetUserId");
        }
    }
}
