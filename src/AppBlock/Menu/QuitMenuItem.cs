namespace AppBlock.Menu;

public sealed class QuitMenuItem : NSMenuItem
{
    public QuitMenuItem()
    {
        Title = "Quit";
        Activated += OnQuit;
    }

    private void OnQuit(object? sender, EventArgs eventArgs)
    {
        NSApplication.SharedApplication.Stop(Self);
    }
}
