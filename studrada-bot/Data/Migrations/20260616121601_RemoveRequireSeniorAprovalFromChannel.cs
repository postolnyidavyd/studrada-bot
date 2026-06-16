using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace studrada_bot.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequireSeniorAprovalFromChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireSeniorApproval",
                table: "Channels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireSeniorApproval",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
