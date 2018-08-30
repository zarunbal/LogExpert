
using NUnit.Framework;
using System;
using System.IO;

namespace LogExpert
{
    [TestFixture]
    public class SquareBracketColumnizerTest
    {
        [TestCase(@".\TestData\SquareBracketColumnizerTest_01.txt", 5)]
        [TestCase(@".\TestData\SquareBracketColumnizerTest_02.txt", 5)]
        [TestCase(@".\TestData\SquareBracketColumnizerTest_03.txt", 6)]
        public void Prepare_HappyFile_ReturnColumnCount(string fileName, int count)
        {
            SquareBracketColumnizer squareBracketColumnizer = new SquareBracketColumnizer();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            squareBracketColumnizer.Prepare(path);
            Assert.AreEqual(squareBracketColumnizer.GetColumnCount(), count);
        }

    }
}