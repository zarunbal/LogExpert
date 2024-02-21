using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using LogExpert.Classes.DateTimeParser;
using NUnit.Framework.Legacy;

namespace LogExpert.Tests
{
    [TestFixture]
    public class DateFormatParserTest
    {
        [Test]
        public void CanParseAllCultures()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            foreach (var culture in cultures)
            {
                if (culture.Name == "dz" || culture.Name.StartsWith("dz-"))
                {
                    Console.WriteLine("The dz (Dzongkha) time format is not supported yet.");
                    continue;
                }

                var datePattern = GetDateAndTimeFormat(culture);
                var message = $"Culture: {culture.Name} ({culture.EnglishName} {datePattern})";
                var sections = Parser.ParseSections(datePattern, out bool syntaxError);

                ClassicAssert.IsFalse(syntaxError, message);

                var dateSection = sections.FirstOrDefault();
                ClassicAssert.IsNotNull(dateSection, message);

                var now = DateTime.Now;
                var expectedFormattedDate = now.ToString(datePattern);
                var actualFormattedDate = now.ToString(string.Join("", dateSection.GeneralTextDateDurationParts));
                ClassicAssert.AreEqual(expectedFormattedDate, actualFormattedDate, message);
            }
        }

        [Test]
        [TestCase("en-US", "MM", "dd", "yyyy", "hh", "mm", "ss", "tt")]
        [TestCase("fr-FR", "dd", "MM", "yyyy", "HH", "mm", "ss")]
        [TestCase("de-DE", "dd", "MM", "yyyy", "HH", "mm", "ss")]
        [TestCase("ar-TN", "dd", "MM", "yyyy", "HH", "mm", "ss")]
        [TestCase("as", "dd", "MM", "yyyy", "tt", "hh", "mm", "ss")]
        [TestCase("bg", "dd", "MM", "yyyy", "HH", "mm", "ss")]
        public void TestDateFormatParserFromCulture(string cultureInfoName, params string[] expectedDateParts)
        {
            var culture = CultureInfo.GetCultureInfo(cultureInfoName);

            var datePattern = GetDateAndTimeFormat(culture);

            var sections = Parser.ParseSections(datePattern, out bool syntaxError);

            var message = $"Culture: {culture.EnglishName}, Actual date pattern: {datePattern}";

            ClassicAssert.IsFalse(syntaxError, message);

            var dateSection = sections.FirstOrDefault();
            ClassicAssert.IsNotNull(dateSection);

            var dateParts = dateSection
                .GeneralTextDateDurationParts
                .Where(Token.IsDatePart)
                .Select(p => DateFormatPartAdjuster.AdjustDateTimeFormatPart(p))
                .ToArray();

            ClassicAssert.AreEqual(expectedDateParts.Length, dateParts.Length, message);

            for (var i = 0; i < expectedDateParts.Length; i++)
            {
                var expected = expectedDateParts[i];
                var actual = dateParts[i];
                ClassicAssert.AreEqual(expected, actual, message);
            }
        }

        private string GetDateAndTimeFormat(CultureInfo culture)
        {
            return string.Concat(
                culture.DateTimeFormat.ShortDatePattern,
                " ",
                culture.DateTimeFormat.LongTimePattern
            );
        }
    }
}