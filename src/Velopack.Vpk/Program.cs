﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using Velopack.Packaging;
using Velopack.Vpk.Commands;
using Velopack.Vpk.Compat;
using Velopack.Vpk.Logging;

namespace Velopack.Vpk;

public class Program
{
    public static CliOption<bool> VerboseOption { get; }
        = new CliOption<bool>("--verbose")
        .SetRecursive(true)
        .SetDescription("Print diagnostic messages.");

    public static CliOption<bool> LegacyConsole { get; }
        = new CliOption<bool>("--legacy-console")
        .SetRecursive(true)
        .SetDescription("Disable console colors and interactive components.");

    private static RunnerFactory Runner { get; set; }

    public static readonly string INTRO
        = $"Velopack CLI {VelopackRuntimeInfo.VelopackDisplayVersion} for creating and distributing releases.";

    public static async Task<int> Main(string[] args)
    {
        CliRootCommand platformRootCommand = new CliRootCommand() {
            VerboseOption,
            LegacyConsole,
        };
        platformRootCommand.TreatUnmatchedTokensAsErrors = false;
        ParseResult parseResult = platformRootCommand.Parse(args);
        bool verbose = parseResult.GetValue(VerboseOption);
        bool legacyConsole = parseResult.GetValue(LegacyConsole)
            || Console.IsOutputRedirected
            || Console.IsErrorRedirected;

        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings {
            ApplicationName = "Velopack",
            EnvironmentName = "Production",
            ContentRootPath = Environment.CurrentDirectory,
            Configuration = new ConfigurationManager(),
        });

        builder.Configuration.AddEnvironmentVariables("VPK_");

        var conf = new LoggerConfiguration()
            .MinimumLevel.Is(verbose ? LogEventLevel.Debug : LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

        if (legacyConsole) {
            // spectre can have issues with redirected output, so we disable it.
            Packaging.Progress.IsEnabled = false;
            conf.WriteTo.Console();
        } else {
            Packaging.Progress.IsEnabled = true;
            conf.WriteTo.Spectre();
        }

        Log.Logger = conf.CreateLogger();
        builder.Logging.AddSerilog();

        var host = builder.Build();
        var logFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = logFactory.CreateLogger("vpk");

        Runner = new RunnerFactory(logger, host.Services.GetRequiredService<IConfiguration>());

        CliRootCommand rootCommand = new CliRootCommand(INTRO) {
            VerboseOption,
            LegacyConsole,
        };

        switch (VelopackRuntimeInfo.SystemOs) {
        case RuntimeOs.Windows:
            Add(rootCommand, new WindowsPackCommand(), nameof(ICommandRunner.ExecutePackWindows));
            break;
        case RuntimeOs.OSX:
            Add(rootCommand, new OsxBundleCommand(), nameof(ICommandRunner.ExecuteBundleOsx));
            Add(rootCommand, new OsxPackCommand(), nameof(ICommandRunner.ExecutePackOsx));
            break;
        case RuntimeOs.Linux:
            Add(rootCommand, new LinuxPackCommand(), nameof(ICommandRunner.ExecutePackLinux));
            break;
        default:
            throw new NotSupportedException("Unsupported OS platform: " + VelopackRuntimeInfo.SystemOs.GetOsLongName());
        }

        CliCommand downloadCommand = new CliCommand("download", "Download's the latest release from a remote update source.");
        Add(downloadCommand, new HttpDownloadCommand(), nameof(ICommandRunner.ExecuteHttpDownload));
        Add(downloadCommand, new S3DownloadCommand(), nameof(ICommandRunner.ExecuteS3Download));
        Add(downloadCommand, new GitHubDownloadCommand(), nameof(ICommandRunner.ExecuteGithubDownload));
        rootCommand.Add(downloadCommand);

        var uploadCommand = new CliCommand("upload", "Upload local package(s) to a remote update source.");
        Add(uploadCommand, new S3UploadCommand(), nameof(ICommandRunner.ExecuteS3Upload));
        Add(uploadCommand, new GitHubUploadCommand(), nameof(ICommandRunner.ExecuteGithubUpload));
        rootCommand.Add(uploadCommand);

        var deltaCommand = new CliCommand("delta", "Utilities for creating or applying delta packages.");
        Add(deltaCommand, new DeltaGenCommand(), nameof(ICommandRunner.ExecuteDeltaGen));
        Add(deltaCommand, new DeltaPatchCommand(), nameof(ICommandRunner.ExecuteDeltaPatch));
        rootCommand.Add(deltaCommand);

        var cli = new CliConfiguration(rootCommand);
        return await cli.InvokeAsync(args);
    }

    private static CliCommand Add<T>(CliCommand parent, T command, string commandName)
        where T : BaseCommand
    {
        command.SetAction((ctx, token) => {
            command.SetProperties(ctx);
            return Runner.CreateAndExecuteAsync(commandName, command);
        });
        parent.Subcommands.Add(command);
        return command;
    }
}
