using Jahoot.WebApi.Services;
using Microsoft.Extensions.Time.Testing;

namespace Jahoot.WebApi.Tests.Services;

public class TimedBackgroundServiceTests
{
    private FakeTimeProvider _fakeTimeProvider;

    [SetUp]
    public void SetUp()
    {
        _fakeTimeProvider = new FakeTimeProvider();
    }

    [Test]
    public void Constructor_SetsTimeProvider()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);

        Assert.That(service.ExposedTimeProvider, Is.EqualTo(_fakeTimeProvider));
    }

    [Test]
    public void InitializeTimer_CreatesTimerWithCorrectParameters()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);
        bool callbackCalled = false;

        TimeSpan interval = TimeSpan.FromSeconds(10);

        service.CallInitializeTimer(Callback, interval);
        _fakeTimeProvider.Advance(interval);

        Assert.That(callbackCalled, Is.True);
        return;

        void Callback(object? _)
        {
            callbackCalled = true;
        }
    }

    [Test]
    public void Dispose_DisposesTimer()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);
        bool callbackCalled = false;

        TimeSpan interval = TimeSpan.FromSeconds(10);
        service.CallInitializeTimer(Callback, interval);

        service.Dispose();
        _fakeTimeProvider.Advance(interval);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(service.IsDisposed, Is.True);
            Assert.That(callbackCalled, Is.False);
        }

        return;

        void Callback(object? _)
        {
            callbackCalled = true;
        }
    }

    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);

        service.Dispose();

        Assert.DoesNotThrow(() => service.Dispose());
        Assert.That(service.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_SetsDisposedFlag()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);

        service.Dispose(true);

        Assert.That(service.IsDisposed, Is.True);
    }

    [Test]
    public void InitializeTimer_DisposesPreviousTimerWhenCalledAgain()
    {
        TestableTimedBackgroundService service = new(_fakeTimeProvider);
        bool firstCallbackCalled = false;
        bool secondCallbackCalled = false;

        TimeSpan interval = TimeSpan.FromSeconds(10);

        service.CallInitializeTimer(_ => firstCallbackCalled = true, interval);
        _fakeTimeProvider.Advance(interval);
        Assert.That(firstCallbackCalled, Is.True, "First callback should have been called initially.");
        firstCallbackCalled = false; // Reset for checking after timer updated

        service.CallInitializeTimer(_ => secondCallbackCalled = true, interval);
        _fakeTimeProvider.Advance(interval);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(firstCallbackCalled, Is.False, "First callback should not be called after new timer initialized.");
            Assert.That(secondCallbackCalled, Is.True, "Second callback should be called.");
        }
    }

    private class TestableTimedBackgroundService(TimeProvider timeProvider) : TimedBackgroundService(timeProvider)
    {
        public bool IsDisposed => Disposed;

        public TimeProvider ExposedTimeProvider => TimeProvider;

        public void CallInitializeTimer(TimerCallback callback, TimeSpan interval)
        {
            InitializeTimer(callback, interval);
        }

        public new void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
