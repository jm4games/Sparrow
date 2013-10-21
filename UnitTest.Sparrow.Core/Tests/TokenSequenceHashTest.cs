using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sparrow.Core.Tests
{
    [TestClass]
    public class TokenSequenceHashTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenRawSequenceNull()
        {
            new TokenSequenceHash(null);
        }

        [TestMethod]
        public void TokenCountIncreasedWhenTokenMarked()
        {
            TokenSequenceHash hash = new TokenSequenceHash();

            Assert.AreEqual(0, hash.TokenCount);

            hash.MarkNextTokenAsAlpha();
            Assert.AreEqual(1, hash.TokenCount);

            hash.MarkNextTokenAsNumber();
            Assert.AreEqual(2, hash.TokenCount);
        }

        [TestMethod]
        public void BitIs0WhenTokenIsAlpha()
        {
            TokenSequenceHash hash = new TokenSequenceHash();

            hash.MarkNextTokenAsAlpha();

            Assert.AreEqual(0, hash.GetBytes()[0]);
        }

        [TestMethod]
        public void BitIs1WhenTokenIsNumber()
        {
            TokenSequenceHash hash = new TokenSequenceHash();

            hash.MarkNextTokenAsNumber();

            Assert.AreEqual(1, hash.GetBytes()[0]);
        }

        [TestMethod]
        public void BitsMarkedInSequenceWhenMultipleTokensSet()
        {
            TokenSequenceHash hash = new TokenSequenceHash();

            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();
            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();
            
            byte[] bytes = hash.GetBytes();

            Assert.AreEqual(10 /* think binary flags */, bytes[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MarkAlphaWhenMaxNumberOfMarkersReached()
        {
            byte[] sequence = new byte[TokenSequenceHash.SEQUENCE_LENGTH];
            sequence[TokenSequenceHash.SEQUENCE_LENGTH - 1] = Byte.MaxValue;

            TokenSequenceHash hash = new TokenSequenceHash(sequence);

            hash.MarkNextTokenAsAlpha();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MarkNumberWhenMaxNumberOfMarkersReached()
        {
            byte[] sequence = new byte[TokenSequenceHash.SEQUENCE_LENGTH];
            sequence[TokenSequenceHash.SEQUENCE_LENGTH - 1] = Byte.MaxValue;

            TokenSequenceHash hash = new TokenSequenceHash(sequence);

            hash.MarkNextTokenAsAlpha();
        }

        [TestMethod]
        public void SanityCheckBitsMarkedWhenAllBitsUsed()
        {
            TokenSequenceHash hash = new TokenSequenceHash();
            int lastMarkerIndex = TokenSequenceHash.SEQUENCE_LENGTH - 2;

            for (int i = 0; i < Byte.MaxValue; i++)
            {
                hash.MarkNextTokenAsNumber();
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
            TokenSequenceHash hash1 = new TokenSequenceHash();

            for (int i = 0; i < Byte.MaxValue; i++)
            {
                if (i % 3 == 0)
                {
                    hash1.MarkNextTokenAsNumber();
                }
                else if (i % 2 == 0)
                {
                    hash1.MarkNextTokenAsAlpha();
                }
            }

            TokenSequenceHash hash2 = new TokenSequenceHash(hash1.GetBytes());

            Assert.IsTrue(hash1 == hash2, "==");
            Assert.IsTrue(hash1.Equals(hash2), ".Equals");
        }

        [TestMethod]
        public void HashesNotEqualWhenOtherHashNull()
        {
            TokenSequenceHash hash1 = new TokenSequenceHash();

            Assert.IsFalse(hash1 == null, "==");
            Assert.IsFalse(null == hash1, "==");
            Assert.IsFalse(hash1.Equals(null));
        }

        [TestMethod]
        public void HashesNotEqualWhenBitsDoNotMatch()
        {
            TokenSequenceHash hash1 = new TokenSequenceHash();
            TokenSequenceHash hash2 = new TokenSequenceHash();

            hash1.MarkNextTokenAsNumber();

            Assert.AreNotEqual(hash1, hash2);
        }

        [TestMethod]
        public void HashInCorrectStringFormatWhenToStringInvoked()
        {
            TokenSequenceHash hash = new TokenSequenceHash();

            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();
            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();

            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();
            hash.MarkNextTokenAsNumber();
            hash.MarkNextTokenAsAlpha();

            Assert.AreEqual("AA0000000000000000000000000000000000000000000000000000000000000008",
                            hash.ToString());
        }
    }
}
