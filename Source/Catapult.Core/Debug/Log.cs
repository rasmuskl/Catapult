using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Catapult.Core.Debug
{
    public class LogWindowLogEventSink : ILogEventSink
    {
        private readonly MessageTemplateTextFormatter _formatter;
        private static readonly List<string> Buffer = new List<string>();
        private static readonly List<Action<string>> Listeners = new List<Action<string>>();

        public LogWindowLogEventSink(MessageTemplateTextFormatter formatter)
        {
            _formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            var renderSpace = new StringWriter();
            _formatter.Format(logEvent, renderSpace);
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
}