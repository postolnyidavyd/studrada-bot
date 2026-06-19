using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace studrada_bot.Migrations
{
    /// <inheritdoc />
    public partial class FixPostTargetDuplicateFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostTargets_Posts_PostId1",
                table: "PostTargets");

            migrationBuilder.DropIndex(
                name: "IX_PostTargets_PostId1",
                table: "PostTargets");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "PostTargets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "PostTargets",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostTargets_PostId1",
                table: "PostTargets",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTargets_Posts_PostId1",
                table: "PostTargets",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");
        }
    }
}
