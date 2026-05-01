using System.Collections.Frozen;

namespace AppBlock;

public sealed class AppDelegate : NSApplicationDelegate
{
    private NSStatusItem? statusItem;
    private FrozenSet<string?> preventedApplications = FrozenSet.Create<string?>("com.apple.Music");

    public override void DidFinishLaunching(NSNotification notification)
    {
        NSWorkspace.Notifications.ObserveWillLaunchApplication(OnLaunch);

        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
        statusItem.Button.Image = NSImage.GetSystemSymbol("figure.walk.circle.fill", null);
        statusItem.Menu = new NSMenu();
        statusItem.Menu.AddItem(new NSMenuItem("Quit", OnQuit));
    }

    private void OnLaunch(object? sender, NSWorkspaceApplicationEventArgs eventArgs)
    {
        if (preventedApplications.Contains(eventArgs.Application.BundleIdentifier))
        {
            eventArgs.Application.ForceTerminate();
        }
    }

    private void OnQuit(object? sender, EventArgs eventArgs)
    {
        NSApplication.SharedApplication.Stop(Self);
    }
}
