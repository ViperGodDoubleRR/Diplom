using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueCommentMediaPerComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentMedia_CommentId",
                table: "CommentMedia");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMedia_CommentId",
                table: "CommentMedia",
                column: "CommentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentMedia_CommentId",
                table: "CommentMedia");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMedia_CommentId",
                table: "CommentMedia",
                column: "CommentId");
        }
    }
}
