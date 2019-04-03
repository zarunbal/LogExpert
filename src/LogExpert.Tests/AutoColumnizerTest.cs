using System;
using System.IO;
using NUnit.Framework;

namespace LogExpert.Tests
{
    /// <summary>
    /// Summary description for AutoColumnizerTest
    /// </summary>
    [TestFixture]
    public class AutoColumnizerTest
    {
        [TestCase(@".\TestData\SquareBracketColumnizerTest_01.txt", "Square Bracket Columnizer")]
        [TestCase(@".\TestData\SquareBracketColumnizerTest_02.txt", "Square Bracket Columnizer")]
        [TestCase(@".\TestData\SquareBracketColumnizerTest_03.txt", "Square Bracket Columnizer")]
        [TestCase(@".\TestData\SquareBracketColumnizerTest_04.txt", "Timestamp Columnizer")]
        public void FindColumnizer_HappyFile_ReturnCorrectColumnizer(string fileName, string columnizerName)
        {
            AutoColumnizer autoColumnizer = new AutoColumnizer();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            LogfileReader reader = new LogfileReader(path, new EncodingOptions(), true, 40, 50, new MultifileOptions());
            reader.ReadFiles();
            var result = autoColumnizer.FindColumnizer(path, reader);

            Assert.AreEqual(result.GetName(), columnizerName);
        }

    }
}
