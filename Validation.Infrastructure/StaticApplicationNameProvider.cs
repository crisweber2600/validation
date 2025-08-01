namespace Validation.Infrastructure;

public class StaticApplicationNameProvider : IApplicationNameProvider
{
    private readonly string _name;
    public StaticApplicationNameProvider(string name)
    {
        _name = name;
    }

    public string GetApplicationName() => _name;
}
