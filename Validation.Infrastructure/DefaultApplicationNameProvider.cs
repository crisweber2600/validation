using Validation.Domain;

namespace Validation.Infrastructure;

public class DefaultApplicationNameProvider : IApplicationNameProvider
{
    public string ApplicationName { get; }

    public DefaultApplicationNameProvider() : this("App") { }

    public DefaultApplicationNameProvider(string name)
    {
        ApplicationName = name;
    }
}
