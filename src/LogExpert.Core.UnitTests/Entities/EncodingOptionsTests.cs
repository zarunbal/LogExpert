using LogExpert.Core.Entities;

using NUnit.Framework;

using System.Text;

namespace LogExpert.Core.UnitTests.Entities;

[TestFixture]
public class EncodingOptionsTests
{
    [Test]
    public void Constructor_Default_InitializesWithNullValues()
    {
        var options = new EncodingOptions();

        Assert.That(options.Encoding, Is.Null);
        Assert.That(options.DefaultEncoding, Is.Null);
    }

    [Test]
    public void Encoding_SetAndGet_WorksCorrectly()
    {
        var options = new EncodingOptions();
        var encoding = Encoding.UTF8;

        options.Encoding = encoding;

        Assert.That(options.Encoding, Is.EqualTo(encoding));
    }

    [Test]
    public void DefaultEncoding_SetAndGet_WorksCorrectly()
    {
        var options = new EncodingOptions();
        var encoding = Encoding.ASCII;

        options.DefaultEncoding = encoding;

        Assert.That(options.DefaultEncoding, Is.EqualTo(encoding));
    }

    [Test]
    public void BothEncodings_SetDifferentValues_StoreIndependently()
    {
        var options = new EncodingOptions();
        var utf8 = Encoding.UTF8;
        var ascii = Encoding.ASCII;

        options.Encoding = utf8;
        options.DefaultEncoding = ascii;

        Assert.That(options.Encoding, Is.EqualTo(utf8));
        Assert.That(options.DefaultEncoding, Is.EqualTo(ascii));
        Assert.That(options.Encoding, Is.Not.EqualTo(options.DefaultEncoding));
    }

    [Test]
    public void Encoding_SetToNull_AllowsNullValue()
    {
        var options = new EncodingOptions
        {
            Encoding = Encoding.UTF8
        };

        options.Encoding = null;

        Assert.That(options.Encoding, Is.Null);
    }

    [Test]
    public void DefaultEncoding_SetToNull_AllowsNullValue()
    {
        var options = new EncodingOptions
        {
            DefaultEncoding = Encoding.UTF8
        };

        options.DefaultEncoding = null;

        Assert.That(options.DefaultEncoding, Is.Null);
    }
}