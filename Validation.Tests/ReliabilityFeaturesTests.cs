using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Domain.Events;
using Xunit;

namespace Validation.Tests;

public class ReliabilityFeaturesTests
{
    class FailingConsumer : IConsumer<SaveRequested>
    {
        public static int Attempts;

        public Task Consume(ConsumeContext<SaveRequested> context)
        {
            System.Threading.Interlocked.Increment(ref Attempts);
            throw new InvalidOperationException("fail");
        }
    }

    [Fact]
    public async Task Failing_message_is_retried_and_sent_to_error_queue()
    {
        FailingConsumer.Attempts = 0;
        var services = new ServiceCollection();
        services.AddValidationInfrastructure(cfg => cfg.AddConsumer<FailingConsumer>());

        await using var provider = services.BuildServiceProvider(true);
        var bus = provider.GetRequiredService<IBusControl>();
        await bus.StartAsync();
        try
        {
            using var scope = provider.CreateScope();
            var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            await publish.Publish(new SaveRequested(Guid.NewGuid()));
            await Task.Delay(1000);

            Assert.Equal(4, FailingConsumer.Attempts);
        }
        finally
        {
            await bus.StopAsync();
        }
    }
}
