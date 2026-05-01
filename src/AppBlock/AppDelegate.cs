using System.Collections.Frozen;
using ServiceManagement;

namespace AppBlock;

public sealed class AppDelegate : NSApplicationDelegate
{
    private FrozenSet<string> preventedApplications = [];
    private NSStatusItem? statusItem;
    private NSMenuItem? configurationItem;
    private NSMenuItem? autoLaunchItem;

    public override void DidFinishLaunching(NSNotification notification)
    {
        NSWorkspace.Notifications.ObserveWillLaunchApplication(OnLaunch);

        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
        statusItem.Button.Image = NSImage.GetSystemSymbol("figure.walk.circle.fill", null);
        statusItem.Menu = new NSMenu();
        statusItem.Menu.AddItem(configurationItem ??= new NSMenuItem("Applications") { Submenu = new NSMenu() });
        statusItem.Menu.AddItem(autoLaunchItem ??= new NSMenuItem("Launch at login", ToggleAutoLaunch) { State = AutoLaunchEnabled });
        statusItem.Menu.AddItem(NSMenuItem.SeparatorItem);
        statusItem.Menu.AddItem(new NSMenuItem("Quit", OnQuit));

        LoadBlockedApplications();
    }

    private void LoadBlockedApplications()
    {
        var filePath = Environment.GetEnvironmentVariable("APPBLOCK_CONFIG")
                       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "appblock", "blocked-applications.txt");

        if (File.Exists(filePath))
        {
            preventedApplications = File.ReadAllLines(filePath)
                .Where(static line => !line.StartsWith('#') && !string.IsNullOrWhiteSpace(line))
                .Select(static line => line.Trim())
                .Order()
                .ToFrozenSet();
        }
        else
        {
            preventedApplications = [];
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

        if (configurationItem?.Submenu is null)
        {
            return;
        }

        configurationItem.Submenu.RemoveAllItems();

        if (preventedApplications.Any())
        {
            foreach (var application in preventedApplications)
            {
                configurationItem.Submenu.AddItem(new NSMenuItem(application));
            }
        }
        else
        {
            configurationItem.Submenu.AddItem(new NSMenuItem("No applications blocked"));
        }

        configurationItem.Submenu.AddItem(NSMenuItem.SeparatorItem);
        configurationItem.Submenu.AddItem(new NSMenuItem("Edit", (_, _) => NSWorkspace.SharedWorkspace.OpenUrl(new NSUrl(filePath, isDir: false))));
        configurationItem.Submenu.AddItem(new NSMenuItem("Reload", (_, _) => LoadBlockedApplications()));
    }

    private NSCellStateValue AutoLaunchEnabled => SMAppService.MainApp.Status is SMAppServiceStatus.Enabled
        ? NSCellStateValue.On
        : NSCellStateValue.Off;

    private void ToggleAutoLaunch(object? sender, EventArgs eventArgs)
    {
        if (AutoLaunchEnabled is NSCellStateValue.On)
        {
            SMAppService.MainApp.Unregister();
            autoLaunchItem?.State = NSCellStateValue.Off;
        }
        else
        {
            SMAppService.MainApp.Register();
            autoLaunchItem?.State = NSCellStateValue.On;
        }
    }

    private void OnLaunch(object? sender, NSWorkspaceApplicationEventArgs eventArgs)
    {
        if (eventArgs.Application.BundleIdentifier is not null && preventedApplications.Contains(eventArgs.Application.BundleIdentifier))
        {
            eventArgs.Application.ForceTerminate();
        }
    }

    private void OnQuit(object? sender, EventArgs eventArgs)
    {
        NSApplication.SharedApplication.Stop(Self);
    }
}
