using System.Collections.Frozen;

namespace Nestor.Db.Sqlite.Helpers;

public static class SqliteMigration
{
    public static readonly FrozenDictionary<long, string> Migrations;

    static SqliteMigration()
    {
        Migrations = new Dictionary<long, string>
        {
            {
                202601031118,
                @"
CREATE TABLE IF NOT EXISTS MigrationEntity (
    Id INTEGER PRIMARY KEY NOT NULL,
    Sql TEXT NOT NULL
);
"
            },
            {
                202601031119,
                @"
CREATE TABLE IF NOT EXISTS EventEntity (
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    EntityId TEXT NOT NULL,
    EntityType TEXT NOT NULL CHECK(length(EntityType) <= 255),
    EntityProperty TEXT NOT NULL CHECK(length(EntityProperty) <= 255),
    UserId TEXT NOT NULL CHECK(length(UserId) <= 255),
    CreatedAt TEXT NOT NULL,
    EntityBooleanValue INTEGER CHECK (EntityBooleanValue IN (0, 1)),
    EntityByteValue INTEGER,
    EntityUInt16Value INTEGER,
    EntityUInt32Value INTEGER,
    EntityUInt64Value INTEGER,
    EntitySByteValue INTEGER,
    EntityInt16Value INTEGER,
    EntityInt32Value INTEGER,
    EntityInt64Value INTEGER,
    EntitySingleValue REAL,
    EntityDoubleValue REAL,
    EntityDecimalValue TEXT,
    EntityCharValue TEXT CHECK(length(EntityCharValue) <= 1),
    EntityByteArrayValue BLOB,
    EntityStringValue TEXT,
    EntityGuidValue TEXT,
    EntityDateTimeValue TEXT,
    EntityDateTimeOffsetValue TEXT,
    EntityDateOnlyValue TEXT,
    EntityTimeOnlyValue TEXT,
    EntityTimeSpanValue TEXT,
    IsLast INTEGER NOT NULL CHECK (IsLast IN (0, 1))
);
"
            },
        }.ToFrozenDictionary();
    }
}
