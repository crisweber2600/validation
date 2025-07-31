using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Gherkin.Quick;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Tests;

[FeatureFile("./ValidationFlowConfig.feature")]
public sealed class ConfigSteps : Feature
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
    }
    private string _json = string.Empty;
    private ServiceProvider? _provider;

    [Given("a validation flow configuration")]
    public void GivenConfiguration()
    {
        _json = "{\"Flows\":[{\"Type\":\"Validation.Domain.Entities.Item, Validation.Domain\",\"SaveValidation\":true,\"SaveCommit\":true}]}";
    }

    [When("I load the options and configure services")]
    public void WhenLoadOptions()
    {
        var opts = ValidationFlowOptions.Load(_json);
        var services = new ServiceCollection();
        services.AddSingleton<IValidationPlanProvider, TestPlanProvider>();
        services.AddSingleton<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddSingleton<SummarisationValidator>();
        services.AddValidationFlows(opts);
        _provider = services.BuildServiceProvider();
    }

    [Then("services for Item are resolvable")]
    public void ThenServicesResolvable()
    {
        Assert.NotNull(_provider!.GetService<SaveValidationConsumer<Item>>());
        Assert.NotNull(_provider!.GetService<SaveCommitConsumer<Item>>());
    }
}
