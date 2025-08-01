using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using ValidationFlow.Messages;
using Validation.Infrastructure.Messaging;
using MassTransit;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;

namespace Validation.Tests;

public class ValidationFlowIntegrationTests
{
    [Fact]
    public async Task Save_requested_triggers_validated_event_and_audit_saved()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<SaveRequestedConsumer<Item>>());
        services.AddValidationFlow<AlwaysValidRule>(opts =>
        {
            opts.SetupDatabase<TestDbContext>("flowtest");
        });

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            using var scope = provider.CreateScope();
            var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var item = new Item(2);
            await publish.Publish(new SaveRequested<Item>("App", "Item", item.Id, item));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            var ctx = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            Assert.Equal(1, ctx.SaveAudits.Count());
        }
        finally
        {
            await harness.Stop();
        }
    }
}