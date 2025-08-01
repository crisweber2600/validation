using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Domain.Entities;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class AddValidationFlowsTests
{
    [Fact]
    public void Registers_consumers_from_json_config()
    {
        var json = "{\"Type\":\"Validation.Domain.Entities.Item, Validation.Domain\",\"SaveValidation\":true,\"SaveCommit\":true,\"MetricProperty\":\"Metric\",\"ThresholdType\":\"PercentChange\",\"ThresholdValue\":0.2}";
        var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
        var options = JsonSerializer.Deserialize<ValidationFlowServiceCollectionExtensions.ValidationFlowConfig[]>("[" + json + "]", opts);
        var services = new ServiceCollection();
        services.AddSingleton<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddValidationFlows(options!);
        var provider = services.BuildServiceProvider(true);
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<SaveValidationConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
    }
}
