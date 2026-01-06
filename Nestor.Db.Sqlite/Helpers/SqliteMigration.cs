using System.Collections.Frozen;

namespace Nestor.Db.Sqlite.Helpers;

public static class SqliteMigration
{
    public static readonly FrozenDictionary<int, string> Migrations;

    static SqliteMigration()
    {
        Migrations = new Dictionary<int, string>
        {
            {
                1,
                @"
CREATE TABLE IF NOT EXISTS Migrations (
    Id INTEGER PRIMARY KEY NOT NULL,
    Sql TEXT NOT NULL
);
"
            },
            {
                2,
                @"
CREATE TABLE IF NOT EXISTS Events (
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
    TransactionId TEXT NOT NULL
);
"
            },
        }.ToFrozenDictionary();
    }
}
