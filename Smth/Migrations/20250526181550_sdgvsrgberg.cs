using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smth.Migrations
{
    /// <inheritdoc />
    public partial class sdgvsrgberg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Participants_ParticipantId",
                table: "Answers");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Participants_ParticipantId",
                table: "Answers",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Participants_ParticipantId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Participants_ParticipantId",
                table: "Answers",
                column: "ParticipantId",
                principalTable: "Participants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
