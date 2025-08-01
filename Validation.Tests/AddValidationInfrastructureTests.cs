using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Infrastructure.DI;
using Xunit;

namespace Validation.Tests;

public class AddValidationInfrastructureTests
{
    public record RetryMessage;

    class FailingConsumer : IConsumer<RetryMessage>
    {
        public static int Attempts;
        public Task Consume(ConsumeContext<RetryMessage> context)
        {
            Attempts++;
            throw new Exception("fail");
        }
    }

    [Fact]
    public async Task AddValidationInfrastructure_retries_failed_messages()
    {
        FailingConsumer.Attempts = 0;
        var services = new ServiceCollection();
        services.AddValidationInfrastructure(x =>
        {
            x.AddConsumer<FailingConsumer>();
        });
        await using var provider = services.BuildServiceProvider();
        var busControl = provider.GetRequiredService<IBusControl>();

        await busControl.StartAsync();
        try
        {
            var bus = provider.GetRequiredService<IBus>();
            await bus.Publish(new RetryMessage());
            await Task.Delay(1000);
            Assert.Equal(4, FailingConsumer.Attempts);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }

    [Fact]
    public async Task AddMongoValidationInfrastructure_retries_failed_messages()
    {
        FailingConsumer.Attempts = 0;
        var services = new ServiceCollection();
        var database = new MongoClient("mongodb://localhost:27017").GetDatabase("test");
        services.AddMongoValidationInfrastructure(database, x =>
        {
            x.AddConsumer<FailingConsumer>();
        });
        await using var provider = services.BuildServiceProvider();
        var busControl = provider.GetRequiredService<IBusControl>();

        await busControl.StartAsync();
        try
        {
            var bus = provider.GetRequiredService<IBus>();
            await bus.Publish(new RetryMessage());
            await Task.Delay(1000);
            Assert.Equal(4, FailingConsumer.Attempts);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}
