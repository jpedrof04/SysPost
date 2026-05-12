using System.Diagnostics;

namespace SysPost.Services;

public class QueryMonitor
{
    private readonly Stopwatch _sw = new();

    public int QueryCount { get; private set; }
    public double TotalTimeMs { get; private set; }
    public double LastQueryMs { get; private set; }

    public void Start()
    {
        _sw.Restart();
    }

    public void Stop()
    {
        _sw.Stop();
        LastQueryMs = _sw.Elapsed.TotalMilliseconds;
        TotalTimeMs += LastQueryMs;
        QueryCount++;
    }
}
