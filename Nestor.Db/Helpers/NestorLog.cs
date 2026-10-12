using System;
using Microsoft.Extensions.Logging;

namespace Nestor.Db.Helpers;

public static partial class NestorLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Start POST request")]
    public static partial void StartPostRequest(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "End POST request. Total time {Time}"
    )]
    public static partial void EndPostRequest(this ILogger logger, TimeSpan time);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error executing database command with connection string {ConnectionString}"
    )]
    public static partial void ConnectionError(this ILogger logger, string connectionString);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Error,
        Message = "Error executing database command with {CommandText}"
    )]
    public static partial void CommandError(this ILogger logger, string commandText);
}
