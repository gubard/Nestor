using System;
using System.Collections.Generic;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Nestor.Db.Helpers;

public static class InsertHelper
{
    private static readonly Dictionary<string, Func<Guid, DbSession, SqlQuery>> Factories = new();

    public static SqlQuery CreateDefaultInsert(string entityName, Guid id, DbSession session)
    {
        return Factories[entityName].Invoke(id, session);
    }

    public static void AddDefaultInsert(string entityName, Func<Guid, DbSession, SqlQuery> func)
    {
        Factories.Add(entityName, func);
    }
}
