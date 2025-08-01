using Validation.Domain;

namespace Validation.Infrastructure;

public sealed class StaticApplicationNameProvider : IApplicationNameProvider
{
    private readonly string _name;

    public StaticApplicationNameProvider(string name) => _name = name;

    public string GetName() => _name;
}
