using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sparrow.Core.Tests
{
    [TestClass]
    public class BinarySequenceHashBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenRawSequenceNull()
        {
            new BinarySequenceHashBuilder(null);
        }

        [TestMethod]
        public void TokenCountIncreasedWhenTokenMarked()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();

            Assert.AreEqual(0, hash.SequenceLength);

            hash.AppendZeroToSequence();
            Assert.AreEqual(1, hash.SequenceLength);

            hash.AppendOneSequence();
            Assert.AreEqual(2, hash.SequenceLength);
        }

        [TestMethod]
        public void BitIs0WhenTokenIsAlpha()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();

            hash.AppendZeroToSequence();

            Assert.AreEqual(0, hash.GetBytes()[0]);
        }

        [TestMethod]
        public void BitIs1WhenTokenIsNumber()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();

            hash.AppendOneSequence();

            Assert.AreEqual(1, hash.GetBytes()[0]);
        }

        [TestMethod]
        public void BitsMarkedInSequenceWhenMultipleTokensSet()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();

            hash.AppendOneSequence();
            hash.AppendZeroToSequence();
            hash.AppendOneSequence();
            hash.AppendZeroToSequence();
            
            byte[] bytes = hash.GetBytes();

            Assert.AreEqual(10 /* think binary flags */, bytes[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MarkAlphaWhenMaxNumberOfMarkersReached()
        {
            byte[] sequence = new byte[BinarySequenceHashBuilder.SEQUENCE_BYTE_LENGTH];
            sequence[BinarySequenceHashBuilder.SEQUENCE_BYTE_LENGTH - 1] = Byte.MaxValue;

            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder(sequence);

            hash.AppendZeroToSequence();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MarkNumberWhenMaxNumberOfMarkersReached()
        {
            byte[] sequence = new byte[BinarySequenceHashBuilder.SEQUENCE_BYTE_LENGTH];
            sequence[BinarySequenceHashBuilder.SEQUENCE_BYTE_LENGTH - 1] = Byte.MaxValue;

            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder(sequence);

            hash.AppendZeroToSequence();
        }

        [TestMethod]
        public void SanityCheckBitsMarkedWhenAllBitsUsed()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();
            int lastMarkerIndex = BinarySequenceHashBuilder.SEQUENCE_BYTE_LENGTH - 2;

            for (int i = 0; i < Byte.MaxValue; i++)
            {
                hash.AppendOneSequence();
            }

            byte[] bytes = hash.GetBytes();

            for (int i = 0; i < lastMarkerIndex; i++)
            {
                if (bytes[i] != 0xFF)
                {
                    Assert.Fail();
                    break;
                }
            }

            Assert.AreEqual(127, bytes[lastMarkerIndex]);
        }

        [TestMethod]
        public void HashesAreEqualWhenEquivalentHashesAreCompared()
        {
            BinarySequenceHashBuilder hash1 = new BinarySequenceHashBuilder();

            for (int i = 0; i < Byte.MaxValue; i++)
            {
                if (i % 3 == 0)
                {
                    hash1.AppendOneSequence();
                }
                else if (i % 2 == 0)
                {
                    hash1.AppendZeroToSequence();
                }
            }

            BinarySequenceHashBuilder hash2 = new BinarySequenceHashBuilder(hash1.GetBytes());

            Assert.IsTrue(hash1 == hash2, "==");
            Assert.IsTrue(hash1.Equals(hash2), ".Equals");
        }

        [TestMethod]
        public void HashesNotEqualWhenOtherHashNull()
        {
            BinarySequenceHashBuilder hash1 = new BinarySequenceHashBuilder();

            Assert.IsFalse(hash1 == null, "==");
            Assert.IsFalse(null == hash1, "==");
            Assert.IsFalse(hash1.Equals(null));
        }

        [TestMethod]
        public void HashesNotEqualWhenBitsDoNotMatch()
        {
            BinarySequenceHashBuilder hash1 = new BinarySequenceHashBuilder();
            BinarySequenceHashBuilder hash2 = new BinarySequenceHashBuilder();

            hash1.AppendOneSequence();

            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void HashInCorrectStringFormatWhenToStringInvoked()
        {
            BinarySequenceHashBuilder hash = new BinarySequenceHashBuilder();

            hash.AppendOneSequence();
            hash.AppendZeroToSequence();
            hash.AppendOneSequence();
            hash.AppendZeroToSequence();

            hash.AppendOneSequence();
            hash.AppendZeroToSequence();
            hash.AppendOneSequence();
            hash.AppendZeroToSequence();

            Assert.AreEqual("AA0000000000000000000000000000000000000000000000000000000000000008",
                            hash.ToString());
        }
    }
}
