using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Xunit;

namespace Validation.Tests;

public class EventPublishingRepositoryHarnessTests
{
    [Fact]
    public async Task SaveAsync_publishes_event_with_matching_payload()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness();
        services.AddScoped<EventPublishingRepository<Item>>();
        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            using var scope = provider.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<EventPublishingRepository<Item>>();
            var item = new Item(10);
            await repo.SaveAsync(item);
            var published = await harness.Published.SelectAsync<SaveRequested<Item>>().First();
            Assert.Equal(item.Id, published.Context.Message.Entity.Id);
            Assert.Same(item, published.Context.Message.Entity);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
