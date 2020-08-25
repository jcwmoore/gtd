using Microsoft.EntityFrameworkCore.Migrations;

namespace Gtd.Web.Data.Migrations
{
    public partial class sortabletasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Sort",
                table: "Tasks",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sort",
                table: "Tasks");
        }
    }
}
