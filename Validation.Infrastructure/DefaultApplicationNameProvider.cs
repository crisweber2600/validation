using Validation.Domain;

namespace Validation.Infrastructure;

public class DefaultApplicationNameProvider : IApplicationNameProvider
{
    public string ApplicationName { get; }

    public DefaultApplicationNameProvider(string? name = null)
    {
        ApplicationName = name ?? "App";
    }
}
