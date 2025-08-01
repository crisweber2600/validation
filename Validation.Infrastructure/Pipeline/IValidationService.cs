namespace Validation.Infrastructure.Pipeline;

public interface IValidationService
{
    Task<bool> ValidateAsync<T>(decimal summary, CancellationToken cancellationToken = default);
}
