using Microsoft.Extensions.Configuration;

namespace Proply.IntegrationTests;

public abstract class TestBase
{
    protected readonly IConfiguration Configuration;

    protected TestBase()
    {
        // Load configuration from the Bootstrapper project's appsettings.json
        // or local secrets if available.
        // Assuming the test runs from bin/Debug/net9.0/ we need to go up to src
        
        var currentDirectory = Directory.GetCurrentDirectory();
        
        // Navigate up to solution root (approximate, simpler to just hardcode partial path or find solution root)
        // A robust way to find the source appsettings:
        var solutionRoot = FindSolutionRoot(currentDirectory);
        var appSettingsPath = Path.Combine(solutionRoot, "src", "Proply.Bootstrapper", "appsettings.json");

        var builder = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: true)
            .AddUserSecrets<TestBase>(); // Optional: if we want to override with secrets

        Configuration = builder.Build();
    }

    private string FindSolutionRoot(string currentPath)
    {
        var dir = new DirectoryInfo(currentPath);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Proply.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new Exception("Could not find solution root");
    }
}
