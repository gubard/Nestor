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
}
