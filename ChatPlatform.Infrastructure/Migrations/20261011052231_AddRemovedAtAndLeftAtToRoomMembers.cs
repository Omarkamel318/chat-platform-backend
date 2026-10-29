using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemovedAtAndLeftAtToRoomMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "ChatRoomMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                table: "ChatRoomMembers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "ChatRoomMembers");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "ChatRoomMembers");
        }
    }
}
