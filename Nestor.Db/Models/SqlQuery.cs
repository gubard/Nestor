using System.Data.Common;

namespace Nestor.Db.Models;

public readonly struct SqlQuery
{
    public readonly string Sql;
    public readonly DbParameter[] Parameters;

    public SqlQuery(string sql, params DbParameter[] parameters)
    {
        Sql = sql;
        Parameters = parameters;
    }

    public SqlQuery(string sql)
    {
        Sql = sql;
        Parameters = [];
    }

    public static implicit operator SqlQuery(string sql)
    {
        return new(sql);
    }
}
