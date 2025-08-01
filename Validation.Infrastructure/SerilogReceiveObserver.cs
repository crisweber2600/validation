using System;
using System.Threading.Tasks;
using MassTransit;
using Serilog;

namespace Validation.Infrastructure;

/// <summary>
/// Logs failed message deliveries using Serilog.
/// </summary>
public class SerilogReceiveObserver : IReceiveObserver
{
    private readonly ILogger _logger;

    public SerilogReceiveObserver(ILogger logger)
    {
        _logger = logger;
    }

    public Task PreReceive(ReceiveContext context) => Task.CompletedTask;
    public Task PostReceive(ReceiveContext context) => Task.CompletedTask;
    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class => Task.CompletedTask;

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan elapsed, string consumerType, Exception exception) where T : class
    {
        _logger.Error(exception, "Message of type {MessageType} failed", typeof(T).Name);
        return Task.CompletedTask;
    }

    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        _logger.Error(exception, "Receive fault for {InputAddress}", context.InputAddress);
        return Task.CompletedTask;
    }
}
