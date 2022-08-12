using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class removedFollowersPK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "followers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "followers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
