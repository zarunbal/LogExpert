using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LogExpert.Tests.Extensions
{
    [TestFixture]
    public class EnumerableTests
    {
        [Test]
        public void Extensions_IsEmpty_NullArray()
        {
            object[] arrayObject = null;

            Assert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_EmptyArray()
        {
            object[] arrayObject = new object[0];

            Assert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_FilledArray()
        {
            object[] arrayObject = new[] {new object()};

            Assert.IsFalse(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_NullIEnumerable()
        {
            IEnumerable<object> arrayObject = null;

            Assert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_EmptyIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>();

            Assert.IsTrue(arrayObject.IsEmpty());
        }

        [Test]
        public void Extensions_IsEmpty_FilledIEnumerable()
        {
            IEnumerable<object> arrayObject = new List<object>(new []{new object()});

            Assert.IsFalse(arrayObject.IsEmpty());
        }
    }
}