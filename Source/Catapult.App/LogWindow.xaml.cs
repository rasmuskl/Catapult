using Catapult.Core.Debug;

namespace Catapult.App;

public partial class LogWindow
{
    public LogWindow()
    {
        InitializeComponent();
        LogWindowLogEventSink.Attach(s => Status.Dispatcher.Invoke(() => AddLog(s)));
    }

    private void AddLog(string message)
    {
        Status.AppendText(message + Environment.NewLine);
        Status.ScrollToEnd();
    }
}