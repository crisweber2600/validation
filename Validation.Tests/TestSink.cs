using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Validation.Tests;

public class TestSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = new();
    public void Emit(LogEvent logEvent)
    {
        Events.Add(logEvent);
    }
}
