using System.IO;
using Xunit;

namespace Validation.Tests;

public class LinqPadScriptTests
{
    [Fact]
    public void ValidationServiceDemoScriptExists()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && !File.Exists(Path.Combine(dir, "Validation.sln")))
            dir = Directory.GetParent(dir)?.FullName;
        Assert.NotNull(dir);
        var scriptPath = Path.Combine(dir!, "ValidationServiceDemo.linq");
        Assert.True(File.Exists(scriptPath), $"Script file not found at {scriptPath}");
    }
}
