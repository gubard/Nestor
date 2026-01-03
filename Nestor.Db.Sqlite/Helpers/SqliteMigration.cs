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
create table MigrationEntity
(
    Id                        INTEGER           not null
        constraint PK_MigrationEntity,
    Sql         TEXT
)
"
            },
            {
                202601031119,
                @"
create table EventEntity
(
    Id                        INTEGER           not null
        constraint PK_EventEntity
            primary key autoincrement,
    EntityId                  TEXT              not null,
    EntityType                TEXT              not null,
    EntityProperty            TEXT              not null,
    UserId                    TEXT              not null,
    CreatedAt                 TEXT              not null,
    EntityBooleanValue        INTEGER,
    EntityByteValue           INTEGER,
    EntityUInt16Value         INTEGER,
    EntityUInt32Value         INTEGER,
    EntityUInt64Value         INTEGER,
    EntitySByteValue          INTEGER,
    EntityInt16Value          INTEGER,
    EntityInt32Value          INTEGER,
    EntityInt64Value          INTEGER,
    EntitySingleValue         REAL,
    EntityDoubleValue         REAL,
    EntityDecimalValue        TEXT,
    EntityCharValue           TEXT,
    EntityByteArrayValue      BLOB,
    EntityStringValue         TEXT,
    EntityGuidValue           TEXT,
    EntityDateTimeValue       TEXT,
    EntityDateTimeOffsetValue TEXT,
    EntityDateOnlyValue       TEXT,
    EntityTimeOnlyValue       TEXT,
    EntityTimeSpanValue       TEXT,
    IsLast                    INTEGER default 0 not null
)
"
            },
        }.ToFrozenDictionary();
    }
}
