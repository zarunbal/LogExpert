using System.Text;

using LogExpert.Core.Classes.Log;
using LogExpert.Core.Entities;

namespace LogExpert.Benchmarks;

/// <summary>
/// Quick test to verify PositionAwareStreamReaderPipelineNew works correctly
/// before running full benchmarks
/// </summary>
public class QuickPipelineTest
{
    public static void Run ()
    {
        Console.WriteLine("Testing PositionAwareStreamReaderPipelineNew...");

        // Generate test data
        var sb = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            sb.AppendLine($"Line {i}: This is a test line with some content");
        }
        var testData = Encoding.UTF8.GetBytes(sb.ToString());

        try
        {
            // Test 1: Read all lines
            Console.WriteLine("\nTest 1: Reading all lines...");
            using (var stream = new MemoryStream(testData))
            using (var reader = new PositionAwareStreamReaderPipelineNew(stream, new EncodingOptions(), 10000))
            {
                int lineCount = 0;
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
                Console.WriteLine($"✓ Read {lineCount} lines");
            }

            // Test 2: Memory API
            Console.WriteLine("\nTest 2: Testing memory API...");
            using (var stream = new MemoryStream(testData))
            using (var reader = new PositionAwareStreamReaderPipelineNew(stream, new EncodingOptions(), 10000))
            {
                int lineCount = 0;
                while (reader.TryReadLine(out var lineMemory))
                {
                    lineCount++;
                    // Verify we can access the memory
                    _ = lineMemory.Length;
                }
                Console.WriteLine($"✓ Read {lineCount} lines via memory API");
            }

            // Test 3: Position seeking
            Console.WriteLine("\nTest 3: Testing position seeking...");
            using (var stream = new MemoryStream(testData))
            using (var reader = new PositionAwareStreamReaderPipelineNew(stream, new EncodingOptions(), 10000))
            {
                // Read first 10 lines
                for (int i = 0; i < 10; i++)
                {
                    _ = reader.ReadLine();
                }

                // Seek back to beginning
                reader.Position = 0;

                // Read all lines again
                int lineCount = 0;
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
                Console.WriteLine($"✓ Seek and read {lineCount} lines");
            }

            Console.WriteLine("\n✅ All tests passed!");
            Console.WriteLine("\nReady to run benchmarks:");
            Console.WriteLine("  cd LogExpert.Benchmarks");
            Console.WriteLine("  dotnet run -c Release --filter \"*Pipeline*\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace:\n{ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}
