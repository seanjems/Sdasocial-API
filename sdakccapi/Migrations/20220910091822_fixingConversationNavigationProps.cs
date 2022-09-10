using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class fixingConversationNavigationProps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversationMembers_conversations_ConversationsId",
                table: "conversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_conversationMembers_ConversationsId",
                table: "conversationMembers");

            migrationBuilder.DropColumn(
                name: "ConversationsId",
                table: "conversationMembers");

            migrationBuilder.CreateIndex(
                name: "IX_conversationMembers_ConversationId",
                table: "conversationMembers",
                column: "ConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversationMembers_conversations_ConversationId",
                table: "conversationMembers",
                column: "ConversationId",
                principalTable: "conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversationMembers_conversations_ConversationId",
                table: "conversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_conversationMembers_ConversationId",
                table: "conversationMembers");

            migrationBuilder.AddColumn<long>(
                name: "ConversationsId",
                table: "conversationMembers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversationMembers_ConversationsId",
                table: "conversationMembers",
                column: "ConversationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_conversationMembers_conversations_ConversationsId",
                table: "conversationMembers",
                column: "ConversationsId",
                principalTable: "conversations",
                principalColumn: "Id");
        }
    }
}
