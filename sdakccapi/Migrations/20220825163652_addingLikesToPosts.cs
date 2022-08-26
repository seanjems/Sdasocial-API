using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class addingLikesToPosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "posts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "likes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "PostsId",
                table: "likes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "followers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "comments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CoverPhotoUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_likes_PostsId",
                table: "likes",
                column: "PostsId");

            migrationBuilder.AddForeignKey(
                name: "FK_likes_posts_PostsId",
                table: "likes",
                column: "PostsId",
                principalTable: "posts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_likes_posts_PostsId",
                table: "likes");

            migrationBuilder.DropIndex(
                name: "IX_likes_PostsId",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "PostsId",
                table: "likes");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "followers");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "CoverPhotoUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePicUrl",
                table: "AspNetUsers");
        }
    }
}
