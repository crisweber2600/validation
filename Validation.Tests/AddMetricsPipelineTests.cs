using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Validation.Tests;

public class AddMetricsPipelineTests
{
    [Fact]
    public void AddMetricsPipeline_registers_pipeline_services()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISaveAuditRepository, InMemorySaveAuditRepository>();
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IPublishEndpoint>(sp => new MassTransit.Testing.InMemoryTestHarness().Bus);
        services.AddSingleton<ILoggerFactory, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory>();
        services.AddLogging();
        services.AddMetricsPipeline<object>();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<SummarizationService>());
        Assert.NotNull(provider.GetService<ValidationService>());
        Assert.NotNull(provider.GetService<CommitService>());
        Assert.NotNull(provider.GetService<DiscardHandler>());
        var hosted = provider.GetServices<IHostedService>().OfType<PipelineOrchestrator<object>>().FirstOrDefault();
        Assert.NotNull(hosted);
    }
}
