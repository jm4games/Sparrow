using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sparrow.Core
{
    [TestClass]
    public class AbstractStringMaskTests
    {
        private enum TestMask
        {
            Default,
            M1,
            M2,
            M3
        }

        private static Dictionary<TestMask, string> masks = new Dictionary<TestMask, string>()
        {
            {TestMask.Default, "%d"},
            {TestMask.M1, "%a"},
            {TestMask.M2, "%b"}
            /* M3 should not be specified */
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenTokenizerNull()
        {            
            new AbstractStringMask<TestMask>(null, null, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenMasksNull()
        {
            new AbstractStringMask<TestMask>(new AbstractStringTokenizer("Test"), null, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorThrowsWhenDefaultMaskStringNotDefined()
        {
            new AbstractStringMask<TestMask>(new AbstractStringTokenizer("Test"), new Dictionary<TestMask, string>(), TestMask.M1);
        }

        [TestMethod]
        public void TaskMappingDefaultSetWhenMaskCreated()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test 21!");
            TestMask[] mappings = mask.GetMaskMappings();

            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i] != TestMask.Default)
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod]
        public void MaskSetWhenSet()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(0, TestMask.M1);

            Assert.AreEqual(TestMask.M1, mask.GetMaskMappings()[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenSettingMaskOfNegativeIndex()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(-1, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenSettingMaskOfIndexGreaterThenNumberOfTokens()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(mask.GetMaskMappings().Length + 1, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionThrownWhenSettingMaskWithNoStringDefined()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(0, TestMask.M3);
        }

        [TestMethod]
        public void MaskedStringHasDefaultMasksWhenGeneratedWithNoMaskMappingSet()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            Assert.AreEqual("%d - %d!", mask.MaskedString);
        }

        [TestMethod]
        public void MaskedStringHasSetMaskedValuesWhenValueSet()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M2);

            Assert.AreEqual("%a - %b!", mask.MaskedString);
        }

        [TestMethod]
        public void MaskedValueHasOrignalTokenMarkerWhenEscapedByTokenizer()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test % 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M2);

            Assert.AreEqual("%a % %b!", mask.MaskedString);
        }

        [TestMethod]
        public void FirstMaskValueReturnedWhenMultipleValuesHaveSameMask()
        {
            const string value = "Test";
            AbstractStringMask<TestMask> mask = CreateMask(value + " - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M1);

            Assert.AreEqual(value, mask.GetFirstMaskValue(TestMask.M1));
        }

        [TestMethod]
        public void EntireMaskValueReturnedWhenMultipleValuesHaveSameMask()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M1);

            Assert.AreEqual("Test 21", mask.GetMaskValue(TestMask.M1, " "));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionThrownWhenMaskValueDelimiterNull()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            mask.GetMaskValue(TestMask.Default, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionThrownWhenTokenIndexListNullWhenSettingTokenMask()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(null, TestMask.M1);
        }

        [TestMethod]
        public void MultipleTokenIndexSetWhenSetMaskTokenUsesList()
        {
            AbstractStringMask<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(new List<int>() { 0, 1 }, TestMask.M1);

            TestMask[] mappings = mask.GetMaskMappings();
            
            for (int i = 0; i < 2; i++)
            {
                if (mappings[i] != TestMask.M1)
                {
                    Assert.Fail();
                }
            }
        }

        private AbstractStringMask<TestMask> CreateMask(string valueToTokenize)
        {
            return new AbstractStringMask<TestMask>(new AbstractStringTokenizer(valueToTokenize), masks, TestMask.Default);
        }
    }
}
