using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class moreUpdatesToHubsLogic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversations_conversationMembers_MembersUserId_MembersConversationId",
                table: "conversations");

            migrationBuilder.DropIndex(
                name: "IX_conversations_MembersUserId_MembersConversationId",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "MembersConversationId",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "MembersUserId",
                table: "conversations");

            migrationBuilder.AddColumn<long>(
                name: "ConversationsId",
                table: "conversationMembers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "activeUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_conversationMembers_ConversationsId",
                table: "conversationMembers",
                column: "ConversationsId");

            migrationBuilder.CreateIndex(
                name: "IX_activeUsers_UserId",
                table: "activeUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_activeUsers_AspNetUsers_UserId",
                table: "activeUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversationMembers_AspNetUsers_UserId",
                table: "conversationMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversationMembers_conversations_ConversationsId",
                table: "conversationMembers",
                column: "ConversationsId",
                principalTable: "conversations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activeUsers_AspNetUsers_UserId",
                table: "activeUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_conversationMembers_AspNetUsers_UserId",
                table: "conversationMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_conversationMembers_conversations_ConversationsId",
                table: "conversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_conversationMembers_ConversationsId",
                table: "conversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_activeUsers_UserId",
                table: "activeUsers");

            migrationBuilder.DropColumn(
                name: "ConversationsId",
                table: "conversationMembers");

            migrationBuilder.AddColumn<long>(
                name: "MembersConversationId",
                table: "conversations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "MembersUserId",
                table: "conversations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "activeUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_MembersUserId_MembersConversationId",
                table: "conversations",
                columns: new[] { "MembersUserId", "MembersConversationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_conversations_conversationMembers_MembersUserId_MembersConversationId",
                table: "conversations",
                columns: new[] { "MembersUserId", "MembersConversationId" },
                principalTable: "conversationMembers",
                principalColumns: new[] { "UserId", "ConversationId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
