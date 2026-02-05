using System.Collections.Frozen;
using System.Collections.Generic;

namespace Nestor.Db.Helpers;

public static class IdempotenceMigration
{
    public static readonly FrozenDictionary<int, string> Migrations;

    static IdempotenceMigration()
    {
        Migrations = new Dictionary<int, string>
        {
            {
                20,
                @"
CREATE TABLE IF NOT EXISTS Idempotents (
  Id        TEXT    NOT NULL PRIMARY KEY,
  Data      BLOB    NOT NULL,
  DataType  TEXT    NOT NULL,
  CreatedAt TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now'))
);
"
            },
        }.ToFrozenDictionary();
    }
}
