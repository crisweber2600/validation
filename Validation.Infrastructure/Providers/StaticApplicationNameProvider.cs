namespace Validation.Infrastructure.Providers;

public class StaticApplicationNameProvider : IApplicationNameProvider
{
    public StaticApplicationNameProvider(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }
}
