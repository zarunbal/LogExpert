using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using Nuke.GitHub;

using Serilog;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.GitHub.GitHubTasks;

[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main () => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(UpdateBuildNumber = true)]
    readonly Nuke.Common.Tools.GitVersion.GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath BinDirectory => RootDirectory / "bin";

    AbsolutePath OutputDirectory => BinDirectory / Configuration;

    AbsolutePath PackageDirectory => BinDirectory / "Package";

    AbsolutePath ChocolateyDirectory => BinDirectory / "chocolatey";

    AbsolutePath ChocolateyTemplateFiles => RootDirectory / "chocolatey";

    AbsolutePath SftpFileSystemPackagex86 => BinDirectory / "SftpFileSystemx86/";

    AbsolutePath SftpFileSystemPackagex64 => BinDirectory / "SftpFileSystemx64/";

    AbsolutePath StandaloneDirectory => BinDirectory / "Standalone";

    AbsolutePath StandalonePublishDirectory => BinDirectory / "StandalonePublish";

    AbsolutePath SetupDirectory => BinDirectory / "SetupFiles";

    AbsolutePath LicenseDirectory => RootDirectory / "Licenses";

    AbsolutePath InnoSetupScript => SourceDirectory / "setup" / "LogExpertInstaller.iss";

    string SetupCommandLineParameter => $"/dAppVersion=\"{VersionString}\" /O\"{BinDirectory}\" /F\"LogExpert-Setup-{VersionString}\"";

    Version Version
    {
        get
        {
            int patch = 0;

            if (AppVeyor.Instance != null)
            {
                patch = AppVeyor.Instance.BuildNumber;
            }

            return new Version(1, 21, 0, patch);
        }
    }

    [Parameter("Version string")]
    string VersionString => $"{Version.Major}.{Version.Minor}.{Version.Build}";

    [Parameter("Version Information string")]
    //.Branch.{GitVersion.BranchName}.{GitVersion.Sha} removed for testing purpose
    string VersionInformationString => $"{VersionString} {Configuration}";

    [Parameter("Version file string")]
    string VersionFileString => $"{Version.Major}.{Version.Minor}.{Version.Build}";

    [Parameter("Exclude file globs")]
    string[] ExcludeFileGlob => ["**/*.xml", "**/*.XML", "**/*.pdb"];

    [PathVariable("choco.exe")] readonly Tool Chocolatey;

    [Parameter("Exclude directory glob")]
    string[] ExcludeDirectoryGlob => ["**/pluginsx86"];

    [Parameter("My variable", Name = "my_variable")] string MyVariable = null;

    [Parameter("Nuget api key")] string NugetApiKey = null;

    [Parameter("Chocolatey api key")] string ChocolateyApiKey = null;

    [Parameter("GitHub Api key")] string GitHubApiKey = null;

    AbsolutePath[] AppveyorArtifacts =>
    [
        (BinDirectory / $"LogExpert-Setup-{VersionString}.exe"),
        BinDirectory / $"LogExpert-CI-{VersionString}.zip",
        BinDirectory / $"LogExpert.{VersionString}.zip",
        BinDirectory / $"LogExpert-Standalone.{VersionString}.zip",
        BinDirectory / $"LogExpert.ColumnizerLib.{VersionString}.nupkg",
        BinDirectory / $"SftpFileSystem.x64.{VersionString}.zip",
        BinDirectory / $"SftpFileSystem.x86.{VersionString}.zip",
        ChocolateyDirectory / $"logexpert.{VersionString}.nupkg"
    ];

    protected override void OnBuildInitialized ()
    {
        SetVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");

        base.OnBuildInitialized();
    }

    Target Clean => _ => _
        .Before(Compile, Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(dir => dir.DeleteDirectory());

            if (BinDirectory.DirectoryExists())
            {
                BinDirectory.GlobFiles("*", "*.*", ".*").ForEach(file => file.DeleteFile());
                BinDirectory.GlobDirectories("*").ForEach(dir => dir.DeleteDirectory());

                BinDirectory.DeleteDirectory();

                BinDirectory.CreateOrCleanDirectory();
            }
        });

    Target CleanPackage => _ => _
        .Before(Compile, Restore)
        .OnlyWhenDynamic(() => BinDirectory.DirectoryExists())
        .Executes(() =>
        {
            BinDirectory.GlobFiles("**/*.zip", "**/*.nupkg").ForEach(file => file.DeleteFile());

            if (PackageDirectory.DirectoryExists())
            {
                PackageDirectory.DeleteDirectory();

                PackageDirectory.CreateOrCleanDirectory();
            }

            if (ChocolateyDirectory.DirectoryExists())
            {
                ChocolateyDirectory.DeleteDirectory();

                ChocolateyDirectory.CreateOrCleanDirectory();
            }
        });

    Target Restore => _ => _
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

            Log.Information($"Version: '{VersionString}'");

            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetTargets("Rebuild")
                .SetAssemblyVersion(VersionString)
                .SetInformationalVersion(VersionInformationString)
                .SetTargetPlatform(MSBuildTargetPlatform.MSIL)
                .SetConfiguration(Configuration)
                .SetMaxCpuCount(Environment.ProcessorCount));
        });

    AbsolutePath PluginHashGeneratorProject => SourceDirectory / "PluginHashGenerator.Tool" / "PluginHashGenerator.Tool.csproj";

    AbsolutePath PluginHashGeneratedFile => SourceDirectory / "PluginRegistry" / "PluginHashGenerator.Generated.cs";

    Target GeneratePluginHashes => _ => _
        .After(Compile)
        .OnlyWhenStatic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            var pluginsDir = OutputDirectory / "plugins";
            var pluginsx86Dir = OutputDirectory / "pluginsx86";

            // Check if any plugins exist
            if (!pluginsDir.DirectoryExists() && !pluginsx86Dir.DirectoryExists())
            {
                Log.Warning("No plugins directories found. Skipping plugin hash generation.");
                return;
            }

            Log.Information("Generating plugin hashes...");
            Log.Information($"  Output Path: {OutputDirectory}");
            Log.Information($"  Target File: {PluginHashGeneratedFile}");
            Log.Information($"  Configuration: {Configuration}");

            try
            {
                DotNetRun(s => s
                    .SetProjectFile(PluginHashGeneratorProject)
                    .SetApplicationArguments([OutputDirectory, PluginHashGeneratedFile, Configuration])
                    .SetProcessWorkingDirectory(RootDirectory));

                Log.Information("Plugin hashes generated successfully");

                // Rebuild PluginRegistry project to include the generated file
                // IMPORTANT: Set OutputPath to match the main build output directory
                Log.Information("Rebuilding PluginRegistry to include generated hashes...");
                MSBuild(s => s
                    .SetTargetPath(SourceDirectory / "PluginRegistry" / "LogExpert.PluginRegistry.csproj")
                    .SetTargets("Build")
                    .SetConfiguration(Configuration)
                    .SetProperty("OutputPath", OutputDirectory)
                    .SetMaxCpuCount(Environment.ProcessorCount));

                Log.Information("PluginRegistry rebuilt successfully to {OutputDir}", OutputDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to generate plugin hashes");
                throw;
            }
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(c => c
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .CombineWith(SourceDirectory.GlobFiles("**/*Tests.csproj"), (settings, path) =>
                        settings.SetProjectFile(path)), degreeOfParallelism: 4, completeOnFailure: true);
        });

    Target PrepareChocolateyTemplates => _ => _
        .DependsOn(CleanPackage)
        .Executes(() =>
        {
            ChocolateyTemplateFiles.Copy(ChocolateyDirectory, ExistsPolicy.MergeAndOverwriteIfNewer);

            ChocolateyDirectory.GlobFiles("**/*.template").ForEach(path => TransformTemplateFile(path, true));
        });

    Target CopyOutputForChocolatey => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {

            OutputDirectory.Copy(ChocolateyDirectory / "tools", ExistsPolicy.MergeAndOverwriteIfNewer);
            ChocolateyDirectory.GlobFiles(ExcludeFileGlob).ForEach(file => file.DeleteFile());
            ChocolateyDirectory.GlobDirectories(ExcludeDirectoryGlob).ForEach(dir => dir.DeleteDirectory());
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
            OutputDirectory.Copy(PackageDirectory, ExistsPolicy.MergeAndOverwriteIfNewer);
            PackageDirectory.GlobFiles(ExcludeFileGlob).ForEach(file => file.DeleteFile());

            PackageDirectory.GlobDirectories(ExcludeDirectoryGlob).ForEach(dir => dir.DeleteDirectory());

            CompressionExtensions.ZipTo(PackageDirectory, BinDirectory / $"LogExpert.{VersionString}.zip");
        });

    Target CreateStandalonePackage => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {
            Log.Information("Creating standalone (self-contained) package...");

            // Publish the main project as self-contained
            DotNetPublish(s => s
                .SetProject(SourceDirectory / "LogExpert" / "LogExpert.csproj")
                .SetConfiguration(Configuration)
                .SetOutput(StandalonePublishDirectory)
                .SetSelfContained(true)
                .SetRuntime("win-x64")
                .SetProperty("PublishSingleFile", "false")
                .SetProperty("IncludeNativeLibrariesForSelfExtract", "true"));

            // Copy the self-contained publish output to the standalone directory
            StandalonePublishDirectory.Copy(StandaloneDirectory, ExistsPolicy.MergeAndOverwriteIfNewer);

            // Copy plugins from the regular build output
            var pluginsDir = OutputDirectory / "plugins";
            if (pluginsDir.DirectoryExists())
            {
                pluginsDir.Copy(StandaloneDirectory / "plugins", ExistsPolicy.MergeAndOverwriteIfNewer);
            }

            // Clean up excluded files
            StandaloneDirectory.GlobFiles(ExcludeFileGlob).ForEach(file => file.DeleteFile());
            StandaloneDirectory.GlobDirectories(ExcludeDirectoryGlob).ForEach(dir => dir.DeleteDirectory());

            // Create portable mode directory and configuration file
            var portableDir = StandaloneDirectory / "portable";
            portableDir.CreateDirectory();
            (portableDir / "portableMode.json").WriteAllText("{}");

            // Copy licenses
            if (LicenseDirectory.DirectoryExists())
            {
                LicenseDirectory.Copy(StandaloneDirectory / "Licenses", ExistsPolicy.MergeAndOverwriteIfNewer);
            }

            // Create the standalone ZIP package
            CompressionExtensions.ZipTo(StandaloneDirectory, BinDirectory / $"LogExpert-Standalone.{VersionString}.zip");

            Log.Information($"Standalone package created: LogExpert-Standalone.{VersionString}.zip");
        });

    Target ChangeVersionNumber => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            Log.Information($"AssemblyVersion {VersionString}\r\nAssemblyFileVersion {VersionFileString}\r\nAssemblyInformationalVersion {VersionInformationString}");

            AbsolutePath assemblyInfo = SourceDirectory / "Solution Items" / "AssemblyInfo.cs";

            string text = assemblyInfo.ReadAllText();
            Regex configurationRegex = AssemblyConfiguration();
            Regex assemblyVersionRegex = AssemblyVersion();
            Regex assemblyFileVersionRegex = AssemblyFileVersion();
            Regex assemblyInformationalVersionRegex = AssemblyInformationalVersion();

            text = configurationRegex.Replace(text, (match) => ReplaceVersionMatch(match, $"\"{Configuration}\""));
            text = assemblyVersionRegex.Replace(text, (match) => ReplaceVersionMatch(match, VersionString));
            text = assemblyFileVersionRegex.Replace(text, (match) => ReplaceVersionMatch(match, VersionFileString));
            text = assemblyInformationalVersionRegex.Replace(text, (match) => ReplaceVersionMatch(match, VersionInformationString));

            Log.Verbose("Content of AssemblyInfo.cs file");
            Log.Verbose(text);
            Log.Verbose("End of Content");

            assemblyInfo.WriteAllText(text);

            SourceDirectory.GlobFiles("**sftp-plugin/*.cs").ForEach(file =>
            {
                if (string.IsNullOrWhiteSpace(MyVariable))
                {
                    return;
                }

                string fileText = file.ReadAllText();

                Regex reg = SFTPPlugin();

                if (reg.IsMatch(fileText))
                {
                    fileText = reg.Replace(fileText, MyVariable);
                    file.WriteAllText(fileText);
                }
            });
        });

    Target PackageSftpFileSystem => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {
            string[] files = ["SftpFileSystem.dll", "Renci.SshNet.dll"];

            OutputDirectory.GlobFiles([.. files.Select(a => $"plugins/{a}")]).ForEach(file => file.CopyToDirectory(SftpFileSystemPackagex64, ExistsPolicy.FileOverwrite));
            OutputDirectory.GlobFiles([.. files.Select(a => $"pluginsx86/{a}")]).ForEach(file => file.CopyToDirectory(SftpFileSystemPackagex86, ExistsPolicy.FileOverwrite));

            CompressionExtensions.ZipTo(SftpFileSystemPackagex64, BinDirectory / $"SftpFileSystem.x64.{VersionString}.zip");
            CompressionExtensions.ZipTo(SftpFileSystemPackagex86, BinDirectory / $"SftpFileSystem.x86.{VersionString}.zip");
        });

    Target ColumnizerLibCreate => _ => _.DependsOn(Compile, Test)
        .Executes(() =>
        {
            var columnizerFolder = SourceDirectory / "ColumnizerLib";
            DotNetPack(s => s
                .SetProject(columnizerFolder / "ColumnizerLib.csproj")
                .SetConfiguration(Configuration)
                .SetOutputDirectory(BinDirectory)
                .SetVersion(VersionString));
        });

    Target Pack => _ => _
        .DependsOn(BuildChocolateyPackage, CreatePackage, CreateStandalonePackage, PackageSftpFileSystem, ColumnizerLibCreate, CopyLicenses, GeneratePluginHashes, CreateSetup);

    Target CopyFilesForSetup => _ => _
        .DependsOn(Compile)
        .After(Test)
        .Executes(() =>
        {
            OutputDirectory.Copy(SetupDirectory, ExistsPolicy.MergeAndOverwriteIfNewer);
            SetupDirectory.GlobFiles(ExcludeFileGlob).ForEach(file => file.DeleteFile());

            SetupDirectory.GlobDirectories(ExcludeDirectoryGlob).ForEach(dir => dir.DeleteDirectory());
        });

    Target CreateSetup => _ => _
        .DependsOn(CopyFilesForSetup, ChangeVersionNumber, Compile)
        .Before(Publish)
        .After(GeneratePluginHashes)
        .OnlyWhenStatic(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            var publishCombinations =
                from framework in new[] { SpecialFolder(SpecialFolders.ProgramFilesX86), SpecialFolder(SpecialFolders.LocalApplicationData) / "Programs" }
                from version in new[] { "5", "6" }
                select framework / $"Inno Setup {version}" / "iscc.exe";
            bool executed = false;
            foreach (var setupCombinations in publishCombinations)
            {
                if (!setupCombinations.FileExists())
                {
                    //Search for next combination
                    continue;
                }

                ExecuteInnoSetup(setupCombinations);
                executed = true;
                break;
            }

            if (!executed)
            {
                Assert.True(true, "Inno setup was not found");
            }
        });

    Target PublishColumnizerNuget => _ => _
        .DependsOn(ColumnizerLibCreate)
    //.DependsOn(ColumnizerLibCreateNuget)
        .Requires(() => NugetApiKey)
        //.OnlyWhenDynamic(() => GitVersion.BranchName.Equals("master") || GitVersion.BranchName.Equals("origin/master"))
        .Executes(() =>
        {
            BinDirectory.GlobFiles("**/LogExpert.ColumnizerLib.*.nupkg").ForEach(file =>
            {
                Log.Debug($"Publish nuget {file}");

                NuGetTasks.NuGetPush(s =>
                {
                    s = s.SetApiKey(NugetApiKey)
                        .SetSource("https://api.nuget.org/v3/index.json")
                        .SetTargetPath(file);

                    return s;
                });
            });
        });

    Target PublishChocolatey => _ => _
        .DependsOn(BuildChocolateyPackage)
        .Requires(() => ChocolateyApiKey)
        .Executes(() =>
        {
            ChocolateyDirectory.GlobFiles("**/*.nupkg").ForEach(file =>
            {
                Log.Debug($"Publish chocolatey package {file}");

                Chocolatey($"push {file} --key {ChocolateyApiKey} --source https://push.chocolatey.org/", WorkingDirectory = ChocolateyDirectory);
            });
        });

    Target PublishGithub => _ => _
        .DependsOn(Pack)
        .Requires(() => GitHubApiKey)
        .Executes(() =>
        {
            var (gitHubOwner, repositoryName) = GetGitHubRepositoryInfo(GitRepository);

            Task task = PublishRelease(s => s
                .SetArtifactPaths([.. BinDirectory.GlobFiles("**/*.zip", "**/*.nupkg", "**/LogExpert-Setup*.exe").Select(a => a.ToString())])
                .SetCommitSha(GitVersion.Sha)
                .SetReleaseNotes($"# Changes\r\n" +
                                 $"# Bugfixes\r\n" +
                                 $"# Contributors\r\n" +
                                 $"Thanks to the contributors!\r\n" +
                                 $"# Infos\r\n" +
                                 $"It might be necessary to unblock the Executables / Dlls to get everything working, especially Plugins (see #55, #13, #8).")
                .SetRepositoryName(repositoryName)
                .SetRepositoryOwner(gitHubOwner)
                .SetTag($"v{VersionString}")
                .SetToken(GitHubApiKey)
                .SetName(VersionString)
            );

            task.Wait();
        });

    Target Publish => _ => _
        .DependsOn(PublishChocolatey, PublishColumnizerNuget, PublishGithub);

    Target PublishToAppveyor => _ => _
        .After(Publish, CreateSetup)
        .OnlyWhenDynamic(() => AppVeyor.Instance != null)
        .Executes(() =>
        {
            CompressionExtensions.ZipTo(BinDirectory / Configuration, BinDirectory / $"LogExpert-CI-{VersionString}.zip");

            AppveyorArtifacts.ForEach((artifact) =>
            {
                var proc = new Process();
                proc.StartInfo = new ProcessStartInfo("appveyor", $"PushArtifact \"{artifact}\"");
                if (!proc.Start())
                {
                    Assert.True(true, "Failed to start appveyor pushartifact");
                }

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    Assert.True(true, $"Exit code is {proc.ExitCode}");
                }
            });
        });

    Target CleanupAppDataLogExpert => _ => _
        .Executes(() =>
        {
            AbsolutePath logExpertApplicationData = SpecialFolder(SpecialFolders.ApplicationData) / "LogExpert";

            var info = new DirectoryInfo(logExpertApplicationData);
            info.GetDirectories().ForEach(a => a.Delete(true));
            logExpertApplicationData.DeleteDirectory();
        });

    Target CleanupDocumentsLogExpert => _ => _
        .Executes(() =>
        {
            AbsolutePath logExpertDocuments = SpecialFolder(SpecialFolders.UserProfile) / "Documents" / "LogExpert";

            var info = new DirectoryInfo(logExpertDocuments);
            info.GetDirectories().ForEach(a => a.Delete(true));
            logExpertDocuments.DeleteDirectory();
        });

    Target CopyLicenses => _ => _
    .DependsOn(Compile)
    .Executes(() =>
    {
        if (LicenseDirectory.DirectoryExists())
        {
            Log.Information("Copying license files to output directory");

            // Copy to main output directory
            LicenseDirectory.Copy(OutputDirectory / "Licenses", ExistsPolicy.MergeAndOverwriteIfNewer);

            Log.Information($"Licenses copied to {OutputDirectory / "Licenses"}");
        }
        else
        {
            Log.Warning($"License directory not found at: {LicenseDirectory}");
        }
    });

    private void ExecuteInnoSetup (AbsolutePath innoPath)
    {
        Process proc = new();

        Log.Information($"Start '{innoPath}' {SetupCommandLineParameter} \"{InnoSetupScript}\"");

        proc.StartInfo = new ProcessStartInfo(innoPath, $"{SetupCommandLineParameter} \"{InnoSetupScript}\"");
        if (!proc.Start())
        {
            Assert.True(true, $"Failed to start {innoPath} with \"{SetupCommandLineParameter}\" \"{InnoSetupScript}\"");
        }

        proc.WaitForExit();

        Log.Information($"Executed '{innoPath}' with exit code {proc.ExitCode}");

        if (proc.ExitCode != 0)
        {
            Assert.True(true, $"Error during execution of {innoPath}, exitcode {proc.ExitCode}");
        }
    }

    private string ReplaceVersionMatch (Match match, string replacement)
    {
        return $"{match.Groups[1]}{replacement}{match.Groups[3]}";
    }

    private void TransformTemplateFile (AbsolutePath path, bool deleteTemplate)
    {
        string text = path.ReadAllText();
        text = text.Replace("##version##", VersionString);

        AbsolutePath template = $"{TemplateRegex().Replace(path, "")}";
        template.WriteAllText(text);
        if (deleteTemplate)
        {
            path.DeleteFile();
        }
    }

    [GeneratedRegex(@"(\[assembly: AssemblyInformationalVersion\("")([^""]*)(""\)\])")]
    private static partial Regex AssemblyInformationalVersion ();

    [GeneratedRegex(@"(\[assembly: AssemblyVersion\("")([^""]*)(""\)\])")]
    private static partial Regex AssemblyVersion ();

    [GeneratedRegex(@"(\[assembly: AssemblyConfiguration\()(""[^""]*"")(\)\])")]
    private static partial Regex AssemblyConfiguration ();

    [GeneratedRegex(@"(\[assembly: AssemblyFileVersion\("")([^""]*)(""\)\])")]
    private static partial Regex AssemblyFileVersion ();

    [GeneratedRegex(@"\w\w{2}[_]p?[tso]?[erzliasx]+[_rhe]{5}", RegexOptions.IgnoreCase, "en-GB")]
    private static partial Regex SFTPPlugin ();

    [GeneratedRegex("\\.template$")]
    private static partial Regex TemplateRegex ();
}
