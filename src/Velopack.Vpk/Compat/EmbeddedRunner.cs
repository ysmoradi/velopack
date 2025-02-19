﻿using System.Runtime.Versioning;
using Velopack.Deployment;
using Velopack.Packaging.Commands;
using Velopack.Packaging.Unix.Commands;
using Velopack.Packaging.Windows.Commands;
using Velopack.Vpk.Commands;

namespace Velopack.Vpk.Compat;

public class EmbeddedRunner : ICommandRunner
{
    private readonly ILogger _logger;

    public EmbeddedRunner(ILogger logger)
    {
        _logger = logger;
    }

    [SupportedOSPlatform("osx")]
    public virtual Task ExecuteBundleOsx(OsxBundleCommand command)
    {
        var options = new OsxBundleOptions {
            BundleId = command.BundleId,
            PackAuthors = command.PackAuthors,
            EntryExecutableName = command.EntryExecutableName,
            Icon = command.Icon,
            PackDirectory = command.PackDirectory,
            PackId = command.PackId,
            PackTitle = command.PackTitle,
            PackVersion = command.PackVersion,
            ReleaseDir = command.GetReleaseDirectory(),
            InfoPlistPath = command.InfoPlistPath,
        };
        new OsxBundleCommandRunner(_logger).Bundle(options);
        return Task.CompletedTask;
    }

    [SupportedOSPlatform("osx")]
    public virtual Task ExecutePackOsx(OsxPackCommand command)
    {
        var options = new OsxPackOptions {
            BundleId = command.BundleId,
            PackAuthors = command.PackAuthors,
            EntryExecutableName = command.EntryExecutableName,
            Icon = command.Icon,
            PackDirectory = command.PackDirectory,
            PackId = command.PackId,
            PackTitle = command.PackTitle,
            Channel = command.Channel,
            PackVersion = command.PackVersion,
            TargetRuntime = command.GetRid(),
            ReleaseDir = command.GetReleaseDirectory(),
            DeltaMode = command.Delta,
            NoPackage = command.NoPackage,
            NotaryProfile = command.NotaryProfile,
            PackageConclusion = command.PackageConclusion,
            PackageLicense = command.PackageLicense,
            PackageReadme = command.PackageReadme,
            PackageWelcome = command.PackageWelcome,
            ReleaseNotes = command.ReleaseNotes,
            SigningAppIdentity = command.SigningAppIdentity,
            SigningEntitlements = command.SigningEntitlements,
            SigningInstallIdentity = command.SigningInstallIdentity,
            InfoPlistPath = command.InfoPlistPath,
        };
        return new OsxPackCommandRunner(_logger).Run(options);
    }

    [SupportedOSPlatform("windows")]
    public virtual Task ExecutePackWindows(WindowsPackCommand command)
    {
        var options = new WindowsPackOptions {
            TargetRuntime = command.GetRid(),
            ReleaseDir = command.GetReleaseDirectory(),
            Package = command.Package,
            Icon = command.Icon,
            DeltaMode = command.Delta,
            SignParameters = command.SignParameters,
            EntryExecutableName = command.EntryExecutableName,
            PackAuthors = command.PackAuthors,
            PackDirectory = command.PackDirectory,
            Channel = command.Channel,
            PackId = command.PackId,
            PackTitle = command.PackTitle,
            PackVersion = command.PackVersion,
            ReleaseNotes = command.ReleaseNotes,
            Runtimes = command.Runtimes,
            SignParallel = command.SignParallel,
            SignSkipDll = command.SignSkipDll,
            SignTemplate = command.SignTemplate,
            SplashImage = command.SplashImage,
        };
        return new WindowsPackCommandRunner(_logger).Run(options);
    }

    [SupportedOSPlatform("linux")]
    public virtual Task ExecutePackLinux(LinuxPackCommand command)
    {
        var options = new LinuxPackOptions {
            TargetRuntime = command.GetRid(),
            ReleaseDir = command.GetReleaseDirectory(),
            Icon = command.Icon,
            DeltaMode = command.Delta,
            EntryExecutableName = command.EntryExecutableName,
            PackAuthors = command.PackAuthors,
            PackDirectory = command.PackDirectory,
            Channel = command.Channel,
            PackId = command.PackId,
            PackTitle = command.PackTitle,
            PackVersion = command.PackVersion,
            ReleaseNotes = command.ReleaseNotes,
            PackIsAppDir = command.PackIsAppDir,
        };
        return new LinuxPackCommandRunner(_logger).Run(options);
    }

    //public virtual Task ExecuteReleasifyWindows(WindowsReleasifyCommand command)
    //{
    //    var options = new WindowsReleasifyOptions {
    //        TargetRuntime = command.GetRid(),
    //        ReleaseDir = command.GetReleaseDirectory(),
    //        Package = command.Package,
    //        Icon = command.Icon,
    //        DeltaMode = command.Delta,
    //        SignParameters = command.SignParameters,
    //        EntryExecutableName = command.EntryExecutableName,
    //        Runtimes = command.Runtimes,
    //        Channel = command.Channel,
    //        SignParallel = command.SignParallel,
    //        SignSkipDll = command.SignSkipDll,
    //        SignTemplate = command.SignTemplate,
    //        SplashImage = command.SplashImage,
    //    };
    //    new WindowsPackCommandRunner(_logger).Releasify(options);
    //    return Task.CompletedTask;
    //}

    public virtual Task ExecuteGithubDownload(GitHubDownloadCommand command)
    {
        var options = new GitHubDownloadOptions {
            Prerelease = command.Pre,
            ReleaseDir = command.GetReleaseDirectory(),
            RepoUrl = command.RepoUrl,
            Token = command.Token,
            Channel = command.Channel,
        };
        return new GitHubRepository(_logger).DownloadLatestFullPackageAsync(options);
    }

    public virtual Task ExecuteGithubUpload(GitHubUploadCommand command)
    {
        var options = new GitHubUploadOptions {
            Prerelease = command.Pre,
            ReleaseDir = command.GetReleaseDirectory(),
            RepoUrl = command.RepoUrl,
            Token = command.Token,
            Publish = command.Publish,
            ReleaseName = command.ReleaseName,
            Channel = command.Channel,
            Merge = command.Merge,
            TagName = command.TagName,
        };
        return new GitHubRepository(_logger).UploadMissingAssetsAsync(options);
    }

    public virtual Task ExecuteHttpDownload(HttpDownloadCommand command)
    {
        var options = new HttpDownloadOptions {
            ReleaseDir = command.GetReleaseDirectory(),
            Url = command.Url,
            Channel = command.Channel,
        };
        return new HttpRepository(_logger).DownloadLatestFullPackageAsync(options);
    }

    public virtual Task ExecuteS3Download(S3DownloadCommand command)
    {
        var options = new S3DownloadOptions {
            Bucket = command.Bucket,
            Endpoint = command.Endpoint,
            Session = command.Session,
            KeyId = command.KeyId,
            Region = command.Region,
            ReleaseDir = command.GetReleaseDirectory(),
            Secret = command.Secret,
            Channel = command.Channel,
        };
        return new S3Repository(_logger).DownloadLatestFullPackageAsync(options);
    }

    public virtual Task ExecuteS3Upload(S3UploadCommand command)
    {
        var options = new S3UploadOptions {
            Bucket = command.Bucket,
            Endpoint = command.Endpoint,
            KeyId = command.KeyId,
            Session = command.Session,
            Region = command.Region,
            ReleaseDir = command.GetReleaseDirectory(),
            Secret = command.Secret,
            Channel = command.Channel,
        };
        return new S3Repository(_logger).UploadMissingAssetsAsync(options);
    }

    public virtual Task ExecuteDeltaGen(DeltaGenCommand command)
    {
        var options = new DeltaGenOptions {
            BasePackage = command.BasePackage,
            NewPackage = command.NewPackage,
            OutputFile = command.OutputFile,
            DeltaMode = command.Delta,
        };
        return new DeltaGenCommandRunner(_logger).Run(options);
    }

    public virtual Task ExecuteDeltaPatch(DeltaPatchCommand command)
    {
        var options = new DeltaPatchOptions {
            BasePackage = command.BasePackage,
            PatchFiles = command.PatchFiles,
            OutputFile = command.OutputFile,
        };
        return new DeltaPatchCommandRunner(_logger).Run(options);
    }
}
