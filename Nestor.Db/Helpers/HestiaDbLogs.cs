using System;
using Microsoft.Extensions.Logging;

namespace Nestor.Db.Helpers;

public static partial class NestorDbLogs
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Post Id: {RequestId}")]
    public static partial void PostRequestId(this ILogger logger, Guid requestId);
}
