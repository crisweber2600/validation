using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Pipeline;
using Validation.Tests;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class FixedGather : IGatherService
    {
        private readonly IEnumerable<decimal> _data;
        public FixedGather(IEnumerable<decimal> data) => _data = data;
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct) => Task.FromResult(_data);
    }

    private class StubPublish : MassTransit.IPublishEndpoint
    {
        public List<object> Published { get; } = new();
        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            Published.Add(message!);
            return Task.CompletedTask;
        }
        public Task Publish<T>(object message, CancellationToken cancellationToken = default) where T : class
            => Publish((T)message, cancellationToken);
        public Task Publish<T>(T message, MassTransit.IPipe<MassTransit.PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish(message, cancellationToken);
        public Task Publish<T>(object message, MassTransit.IPipe<MassTransit.PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish((T)message, cancellationToken);
        public Task Publish<T>(T message, MassTransit.IPipe<MassTransit.PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish(message, cancellationToken);
        public Task Publish<T>(object message, MassTransit.IPipe<MassTransit.PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class
            => Publish((T)message, cancellationToken);
        public Task Publish(object message, CancellationToken cancellationToken = default)
            => Publish<object>(message, cancellationToken);
        public Task Publish(object message, MassTransit.IPipe<MassTransit.PublishContext> publishPipe, CancellationToken cancellationToken = default)
            => Publish<object>(message, cancellationToken);
        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default)
        {
            Published.Add(message);
            return Task.CompletedTask;
        }
        public Task Publish(object message, Type messageType, MassTransit.IPipe<MassTransit.PublishContext> publishPipe, CancellationToken cancellationToken = default)
            => Publish(message, messageType, cancellationToken);
        public MassTransit.ConnectHandle ConnectPublishObserver(MassTransit.IPublishObserver observer) => throw new NotImplementedException();
    }

    private class TestDiscard : DiscardHandler
    {
        public int DiscardCount { get; private set; }
        public TestDiscard(Microsoft.Extensions.Logging.ILogger<DiscardHandler> logger, MassTransit.IPublishEndpoint publish) : base(logger, publish) { }
        public override Task HandleAsync<T>(decimal summary, CancellationToken ct)
        {
            DiscardCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Orchestrator_commits_when_valid()
    {
        var gather = new FixedGather(new[] {1m, 2m});
        var repo = new InMemorySaveAuditRepository();
        var publish = new StubPublish();
        var validator = new ValidationService(repo, new InMemoryValidationPlanProvider(), new SummarisationValidator());
        var summarizer = new SummarizationService(ValidationStrategy.Sum);
        var commit = new CommitService(repo, publish);
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscardHandler>.Instance;
        var discard = new TestDiscard(logger, publish);
        var orchestrator = new PipelineOrchestrator<object>(gather, summarizer, validator, commit, discard);

        await orchestrator.ExecuteAsync(CancellationToken.None);

        Assert.Single(repo.Audits);
        Assert.Equal(0, discard.DiscardCount);
    }


    [Fact]
    public async Task Orchestrator_discards_when_invalid()
    {
        var gather = new FixedGather(new[] {10m, 20m});
        var repo = new InMemorySaveAuditRepository();
        var publish = new StubPublish();
        var planProvider = new InMemoryValidationPlanProvider();
        planProvider.AddPlan<object>(new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5m));
        var validator = new ValidationService(repo, planProvider, new SummarisationValidator());
        var summarizer = new SummarizationService(ValidationStrategy.Sum);
        var commit = new CommitService(repo, publish);
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DiscardHandler>.Instance;
        var discard = new TestDiscard(logger, publish);
        var orchestrator = new PipelineOrchestrator<object>(gather, summarizer, validator, commit, discard);

        await orchestrator.ExecuteAsync(CancellationToken.None);

        Assert.Empty(repo.Audits);
        Assert.Equal(1, discard.DiscardCount);
    }
}
