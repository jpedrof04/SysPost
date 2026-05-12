using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace SysPost.Services;

public class QueryMonitorInterceptor : DbCommandInterceptor
{
    private readonly QueryMonitor _monitor;

    public QueryMonitorInterceptor(QueryMonitor monitor)
    {
        _monitor = monitor;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        _monitor.Start();
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        _monitor.Start();
        return new ValueTask<InterceptionResult<DbDataReader>>(result);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        _monitor.Stop();
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        _monitor.Stop();
        return new ValueTask<DbDataReader>(result);
    }
}
