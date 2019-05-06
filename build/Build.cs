using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Build.Execution;
using Nuke.Common;
using Nuke.Common.BuildServers;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.TextTasks;
using static Nuke.Common.IO.CompressionTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath BinDirectory => RootDirectory / "bin";

    AbsolutePath OutputDirectory => BinDirectory / Configuration;

    AbsolutePath PackageDirectory => BinDirectory / "Package";

    AbsolutePath ChocolateyDirectory => BinDirectory / "chocolatey";

    AbsolutePath ChocolateyTemplateFiles => RootDirectory / "chocolatey";

    Version Version
    {
        get
        {
            int patch = 0;

            if (AppVeyor.Instance != null)
            {
                patch = AppVeyor.Instance.BuildNumber;
            }

            return new Version(1, 6, patch);
        }
    }

    [Parameter("Version string")]
    string VersionString => $"{Version.Major}.{Version.Minor}.{Version.Build}";

    [Parameter("Version Information string")]
    string VersionInformationString => $"{VersionString}.{GitRepository.Head}";

    [Parameter("Exclude file globs")]
    string[] ExcludeFileGlob => new[] {"**/*.xml", "**/*.XML", "**/*.pdb", "**/ChilkatDotNet4.dll", "**/SftpFileSystem.dll"};

    [Parameter("My signing key", Name = "my_signing_key")] string MySigningKey = null;

    [PathExecutable("choco.exe")] readonly Tool Chocolatey;

    Target Initialize => _ => _
        .Executes(() =>
        {
            throw new Exception("test ex");
            SetVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");

            if (!string.IsNullOrWhiteSpace(MySigningKey))
            {
                Logger.Info("Replace signing key");
                byte[] bytes = Convert.FromBase64String(MySigningKey);
                AbsolutePath signingKey = SourceDirectory / "Solution Items" / "Key.snk";
                DeleteFile(signingKey);
                WriteAllBytes(signingKey, bytes);
            }
        });

    Target Clean => _ => _
        .DependsOn(Initialize)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

            if (DirectoryExists(BinDirectory))
            {
                BinDirectory.GlobFiles("**/*.*").ForEach(DeleteFile);
                BinDirectory.GlobDirectories("*").ForEach(DeleteDirectory);

                DeleteDirectory(BinDirectory);

                EnsureCleanDirectory(BinDirectory);
            }
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Restore"));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            string version = $"{Version.Major}.{Version.Minor}.{Version.Build}";

            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetAssemblyVersion(VersionString)
                .SetInformationalVersion(VersionInformationString)
                .SetTargetPlatform(MSBuildTargetPlatform.MSIL)
                .SetConfiguration(Configuration)
                .SetMaxCpuCount(Environment.ProcessorCount));
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            SourceDirectory.GlobFiles("**/*Tests.csproj").ForEach(path =>
            {
                DotNetTest(c =>
                {
                    c = c.SetProjectFile(path)
                        .SetConfiguration(Configuration)
                        .EnableNoBuild();
                    return c;
                });
            });
        });

    Target PrepareChocolateyTemplates => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            CopyDirectoryRecursively(ChocolateyTemplateFiles, ChocolateyDirectory, DirectoryExistsPolicy.Merge);

            ChocolateyDirectory.GlobFiles("**/*.template").ForEach(path =>
            {
                string text = ReadAllText(path);
                text = text.Replace("##version##", VersionString);

                WriteAllText($"{Regex.Replace(path, "\\.template$", "")}", text);
                DeleteFile(path);
            });
        });

    Target CopyOutputForChocolatey => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {
            CopyDirectoryRecursively(OutputDirectory, ChocolateyDirectory);
            ChocolateyDirectory.GlobFiles(ExcludeFileGlob).ForEach(DeleteFile);
        });

    Target BuildChocolateyPackage => _ => _
        .DependsOn(PrepareChocolateyTemplates, CopyOutputForChocolatey)
        .Executes(() =>
        {
            Chocolatey("pack", WorkingDirectory = ChocolateyDirectory);
        });

    Target CreatePackage => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {
            CopyDirectoryRecursively(OutputDirectory, PackageDirectory, DirectoryExistsPolicy.Merge);
            PackageDirectory.GlobFiles(ExcludeFileGlob).ForEach(DeleteFile);

            Compress(PackageDirectory, BinDirectory / $"LogExpert.{VersionString}.zip");
        });

    Target Pack => _ => _
        .DependsOn(BuildChocolateyPackage, CreatePackage);
}