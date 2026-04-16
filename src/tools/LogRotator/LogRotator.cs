using System.Text;

const string baseDir = "logs";
const string baseName = "engine.log";
const int maxBackups = 6;
const int linesPerFile = 50;

Directory.CreateDirectory(baseDir);

// Create initial set of files
Console.WriteLine($"Creating initial files in '{Path.GetFullPath(baseDir)}'...");
WriteLogFile(Path.Combine(baseDir, baseName), 0);

for (var i = 1; i <= maxBackups; i++)
{
    WriteLogFile(Path.Combine(baseDir, $"{baseName}.{i}"), i);
}

PrintFiles();
Console.WriteLine();
Console.WriteLine("Open the 'engine.log' file in LogExpert with MultiFile enabled (pattern: *$J(.))");
Console.WriteLine("Press ENTER to perform a rotation (with oldest file deletion), or Q to quit.");

var rotationCount = 0;

while (true)
{
    var key = Console.ReadKey(true);

    if (key.Key == ConsoleKey.Q)
    {
        break;
    }

    if (key.Key != ConsoleKey.Enter)
    {
        continue;
    }

    rotationCount++;
    Console.WriteLine($"\n--- Rotation #{rotationCount} ---");

    // Delete the oldest file (simulates maxBackups limit)
    var oldest = Path.Combine(baseDir, $"{baseName}.{maxBackups}");

    if (File.Exists(oldest))
    {
        File.Delete(oldest);
        Console.WriteLine($"  Deleted: {baseName}.{maxBackups}");
    }

    // Shift all numbered files up by one
    for (var i = maxBackups - 1; i >= 1; i--)
    {
        var src = Path.Combine(baseDir, $"{baseName}.{i}");
        var dst = Path.Combine(baseDir, $"{baseName}.{i + 1}");

        if (File.Exists(src))
        {
            File.Move(src, dst);
            Console.WriteLine($"  Renamed: {baseName}.{i} -> {baseName}.{i + 1}");
        }
    }

    // Rename current log to .1
    var current = Path.Combine(baseDir, baseName);
    var first = Path.Combine(baseDir, $"{baseName}.1");

    if (File.Exists(current))
    {
        File.Move(current, first);
        Console.WriteLine($"  Renamed: {baseName} -> {baseName}.1");
    }

    // Create empty file first (like real log frameworks do), so LogExpert detects
    // newSize < oldSize and triggers ShiftBuffers()
    File.Create(current).Dispose();
    Console.WriteLine($"  Created: {baseName} (empty - triggers rollover detection)");

    PrintFiles();

    // Wait for LogExpert's poll interval to detect the smaller file, then write content
    Console.WriteLine("  Waiting 2s for LogExpert to detect rollover...");
    Thread.Sleep(2000);

    WriteLogFile(current, maxBackups + rotationCount);
    Console.WriteLine($"  Wrote content to {baseName}");

    PrintFiles();
    Console.WriteLine("\nPress ENTER for next rotation, Q to quit.");
}

static void WriteLogFile(string path, int fileId)
{
    using var writer = new StreamWriter(path, false, Encoding.UTF8);

    for (var i = 1; i <= linesPerFile; i++)
    {
        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INFO] File#{fileId:D3} Line {i:D3} - {Path.GetFileName(path)} - Sample log message");
    }
}

static void PrintFiles()
{
    Console.WriteLine("\nCurrent files on disk:");

    foreach (var f in Directory.GetFiles(baseDir, $"{baseName}*").OrderBy(f => f))
    {
        Console.WriteLine($"  {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
    }
}
