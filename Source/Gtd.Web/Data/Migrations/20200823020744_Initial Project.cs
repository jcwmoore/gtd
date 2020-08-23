using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gtd.Web.Data.Migrations
{
    public partial class InitialProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Tasks",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    CompletionStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");

            if(migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {

                migrationBuilder.AddForeignKey(
                    name: "FK_Tasks_Projects_ProjectId",
                    table: "Tasks",
                    column: "ProjectId",
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            }
            else
            {
                migrationBuilder.Sql("DROP TABLE [Tasks]");
                migrationBuilder.Sql(@"
CREATE TABLE [Tasks] (
    Id                  TEXT CONSTRAINT PK_Tasks PRIMARY KEY NOT NULL,
    Created             TEXT NOT NULL,
    Updated             TEXT NOT NULL,
    Title               TEXT NOT NULL,
    Description         TEXT,
    CompletionStatus    INTEGER NOT NULL,
    Important           INTEGER NOT NULL,
    Urgent              INTEGER NOT NULL,
    DueDate             TEXT,
    UserId              TEXT CONSTRAINT FK_Tasks_AspNetUsers_UserId REFERENCES AspNetUsers(Id) ON DELETE CASCADE NOT NULL,
    ProjectId           TEXT CONSTRAINT FK_Tasks_Projects_ProjectId REFERENCES Projects(Id) ON DELETE CASCADE
);");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if(migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_Tasks_Projects_ProjectId",
                    table: "Tasks");

                migrationBuilder.DropIndex(
                    name: "IX_Tasks_ProjectId",
                    table: "Tasks");

                migrationBuilder.DropColumn(
                    name: "ProjectId",
                    table: "Tasks");
            }
            else
            {
                migrationBuilder.Sql("DROP TABLE [Tasks]");
                migrationBuilder.Sql(@"
CREATE TABLE [Tasks] (
    Id                  TEXT CONSTRAINT PK_Tasks PRIMARY KEY NOT NULL,
    Created             TEXT NOT NULL,
    Updated             TEXT NOT NULL,
    Title               TEXT NOT NULL,
    Description         TEXT,
    CompletionStatus    INTEGER NOT NULL,
    Important           INTEGER NOT NULL,
    Urgent              INTEGER NOT NULL,
    DueDate             TEXT,
    UserId              TEXT CONSTRAINT FK_Tasks_AspNetUsers_UserId REFERENCES AspNetUsers(Id) ON DELETE CASCADE NOT NULL
);");
            }
            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
