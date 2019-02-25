using Microsoft.EntityFrameworkCore.Migrations;

namespace Milka.Migrations
{
    public partial class SubscriptionsUniqueEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AlternateKey_Unique_Email",
                table: "Subscriber",
                column: "Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AlternateKey_Unique_Email",
                table: "Subscriber");
        }
    }
}
