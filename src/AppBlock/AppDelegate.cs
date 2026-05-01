using AppBlock.Menu;

namespace AppBlock;

public sealed class AppDelegate : NSApplicationDelegate
{
    private readonly BlockedApplicationsMenuItem blockedApplications = new();
    private NSStatusItem? statusItem;

    public override void DidFinishLaunching(NSNotification notification)
    {
        NSWorkspace.Notifications.ObserveWillLaunchApplication(OnLaunch);

        statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Square);
        statusItem.Button.Image = NSImage.GetSystemSymbol("figure.walk.circle.fill", null);
        statusItem.Menu = new NSMenu();
        statusItem.Menu.AddItem(blockedApplications);
        statusItem.Menu.AddItem(new AutoLaunchMenuItem());
        statusItem.Menu.AddItem(NSMenuItem.SeparatorItem);
        statusItem.Menu.AddItem(new QuitMenuItem());
    }

    private void OnLaunch(object? sender, NSWorkspaceApplicationEventArgs eventArgs)
    {
        if (blockedApplications.IsBlocked(eventArgs.Application.BundleIdentifier))
        {
            eventArgs.Application.ForceTerminate();
        }
    }
}
