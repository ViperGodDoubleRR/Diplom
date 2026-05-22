using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessionTable_Users_UserId",
                table: "UserSessionTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessionTable",
                table: "UserSessionTable");

            migrationBuilder.RenameTable(
                name: "UserSessionTable",
                newName: "UserSessions");

            migrationBuilder.RenameColumn(
                name: "IpAdress",
                table: "UserSessions",
                newName: "IpAddress");

            migrationBuilder.RenameIndex(
                name: "IX_UserSessionTable_UserId",
                table: "UserSessions",
                newName: "IX_UserSessions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Users_UserId",
                table: "UserSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Users_UserId",
                table: "UserSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSessions",
                table: "UserSessions");

            migrationBuilder.RenameTable(
                name: "UserSessions",
                newName: "UserSessionTable");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "UserSessionTable",
                newName: "IpAdress");

            migrationBuilder.RenameIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessionTable",
                newName: "IX_UserSessionTable_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSessionTable",
                table: "UserSessionTable",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessionTable_Users_UserId",
                table: "UserSessionTable",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
