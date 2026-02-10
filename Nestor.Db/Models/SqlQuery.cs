using System;
using System.Data.Common;
using System.Linq;

namespace Nestor.Db.Models;

public readonly struct SqlQuery
{
    public readonly string Sql;
    public readonly ReadOnlyMemory<QueryParameter> Parameters;

    public SqlQuery(string sql, params QueryParameter[] parameters)
    {
        Sql = sql;
        Parameters = parameters;
    }

    public SqlQuery(string sql)
    {
        Sql = sql;
        Parameters = ReadOnlyMemory<QueryParameter>.Empty;
    }

    public static implicit operator SqlQuery(string sql)
    {
        return new(sql);
    }

    public DbParameter[] CreateParameters(DbCommand command)
    {
        if (Parameters.IsEmpty)
        {
            return Array.Empty<DbParameter>();
        }

        return Parameters.ToArray().Select(p => p.CreateParameter(command)).ToArray();
    }
}
