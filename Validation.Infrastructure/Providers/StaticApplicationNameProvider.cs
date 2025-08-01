namespace Validation.Infrastructure;

public class StaticApplicationNameProvider : IApplicationNameProvider
{
    public StaticApplicationNameProvider(string name)
    {
        ApplicationName = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string ApplicationName { get; }
}
