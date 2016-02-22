using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace AlphaLaunch.Core.Debug
{
    public static class LogWindowLoggerConfigurationExtensions
    {
        const string DefaultConsoleOutputTemplate = "{Message}{Exception}";

        public static LoggerConfiguration LogWindow(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultConsoleOutputTemplate,
            IFormatProvider formatProvider = null)
        {
            if (sinkConfiguration == null)
            {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }
            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            return sinkConfiguration.Sink(new LogWindowLogEventSink(formatter), restrictedToMinimumLevel);
        }
    }
}