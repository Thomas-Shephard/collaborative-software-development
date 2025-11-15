namespace Jahoot.WebApi.Services;

public abstract class TimedBackgroundService : IDisposable
{
    private Timer? _timer;
    protected bool Disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void InitializeTimer(TimerCallback callback, TimeSpan interval)
    {
        _timer = new Timer(callback, null, interval, interval);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            _timer?.Dispose();
        }

        Disposed = true;
    }
}
