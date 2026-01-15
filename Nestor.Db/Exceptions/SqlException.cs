using System;
using Nestor.Db.Models;

namespace Nestor.Db.Exceptions;

public sealed class SqlException : Exception
{
    public SqlException(SqlQuery query, Exception inner)
        : base(query.Sql, inner)
    {
        Query = query;
    }

    public SqlQuery Query { get; }
}
