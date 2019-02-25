using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Milka.Migrations
{
    public partial class dates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Post",
                newName: "TimeStamp");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Post",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Edited",
                table: "Post",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "Edited",
                table: "Post");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "Post",
                newName: "Date");
        }
    }
}
