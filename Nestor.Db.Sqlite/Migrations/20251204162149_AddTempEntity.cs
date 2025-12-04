using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nestor.Db.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddTempEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TempEntity",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempEntity", x => x.EntityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempEntity");
        }
    }
}
