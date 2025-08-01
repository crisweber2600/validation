using Microsoft.Extensions.Hosting;
using Validation.Domain;

namespace Validation.Infrastructure;

public class DefaultApplicationNameProvider : IApplicationNameProvider
{
    public string ApplicationName { get; }

    public DefaultApplicationNameProvider(IHostEnvironment? env = null)
    {
        ApplicationName = env?.ApplicationName ?? "App";
    }
}
