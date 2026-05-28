using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MediaUserChangeMiniO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "MediaUsers",
                newName: "OriginalName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MediaUsers",
                newName: "MediaType");

            migrationBuilder.AddColumn<string>(
                name: "Bucket",
                table: "MediaUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "MediaUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileKey",
                table: "MediaUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "MediaUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bucket",
                table: "MediaUsers");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "MediaUsers");

            migrationBuilder.DropColumn(
                name: "FileKey",
                table: "MediaUsers");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "MediaUsers");

            migrationBuilder.RenameColumn(
                name: "OriginalName",
                table: "MediaUsers",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "MediaType",
                table: "MediaUsers",
                newName: "Name");
        }
    }
}
