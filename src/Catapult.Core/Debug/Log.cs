using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Catapult.Core.Debug;

public class LogWindowLogEventSink(MessageTemplateTextFormatter formatter) : ILogEventSink
{
    private static readonly List<string> Buffer = [];
    private static readonly List<Action<string>> Listeners = [];

    public void Emit(LogEvent logEvent)
    {
        var renderSpace = new StringWriter();
        formatter.Format(logEvent, renderSpace);
        var output = renderSpace.ToString();

        if (Listeners.Any())
        {
            foreach (var listener in Listeners)
            {
                listener(output);
            }
        }
        else
        {
            Buffer.Add(output);
        }
    }

    public static void Attach(Action<string> listener)
    {
        foreach (var line in Buffer)
        {
            listener(line);
        }

        Listeners.Add(listener);
        Buffer.Clear();
    }
}