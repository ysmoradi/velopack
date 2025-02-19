﻿using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Velopack.Vpk.Logging;

public class MySpectreConsoleSink : ILogEventSink
{
    private readonly string _dirtmp;

    public MySpectreConsoleSink()
    {
        _dirtmp = Path.GetTempPath();
    }

    public void Emit(LogEvent logEvent)
    {
        string prefix = $"[[{logEvent.Timestamp:HH:mm:ss} {LevelStyle.GetLevelHighlight(logEvent)}]] ";
        string message = logEvent.RenderMessage();

        if (VelopackRuntimeInfo.IsWindows) {
            message = message.Replace(_dirtmp, "%TEMP%\\");
        }

        List<IRenderable> renderables = new List<IRenderable>();

        if (message.Contains('\n') || Markup.Remove(prefix + message).Length > Console.WindowWidth) {
            renderables.Add(new Markup(prefix + Environment.NewLine));
            renderables.Add(new Padder(new Markup(message), new Padding(4, 0, 0, 0)));
        } else {
            renderables.Add(new Markup(prefix + message + Environment.NewLine));
        }

        if (logEvent.Exception != null) {
            renderables.Add(new Padder(logEvent.Exception.GetRenderable(ExceptionFormats.ShortenEverything), new Padding(4, 0, 0, 0)));
        }

        AnsiConsole.Write(new RenderableCollection(renderables));
    }
}

public static class MySpectreConsoleSinkExtensions
{
    public static LoggerConfiguration Spectre(
        this LoggerSinkConfiguration loggerConfiguration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
        return loggerConfiguration.Sink(new MySpectreConsoleSink(), restrictedToMinimumLevel, levelSwitch);
    }
}
