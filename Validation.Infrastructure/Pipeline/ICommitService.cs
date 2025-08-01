namespace Validation.Infrastructure.Pipeline;

public interface ICommitService
{
    Task CommitAsync(double value, CancellationToken cancellationToken = default);
}
