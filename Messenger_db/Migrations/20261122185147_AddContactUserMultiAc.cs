using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Messenger_db.Migrations
{
    public partial class AddContactUserMultiAc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    ReceiverUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequests", x => x.Id);
                    table.ForeignKey(name: "FK_ContactRequests_Users_ReceiverUserId",
                          column: x => x.ReceiverUserId,
                          principalTable: "Users",
                          principalColumn: "Id",
                          onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_ContactRequests_Users_SenderUserId", column: x => x.SenderUserId,   principalTable: "Users", principalColumn: "Id",onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_ContactRequests_ReceiverUserId", table: "ContactRequests", column: "ReceiverUserId");

            migrationBuilder.CreateIndex(name: "IX_ContactRequests_SenderUserId_ReceiverUserId", table: "ContactRequests", columns: new[] { "SenderUserId", "ReceiverUserId" }, unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ContactRequests");
        }
    }
}
