using MassTransit.Testing;
using MassTransit;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class EventPublishingRepositoryHarnessTests
{
    [Fact]
    public async Task SaveAsync_publishes_matching_message()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(_ => { });
        services.AddScoped<EventPublishingRepository<Item>>();

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            using var scope = provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<EventPublishingRepository<Item>>();
            var item = new Item(5);
            await repo.SaveAsync(item);

            IPublishedMessage<SaveRequested<Item>>? published = null;
            await foreach (var msg in harness.Published.SelectAsync<SaveRequested<Item>>())
            {
                published = msg;
                break;
            }
            Assert.NotNull(published);
            var message = published!.Context.Message;
            Assert.Equal(item.Id, message.Entity.Id);
            Assert.Equal(item.Metric, message.Entity.Metric);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
