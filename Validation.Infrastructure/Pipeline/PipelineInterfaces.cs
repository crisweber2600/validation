namespace Validation.Infrastructure.Pipeline;

public interface IGatherService
{
    Task<IEnumerable<T>> GatherAsync<T>(CancellationToken ct = default);
}

public interface ISummarisationService
{
    Task<T> SummariseAsync<T>(IEnumerable<T> items, CancellationToken ct = default);
}

public interface IValidationService
{
    Task<bool> ValidateAsync<T>(T summary, CancellationToken ct = default);
}

public interface ICommitService
{
    Task CommitAsync<T>(T summary, CancellationToken ct = default);
}

public record PipelineDiscardedEvent(Type ItemType, object? Summary);
