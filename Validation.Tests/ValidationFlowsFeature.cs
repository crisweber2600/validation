using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Gherkin.Quick;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;
using Validation.Tests;
using Validation.Domain.Entities;

[FeatureFile("./Features/ValidationFlows.feature")]
public sealed class ValidationFlowsFeature : Feature
{
    private ValidationFlowOptions _options = null!;
    private ServiceProvider _provider = null!;

    [Given("a valid item flow configuration")]
    public void GivenConfiguration()
    {
        var json = "{ \"Flows\": [ { \"Type\": \"Validation.Domain.Entities.Item, Validation.Domain\", \"SaveValidation\": true, \"SaveCommit\": true, \"MetricProperty\": \"Metric\", \"ThresholdType\": \"Raw\", \"ThresholdValue\": 5 } ] }";
        _options = ValidationFlowOptions.Load(json);
    }

    [When("services are built")]
    public void WhenServicesAreBuilt()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidationPlanProvider, DummyPlanProvider>();
        services.AddScoped<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddValidationFlows(_options);
        _provider = services.BuildServiceProvider();
    }

    private class DummyPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(10m) };
    }

    [Then("SaveValidationConsumer can be resolved for Item")]
    public void ThenSaveValidationConsumerCanBeResolved()
    {
        Assert.NotNull(_provider.GetService<SaveValidationConsumer<Item>>());
    }

    [And("SaveCommitConsumer can be resolved for Item")]
    public void ThenSaveCommitConsumerCanBeResolved()
    {
        Assert.NotNull(_provider.GetService<SaveCommitConsumer<Item>>());
    }
}

