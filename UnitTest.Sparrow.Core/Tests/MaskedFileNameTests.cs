using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sparrow.Core
{
    [TestClass]
    public class MaskedFileNameTests
    {
        private enum TestMask
        {
            Default,
            M1,
            M2,
            M3
        }

        private static Dictionary<TestMask, MaskConfiguration> masks = new Dictionary<TestMask, MaskConfiguration>()
        {
            {TestMask.Default, new MaskConfiguration(false, "%d")},
            {TestMask.M1, new MaskConfiguration("%a")},
            {TestMask.M2, new MaskConfiguration("%b")}
            /* M3 should not be specified */
        };

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenTokenizerNull()
        {            
            new MaskedFileName<TestMask>(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorThrowsWhenMasksNull()
        {
            new MaskedFileName<TestMask>(new FileNameTokenizer("Test"), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CtorThrowsWhenDefaultMaskStringNotDefined()
        {
            new MaskedFileName<TestMask>(new FileNameTokenizer("Test"), new Dictionary<TestMask, MaskConfiguration>());
        }

        [TestMethod]
        public void TaskMappingDefaultSetWhenMaskCreated()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test 21!");
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
            MaskedFileName<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(0, TestMask.M1);

            Assert.AreEqual(TestMask.M1, mask.GetMaskMappings()[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenSettingMaskOfNegativeIndex()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(-1, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExceptionThrownWhenSettingMaskOfIndexGreaterThenNumberOfTokens()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(mask.GetMaskMappings().Length + 1, TestMask.M1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExceptionThrownWhenSettingMaskWithNoStringDefined()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test 21!");

            mask.SetTokenMask(0, TestMask.M3);
        }

        [TestMethod]
        public void MaskedStringHasDefaultMasksWhenGeneratedWithNoMaskMappingSet()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            Assert.AreEqual(String.Format("{0}{0}", masks[TestMask.Default].MaskString), mask.ToString());
        }

        [TestMethod]
        public void MaskedStringHasSetMaskedValuesWhenValueSet()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M2);

            Assert.AreEqual(masks[TestMask.M1].MaskString + masks[TestMask.M2].MaskString, mask.ToString());
        }

        [TestMethod]
        public void MaskedStringHasNoMaskMarkerWhenEscapedMarkerExist()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test % 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M2);

            Assert.AreEqual(masks[TestMask.M1].MaskString + masks[TestMask.M2].MaskString, mask.ToString());
        }

        [TestMethod]
        public void FirstMaskValueReturnedWhenMultipleValuesHaveSameMask()
        {
            const string value = "Test";
            MaskedFileName<TestMask> mask = CreateMask(value + " - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M1);

            Assert.AreEqual(value, mask.GetFirstMaskValue(TestMask.M1));
        }

        [TestMethod]
        public void EntireMaskValueReturnedWhenMultipleValuesHaveSameMask()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M1);

            Assert.AreEqual("Test 21", mask.GetMaskValue(TestMask.M1, " "));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionThrownWhenMaskValueDelimiterNull()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            mask.GetMaskValue(TestMask.Default, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionThrownWhenTokenIndexListNullWhenSettingTokenMask()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(null, TestMask.M1);
        }

        [TestMethod]
        public void MultipleTokenIndexSetWhenSetMaskTokenUsesList()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

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

        [TestMethod]
        public void TokensMergedWhenMaskMergable()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M1);

            Assert.AreEqual(masks[TestMask.M1].MaskString, mask.ToString());
        }

        [TestMethod]
        public void TokensNotMergedWhenMaskSplitsTwoOtherMasks()
        {
            MaskedFileName<TestMask> mask = CreateMask("Test - 21 - Ok!");

            mask.SetTokenMask(0, TestMask.M1);
            mask.SetTokenMask(1, TestMask.M2);
            mask.SetTokenMask(2, TestMask.M1);

            Assert.AreEqual(String.Format("{0}{1}{0}", masks[TestMask.M1].MaskString, masks[TestMask.M2].MaskString), mask.ToString());
        }

        private MaskedFileName<TestMask> CreateMask(string valueToTokenize)
        {
            return new MaskedFileName<TestMask>(new FileNameTokenizer(valueToTokenize), masks);
        }
    }
}
