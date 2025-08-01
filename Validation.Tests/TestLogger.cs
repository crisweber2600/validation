using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Validation.Tests;

public class TestLogger<T> : ILogger<T>
{
    private class NullScope : IDisposable { public void Dispose() { } }
    public ConcurrentBag<string> Messages { get; } = new();

    public IDisposable BeginScope<TState>(TState state) => new NullScope();
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Messages.Add(formatter(state, exception));
}
