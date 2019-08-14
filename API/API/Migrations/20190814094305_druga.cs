using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Migrations
{
    public partial class druga : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    firstName = table.Column<string>(nullable: true),
                    lastName = table.Column<string>(nullable: true),
                    username = table.Column<string>(nullable: true),
                    password = table.Column<string>(nullable: true),
                    email = table.Column<string>(nullable: true),
                    token = table.Column<string>(nullable: true),
                    image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    post = table.Column<string>(nullable: true),
                    date = table.Column<DateTime>(nullable: false),
                    userIdid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Posts_Users_userIdid",
                        column: x => x.userIdid,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    postid = table.Column<int>(nullable: true),
                    userid = table.Column<int>(nullable: true),
                    text = table.Column<string>(nullable: true),
                    date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_postid",
                        column: x => x.postid,
                        principalTable: "Posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Users_userid",
                        column: x => x.userid,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    postid = table.Column<int>(nullable: true),
                    userid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Likes_Posts_postid",
                        column: x => x.postid,
                        principalTable: "Posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Likes_Users_userid",
                        column: x => x.userid,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_postid",
                table: "Comments",
                column: "postid");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_userid",
                table: "Comments",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_postid",
                table: "Likes",
                column: "postid");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_userid",
                table: "Likes",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_userIdid",
                table: "Posts",
                column: "userIdid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
