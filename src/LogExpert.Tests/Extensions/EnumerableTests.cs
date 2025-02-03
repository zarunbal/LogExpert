using LogExpert.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LogExpert.Tests.Extensions
{
    [TestFixture]
    public class EnumerableTests
    {
        [Test]
        public void Extensions_IsEmpty_NullArray()
        {
            object[] arrayObject = null;

            Assert.That(arrayObject.IsEmpty(), Is.True);
        }

        [Test]
        public void Extensions_IsEmpty_EmptyArray()
        {
            object[] arrayObject = Array.Empty<object>();

            Assert.That(arrayObject.IsEmpty(), Is.True);
        }

        [Test]
        public void Extensions_IsEmpty_FilledArray()
        {
            object[] arrayObject = {new object()};

            Assert.That(!arrayObject.IsEmpty(), Is.True);
        }

        [Test]
        public void Extensions_IsEmpty_NullIEnumerable()
        {
            IEnumerable<object> arrayObject = null;

            Assert.That(arrayObject.IsEmpty(), Is.True);
        }

        [Test]
        public void Extensions_IsEmpty_EmptyIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>();

            Assert.That(arrayObject.IsEmpty(), Is.True);
        }

        [Test]
        public void Extensions_IsEmpty_FilledIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>(new []{new object()});

            Assert.That(!arrayObject.IsEmpty(), Is.True);
        }
    }
}
