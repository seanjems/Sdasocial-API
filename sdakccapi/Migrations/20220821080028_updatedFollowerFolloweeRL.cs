using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class updatedFollowerFolloweeRL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_followers_AspNetUsers_AppUserId",
                table: "followers");

            migrationBuilder.DropForeignKey(
                name: "FK_likes_AspNetUsers_UserId",
                table: "likes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_followers",
                table: "followers");

            migrationBuilder.DropIndex(
                name: "IX_followers_AppUserId",
                table: "followers");

            migrationBuilder.DropColumn(
                name: "FollowerId",
                table: "followers");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "followers");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "followers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "FollowedById",
                table: "followers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_followers",
                table: "followers",
                columns: new[] { "UserId", "FollowedById" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_followers",
                table: "followers");

            migrationBuilder.DropColumn(
                name: "FollowedById",
                table: "followers");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "followers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "FollowerId",
                table: "followers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "followers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_followers",
                table: "followers",
                columns: new[] { "UserId", "FollowerId" });

            migrationBuilder.CreateIndex(
                name: "IX_followers_AppUserId",
                table: "followers",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_followers_AspNetUsers_AppUserId",
                table: "followers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_likes_AspNetUsers_UserId",
                table: "likes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
