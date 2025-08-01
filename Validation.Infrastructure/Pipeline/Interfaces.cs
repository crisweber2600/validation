namespace Validation.Infrastructure.Pipeline;

public interface IGatherService
{
    Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default);
}

public interface ISummarisationService
{
    Task<T> SummariseAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default);
}

public interface IValidationService
{
    Task<bool> ValidateAsync<T>(T summary, CancellationToken cancellationToken = default);
}

public interface ICommitService
{
    Task CommitAsync<T>(T summary, CancellationToken cancellationToken = default);
}
