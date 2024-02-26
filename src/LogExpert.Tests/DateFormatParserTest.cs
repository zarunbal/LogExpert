using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using LogExpert.Classes.DateTimeParser;

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

                Assert.That(syntaxError, Is.False, message);

                var dateSection = sections.FirstOrDefault();
                Assert.That(dateSection, Is.Not.Null, message);

                var now = DateTime.Now;
                var expectedFormattedDate = now.ToString(datePattern);
                var actualFormattedDate = now.ToString(string.Join("", dateSection.GeneralTextDateDurationParts));
                Assert.That(actualFormattedDate, Is.EqualTo(expectedFormattedDate), message);
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

            Assert.That(syntaxError, Is.False, message);

            var dateSection = sections.FirstOrDefault();
            Assert.That(dateSection, Is.Not.Null);

            var dateParts = dateSection
                .GeneralTextDateDurationParts
                .Where(Token.IsDatePart)
                .Select(p => DateFormatPartAdjuster.AdjustDateTimeFormatPart(p))
                .ToArray();

            Assert.That(dateParts.Length, Is.EqualTo(expectedDateParts.Length), message);

            for (var i = 0; i < expectedDateParts.Length; i++)
            {
                var expected = expectedDateParts[i];
                var actual = dateParts[i];
                Assert.That(actual, Is.EqualTo(expected), message);
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