using System;
using System.Collections.Generic;
using LogExpert.Extensions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LogExpert.Tests.Extensions
{
    [TestFixture]
    public class EnumerableTests
    {
        [Test]
        public void Extensions_IsEmpty_NullArray()
        {
            object[] arrayObject = null;

            ClassicAssert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_EmptyArray()
        {
            object[] arrayObject = Array.Empty<object>();

            ClassicAssert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_FilledArray()
        {
            object[] arrayObject = {new object()};

            ClassicAssert.IsFalse(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_NullIEnumerable()
        {
            IEnumerable<object> arrayObject = null;

            ClassicAssert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_EmptyIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>();

            ClassicAssert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_FilledIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>(new []{new object()});

            ClassicAssert.IsFalse(arrayObject.IsEmpty());
        }
    }
}