using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserSessionChange23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessionTable_Users_TokenUserId",
                table: "UserSessionTable");

            migrationBuilder.DropIndex(
                name: "IX_UserSessionTable_TokenUserId",
                table: "UserSessionTable");

            migrationBuilder.RenameColumn(
                name: "TokenUserId",
                table: "UserSessionTable",
                newName: "UserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "UserSessionTable",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionTable_UserId",
                table: "UserSessionTable",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessionTable_Users_UserId",
                table: "UserSessionTable",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessionTable_Users_UserId",
                table: "UserSessionTable");

            migrationBuilder.DropIndex(
                name: "IX_UserSessionTable_UserId",
                table: "UserSessionTable");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserSessionTable",
                newName: "TokenUserId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "UserSessionTable",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessionTable_TokenUserId",
                table: "UserSessionTable",
                column: "TokenUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessionTable_Users_TokenUserId",
                table: "UserSessionTable",
                column: "TokenUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
