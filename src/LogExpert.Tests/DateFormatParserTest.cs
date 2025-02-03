using LogExpert.Classes.DateTimeParser;

using NUnit.Framework;

using System;
using System.Globalization;
using System.Linq;

namespace LogExpert.Tests
{
    [TestFixture]
    public class DateFormatParserTest
    {
        [Test]
        public void CanParseAllCultures()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            //TODO Add Support for languages with TT (AM / PM)
            foreach (var culture in cultures)
            {
                var datePattern = GetDateAndTimeFormat(culture);

                //aa (Afar dd/MM/yyyy h:mm:ss tt)
                //af-NA (Afrikaans (Namibia) yyyy-MM-dd h:mm:ss tt)
                //ak (Akan yyyy/MM/dd h:mm:ss tt)
                //bem (Bemba dd/MM/yyyy h:mm:ss tt)
                //ceb (Cebuano M/d/yyyy h:mm:ss tt)
                //el-CY (Greek (Cyprus) d/M/yyyy h:mm:ss tt)
                //dz (Dzongkha yyyy-MM-dd ཆུ་ཚོད་h:mm:ss tt)
                //es-CO(Spanish(Colombia) d/MM/yyyy h:mm:ss tt)
                //es-DO (Spanish (Dominican Republic) d/M/yyyy h:mm:ss tt)
                //es-PA (Spanish (Panama) MM/dd/yyyy h:mm:ss tt)
                //es-PH (Spanish (Philippines) d/M/yyyy h:mm:ss tt)
                if (datePattern.Contains("tt"))
                {
                    Console.WriteLine($"The {culture.Name} time format is not supported yet.");
                    continue;
                }

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