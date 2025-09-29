using LogExpert.Core.Classes;

using NUnit.Framework;

namespace LogExpert.Core.UnitTests.Classes;

[TestFixture]
public class QualityInfoTests
{
    [Test]
    public void Constructor_DefaultValues_InitializesCorrectly()
    {
        var qualityInfo = new QualityInfo();

        Assert.That(qualityInfo.Quality, Is.EqualTo(0));
    }

    [Test]
    public void Quality_SetAndGet_ReturnsCorrectValue()
    {
        var qualityInfo = new QualityInfo();
        const int expectedQuality = 85;

        qualityInfo.Quality = expectedQuality;

        Assert.That(qualityInfo.Quality, Is.EqualTo(expectedQuality));
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(150)]
    public void Quality_VariousValues_StoresCorrectly(int quality)
    {
        var qualityInfo = new QualityInfo { Quality = quality };

        Assert.That(qualityInfo.Quality, Is.EqualTo(quality));
    }
}