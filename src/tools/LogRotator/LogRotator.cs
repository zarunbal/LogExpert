using System.Text;

const string baseDir = "logs";
const string baseName = "engine.log";
var safeBaseName = Path.GetFileName(baseName);
const int maxBackups = 6;
const int linesPerFile = 50;
// When true, every generated line contains a rotating selection of C0 control
// characters (0x00..0x1F and 0x7F, excluding LF/CR which would break the line
// into multiple records). Useful for verifying the control-character display
// substitution feature in LogExpert. Toggle here to compare runs.
var includeControlChars = true;

// All displayable control codepoints minus LF (0x0A) and CR (0x0D).
var controlChars = Enumerable.Range(0x00, 0x20)
    .Where(c => c is not 0x0A and not 0x0D)
    .Append(0x7F)
    .Select(c => (char)c)
    .ToArray();

Directory.CreateDirectory(baseDir);

// Create initial set of files
Console.WriteLine($"Creating initial files in '{Path.GetFullPath(baseDir)}'...");
WriteLogFile(Path.Join(baseDir, safeBaseName), 0);

for (var i = 1; i <= maxBackups; i++)
{
    WriteLogFile(Path.Join(baseDir, $"{safeBaseName}.{i}"), i);
}

PrintFiles();
Console.WriteLine();
Console.WriteLine("Open the 'engine.log' file in LogExpert with MultiFile enabled (pattern: *$J(.))");
Console.WriteLine($"Control characters in output: {(includeControlChars ? "ENABLED" : "disabled")}");
Console.WriteLine("Press ENTER to perform a rotation (with oldest file deletion).");
Console.WriteLine("Press A to append a single live line (no rotation) for tail testing.");
Console.WriteLine("Press Q to quit.");

var rotationCount = 0;

while (true)
{
    var key = Console.ReadKey(true);

    if (key.Key == ConsoleKey.Q)
    {
        break;
    }

    if (key.Key == ConsoleKey.A)
    {
        AppendLiveLine(Path.Join(baseDir, safeBaseName));
        continue;
    }

    if (key.Key != ConsoleKey.Enter)
    {
        continue;
    }

    rotationCount++;
    Console.WriteLine($"\n--- Rotation #{rotationCount} ---");

    // Delete the oldest file (simulates maxBackups limit)
    var oldest = Path.Join(baseDir, $"{safeBaseName}.{maxBackups}");

    if (File.Exists(oldest))
    {
        File.Delete(oldest);
        Console.WriteLine($"  Deleted: {safeBaseName}.{maxBackups}");
    }

    // Shift all numbered files up by one
    for (var i = maxBackups - 1; i >= 1; i--)
    {
        var src = Path.Join(baseDir, $"{safeBaseName}.{i}");
        var dst = Path.Join(baseDir, $"{safeBaseName}.{i + 1}");

        if (File.Exists(src))
        {
            File.Move(src, dst);
            Console.WriteLine($"  Renamed: {safeBaseName}.{i} -> {safeBaseName}.{i + 1}");
        }
    }

    // Rename current log to .1
    var current = Path.Join(baseDir, safeBaseName);
    var first = Path.Join(baseDir, $"{safeBaseName}.1");

    if (File.Exists(current))
    {
        File.Move(current, first);
        Console.WriteLine($"  Renamed: {safeBaseName} -> {safeBaseName}.1");
    }

    // Create empty file first (like real log frameworks do), so LogExpert detects
    // newSize < oldSize and triggers ShiftBuffers()
    File.Create(current).Dispose();
    Console.WriteLine($"  Created: {safeBaseName} (empty - triggers rollover detection)");

    PrintFiles();

    // Wait for LogExpert's poll interval to detect the smaller file, then write content
    Console.WriteLine("  Waiting 2s for LogExpert to detect rollover...");
    Thread.Sleep(2000);

    WriteLogFile(current, maxBackups + rotationCount);
    Console.WriteLine($"  Wrote content to {safeBaseName}");

    PrintFiles();
    Console.WriteLine("\nPress ENTER for next rotation, A to append a live line, Q to quit.");
}

void WriteLogFile(string path, int fileId)
{
    using var writer = new StreamWriter(path, false, Encoding.UTF8);

    for (var i = 1; i <= linesPerFile; i++)
    {
        writer.WriteLine(BuildLine(fileId, i, Path.GetFileName(path)));
    }
}

void AppendLiveLine(string path)
{
    var name = Path.GetFileName(path);
    using var writer = new StreamWriter(path, append: true, Encoding.UTF8);
    var line = BuildLine(fileId: 999, lineIndex: (int)(DateTime.Now.Ticks & 0xFFF), fileName: name);
    writer.WriteLine(line);
    Console.WriteLine($"  Appended live line to {name} ({new FileInfo(path).Length} bytes total)");
}

string BuildLine(int fileId, int lineIndex, string fileName)
{
    var baseText = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] File#{fileId:D3} Line {lineIndex:D3} - {fileName} - Sample log message";

    if (!includeControlChars)
    {
        return baseText;
    }

    // Embed three control characters that rotate through the full set as the
    // line index advances, so every line looks slightly different and over a
    // run all 31 displayable codepoints (LF/CR excluded) appear.
    var n = controlChars.Length;
    var c1 = controlChars[lineIndex % n];
    var c2 = controlChars[(lineIndex + 7) % n];
    var c3 = controlChars[(lineIndex + 13) % n];

    return $"{baseText} | ctrl:[{c1}{c2}{c3}] payload<{c1}field1{c2}field2{c3}field3>";
}

void PrintFiles()
{
    Console.WriteLine("\nCurrent files on disk:");

    foreach (var f in Directory.GetFiles(baseDir, $"{safeBaseName}*").OrderBy(f => f))
    {
        Console.WriteLine($"  {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
    }
}
