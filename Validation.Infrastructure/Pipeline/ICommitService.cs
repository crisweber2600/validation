namespace Validation.Infrastructure.Pipeline;

public interface ICommitService
{
    Task CommitAsync<T>(decimal summary, CancellationToken cancellationToken = default);
}
