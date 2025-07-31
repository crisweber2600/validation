using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using Xunit;
using Xunit.Gherkin.Quick;

namespace Validation.Tests.Features;

[FeatureFile("./Features/ValidationFlows.feature")]
public sealed class ValidationFlowsSteps
{
    private ServiceProvider? _provider;
    private ValidationFlowOptions? _options;

    [Given("a JSON configuration for Item flow")]
    public void GivenConfig()
    {
        var json = "{\"flows\":[{\"type\":\"Validation.Domain.Entities.Item, Validation.Domain\",\"saveValidation\":true,\"saveCommit\":true,\"metricProperty\":\"Metric\",\"thresholdType\":\"RawDifference\",\"thresholdValue\":10}]}";
        _options = ValidationFlowOptions.Load(json);
    }

    [When("services are configured with AddValidationFlows")]
    public void WhenServicesConfigured()
    {
        var services = new ServiceCollection();
        services.AddValidationFlows(_options!);
        _provider = services.BuildServiceProvider();
    }

    [Then("SaveValidationConsumer for Item can be resolved")]
    public void ThenSaveValidationConsumerResolved()
    {
        var svc = _provider!.GetService<SaveValidationConsumer<Item>>();
        Assert.NotNull(svc);
    }

    [Then("SaveCommitConsumer for Item can be resolved")]
    public void ThenSaveCommitConsumerResolved()
    {
        var svc = _provider!.GetService<SaveCommitConsumer<Item>>();
        Assert.NotNull(svc);
    }
}
