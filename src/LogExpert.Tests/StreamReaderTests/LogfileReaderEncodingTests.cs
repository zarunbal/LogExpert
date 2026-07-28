using System.Text;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Classes.Log.ProgressReporters;
using LogExpert.Core.Entities;
using LogExpert.Core.Enums;
using LogExpert.Core.Helpers;

using NUnit.Framework;

namespace LogExpert.Tests.StreamReaderTests;

/// <summary>
/// Pins the encoding a <see cref="LogfileReader"/> ends up using — the value that reaches the grid,
/// the Encoding menu and the persisted .lxp.
/// </summary>
[TestFixture]
public class LogfileReaderEncodingTests
{
    private const string EURO_LINE = "Euro: €";

    private string _tempFile = null!;

    [SetUp]
    public void Setup ()
    {
        _tempFile = Path.GetTempFileName();
        _ = PluginRegistry.PluginRegistry.Create(Path.GetDirectoryName(_tempFile)!, 500);
    }

    [TearDown]
    public void Cleanup ()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Test]
    public void ReadFiles_BomlessFile_UsesConfiguredDefaultEncoding ()
    {
        var configuredEncoding = EncodingRegistry.GetEncoding(1252);
        File.WriteAllText(_tempFile, EURO_LINE + "\n", configuredEncoding);

        using var reader = CreateReader(new EncodingOptions { DefaultEncoding = configuredEncoding });
        reader.ReadFiles();

        Assert.Multiple(() =>
        {
            Assert.That(reader.CurrentEncoding.CodePage, Is.EqualTo(configuredEncoding.CodePage));
            Assert.That(LineText(reader, 0), Is.EqualTo(EURO_LINE));
        });
    }

    [Test]
    public void ReadFiles_PreamblePresent_OverridesConfiguredDefaultEncoding ()
    {
        File.WriteAllText(_tempFile, EURO_LINE + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        using var reader = CreateReader(new EncodingOptions { DefaultEncoding = EncodingRegistry.GetEncoding(1252) });
        reader.ReadFiles();

        Assert.Multiple(() =>
        {
            Assert.That(reader.CurrentEncoding.WebName, Is.EqualTo(Encoding.UTF8.WebName));
            Assert.That(LineText(reader, 0), Is.EqualTo(EURO_LINE));
        });
    }

    /// <summary>
    /// Deliberate precedence: an explicit <see cref="EncodingOptions.Encoding"/> is either a choice
    /// from the Encoding menu or one persisted per file in the .lxp, so it outranks the file's BOM.
    /// Only <see cref="EncodingOptions.DefaultEncoding"/> (the Preferences value) yields to a BOM.
    /// </summary>
    [Test]
    public void ReadFiles_ExplicitEncoding_TakesPrecedenceOverPreamble ()
    {
        var explicitEncoding = EncodingRegistry.GetEncoding(1252);
        File.WriteAllText(_tempFile, EURO_LINE + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        using var reader = CreateReader(new EncodingOptions { Encoding = explicitEncoding });
        reader.ReadFiles();

        Assert.That(reader.CurrentEncoding.CodePage, Is.EqualTo(explicitEncoding.CodePage));
    }

    /// <summary>
    /// The last link of the chain: nothing explicit, no BOM, no Preferences default — the machine
    /// default is what remains.
    /// </summary>
    [Test]
    public void ReadFiles_NoExplicitEncodingNoPreambleNoConfiguredDefault_UsesTheMachineDefault ()
    {
        File.WriteAllText(_tempFile, "plain ascii\n", Encoding.ASCII);

        using var reader = CreateReader(new EncodingOptions());
        reader.ReadFiles();

        Assert.That(reader.CurrentEncoding.CodePage, Is.EqualTo(Encoding.Default.CodePage));
    }

    [Test]
    public void ChangeEncoding_SwitchesTheReportedEncoding ()
    {
        File.WriteAllText(_tempFile, "plain ascii\n", Encoding.ASCII);

        using var reader = CreateReader(new EncodingOptions { Encoding = Encoding.ASCII });
        reader.ReadFiles();

        reader.ChangeEncoding(Encoding.Latin1);

        Assert.That(reader.CurrentEncoding.CodePage, Is.EqualTo(Encoding.Latin1.CodePage));
    }

    private static string? LineText (LogfileReader reader, int lineNum)
    {
        return reader.GetLogLineMemory(lineNum)?.FullLine.Span.ToString();
    }

    private LogfileReader CreateReader (EncodingOptions encodingOptions)
    {
        return new LogfileReader(
            _tempFile,
            encodingOptions,
            multiFile: false,
            bufferCount: 100,
            linesPerBuffer: 500,
            new MultiFileOptions(),
            ReaderType.SystemDirect,
            PluginRegistry.PluginRegistry.Instance,
            maximumLineLength: 500,
            progressReporter: NullProgressReporter.Instance);
    }
}
