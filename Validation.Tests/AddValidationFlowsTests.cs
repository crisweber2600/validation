using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Validation.Tests;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Validation;
using Xunit;

namespace Validation.Tests;

public class AddValidationFlowsTests
{
    [Fact]
    public void AddValidationFlows_registers_consumers_from_json()
    {
        const string json = "[{" +
            "\"Type\":\"Validation.Domain.Entities.Item, Validation.Domain\"," +
            "\"SaveValidation\":true," +
            "\"SaveCommit\":true," +
            "\"MetricProperty\":\"Metric\"," +
            "\"ThresholdType\":1," +
            "\"ThresholdValue\":0.2" +
            "}]";
        using var doc = JsonDocument.Parse(json);
        var configs = new List<ValidationFlowConfig>();
        foreach (var el in doc.RootElement.EnumerateArray())
        {
            el.TryGetProperty("ThresholdType", out var tt);
            el.TryGetProperty("ThresholdValue", out var tv);
            configs.Add(new ValidationFlowConfig
            {
                Type = el.GetProperty("Type").GetString()!,
                SaveValidation = el.GetProperty("SaveValidation").GetBoolean(),
                SaveCommit = el.GetProperty("SaveCommit").GetBoolean(),
                MetricProperty = el.GetProperty("MetricProperty").GetString(),
                ThresholdType = tt.ValueKind == JsonValueKind.Number ? (ThresholdType?)tt.GetInt32() : tt.ValueKind == JsonValueKind.String ? Enum.Parse<ThresholdType>(tt.GetString()!, true) : null,
                ThresholdValue = tv.ValueKind == JsonValueKind.Number ? tv.GetDecimal() : null
            });
        }

        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("cfg"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddValidationFlows(configs);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<SaveValidationConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
    }
}

