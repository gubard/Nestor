using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nestor.Db.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLast",
                table: "EventEntity",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLast",
                table: "EventEntity");
        }
    }
}
