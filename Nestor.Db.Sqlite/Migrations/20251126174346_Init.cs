using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nestor.Db.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventEntity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(
                        type: "TEXT",
                        maxLength: 255,
                        nullable: false
                    ),
                    EntityProperty = table.Column<string>(
                        type: "TEXT",
                        maxLength: 255,
                        nullable: false
                    ),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EntityBooleanValue = table.Column<bool>(type: "INTEGER", nullable: true),
                    EntityByteValue = table.Column<byte>(type: "INTEGER", nullable: true),
                    EntityUInt16Value = table.Column<ushort>(type: "INTEGER", nullable: true),
                    EntityUInt32Value = table.Column<uint>(type: "INTEGER", nullable: true),
                    EntityUInt64Value = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EntitySByteValue = table.Column<sbyte>(type: "INTEGER", nullable: true),
                    EntityInt16Value = table.Column<short>(type: "INTEGER", nullable: true),
                    EntityInt32Value = table.Column<int>(type: "INTEGER", nullable: true),
                    EntityInt64Value = table.Column<long>(type: "INTEGER", nullable: true),
                    EntitySingleValue = table.Column<float>(type: "REAL", nullable: true),
                    EntityDoubleValue = table.Column<double>(type: "REAL", nullable: true),
                    EntityDecimalValue = table.Column<decimal>(type: "TEXT", nullable: true),
                    EntityCharValue = table.Column<char>(type: "TEXT", nullable: true),
                    EntityByteArrayValue = table.Column<byte[]>(type: "BLOB", nullable: true),
                    EntityStringValue = table.Column<string>(type: "TEXT", nullable: true),
                    EntityGuidValue = table.Column<Guid>(type: "TEXT", nullable: true),
                    EntityDateTimeValue = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EntityDateTimeOffsetValue = table.Column<DateTimeOffset>(
                        type: "TEXT",
                        nullable: true
                    ),
                    EntityDateOnlyValue = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EntityTimeOnlyValue = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    EntityTimeSpanValue = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventEntity", x => x.Id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EventEntity");
        }
    }
}
