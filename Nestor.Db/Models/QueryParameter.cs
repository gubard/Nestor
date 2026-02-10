using System;
using System.Data.Common;

namespace Nestor.Db.Models;

public readonly struct QueryParameter
{
    public readonly string Name;
    public readonly object Value;

    public QueryParameter(string name, object? value)
    {
        Name = name;
        Value = value ?? DBNull.Value;
    }

    public DbParameter CreateParameter(DbCommand command)
    {
        var parameter = command.CreateParameter();
        parameter.Value = Value;
        parameter.ParameterName = Name;

        return parameter;
    }
}
