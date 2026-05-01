using System.Collections.Frozen;

namespace AppBlock.Menu;

public sealed class BlockedApplicationsMenuItem : NSMenuItem
{
    private static string ConfigFilePath { get; } = GetConfigurationFilePath();
    private FrozenSet<string> blockedApplications = [];

    public BlockedApplicationsMenuItem()
    {
        Title = "Blocked Applications";
        Initialize();
    }

    public bool IsBlocked(string? appBundleIdentifier)
    {
        return appBundleIdentifier is not null && blockedApplications.Contains(appBundleIdentifier);
    }

    private void Initialize()
    {
        blockedApplications = ReadConfigFile(ConfigFilePath);

        Submenu ??= new NSMenu();
        Submenu.RemoveAllItems();

        if (blockedApplications.Any())
        {
            foreach (var application in blockedApplications)
            {
                Submenu.AddItem(new NSMenuItem(application));
            }
        }
        else
        {
            Submenu.AddItem(new NSMenuItem("No applications blocked"));
        }

        Submenu.AddItem(SeparatorItem);
        Submenu.AddItem(new NSMenuItem("Edit", (_, _) => NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(ConfigFilePath, isDir: false))));
        Submenu.AddItem(new NSMenuItem("Reload", (_, _) => Initialize()));
    }

    private static FrozenSet<string> ReadConfigFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            try
            {
                File.WriteAllLines(filePath,
                [
                    "# List the applications you want to block in here.",
                    "# One application per line, include nothing else.",
                    "# Lines with a # in front are ignored.",
                    "",
                    "# Example: Block Apple Music from launching",
                    "# com.apple.Music"
                ]);
            }
            catch (Exception)
            {
                // If we can't write the explanation file... we just don't care.
            }

            return [];
        }

        return File.ReadAllLines(filePath)
            .Where(static line => !line.StartsWith('#') && !string.IsNullOrWhiteSpace(line))
            .Select(static line => line.Trim())
            .Order()
            .ToFrozenSet();
    }

    private static string GetConfigurationFilePath()
    {
        var overridePath = Environment.GetEnvironmentVariable("APPBLOCK_CONFIG");
        if (overridePath is not null)
        {
            return overridePath;
        }

        var configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "appblock");
        Directory.CreateDirectory(configDirectory);

        return Path.Combine(configDirectory, "blocked-applications.txt");
    }
}
