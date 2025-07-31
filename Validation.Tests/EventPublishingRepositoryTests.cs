using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class EventPublishingRepositoryTests
{
    [Fact]
    public async Task Save_async_publishes_SaveRequested_event()
    {
        var harness = new InMemoryTestHarness();
        var repo = new EventPublishingRepository<Item>(harness.Bus);
        await harness.Start();
        try
        {
            var item = new Item(10);
            await repo.SaveAsync(item);
            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
