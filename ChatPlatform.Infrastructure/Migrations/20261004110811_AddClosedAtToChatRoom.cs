using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClosedAtToChatRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "ChatRooms",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "ChatRooms");
        }
    }
}
