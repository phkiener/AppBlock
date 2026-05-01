using ServiceManagement;

namespace AppBlock.Menu;

public sealed class AutoLaunchMenuItem : NSMenuItem
{
    public AutoLaunchMenuItem()
    {
        Title = "Launch at login";
        State = AutoLaunchState;

        Activated += ToggleAutoLaunch;
    }

    private NSCellStateValue AutoLaunchState => SMAppService.MainApp.Status is SMAppServiceStatus.Enabled
        ? NSCellStateValue.On
        : NSCellStateValue.Off;

    private void ToggleAutoLaunch(object? sender, EventArgs e)
    {
        if (SMAppService.MainApp.Status is SMAppServiceStatus.Enabled)
        {
            SMAppService.MainApp.Unregister();
            State = NSCellStateValue.Off;
        }
        else
        {
            SMAppService.MainApp.Register();
            State = NSCellStateValue.On;
        }
    }
}
