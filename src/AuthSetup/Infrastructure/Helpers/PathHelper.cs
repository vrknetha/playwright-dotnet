using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace AuthSetup.Infrastructure.Helpers;

public static class PathHelper
{
    public static string GetSolutionRootPath()
    {
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDir != null && !File.Exists(Path.Combine(currentDir.FullName, "PlaywrightDemo.sln")))
        {
            currentDir = currentDir.Parent;
        }

        var rootPath = currentDir?.FullName
            ?? throw new InvalidOperationException("Could not find solution root directory");

        // Verify the path
        if (!Directory.Exists(rootPath))
        {
            throw new InvalidOperationException($"Root path does not exist: {rootPath}");
        }

        return rootPath;
    }
}